using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;

namespace TrimAndUnifyVcfs
{
    public class OneKGenVcfProcessor
    {
        #region members

        private const int KeptColumnCount = 8;

        private readonly string _inputDirectory;
        private readonly string _smallVariantOutputFile;
        private readonly string _structuralVariantOutputFile;

        private readonly Dictionary<string, Tuple<string, bool>> _sampleInfo; //sampleID-->population,isFemale
        private readonly Dictionary<string, List<string>> _superPopulation;
        private readonly List<string> _availablePopulations;
        private readonly Dictionary<int, string> _sampleColumn;

        private static int _problematicSv;
        private readonly HashSet<Tuple<string, bool>> _svTypeSet;

        private double[] _allFrequencies;
        private int[] _altAlleleCounts;
        private int _totalAlleleNumber;
        private Dictionary<string, double[]> _originalFrequencies;
        private string _ancestralAllele;

        #endregion

        public OneKGenVcfProcessor(string inputDirectory, string smallVariantOutputFile, string structuralVariantOutputFile,
            string sampleInfoFile, string popInfoFile)
        {
            _inputDirectory              = inputDirectory;
            _smallVariantOutputFile      = smallVariantOutputFile;
            _structuralVariantOutputFile = structuralVariantOutputFile;
            _sampleInfo                  = new Dictionary<string, Tuple<string, bool>>();
            _superPopulation             = new Dictionary<string, List<string>>();
            _availablePopulations        = new List<string>();
            _sampleColumn                = new Dictionary<int, string>();
            _svTypeSet                   = new HashSet<Tuple<string, bool>>();
            
            // read sample information;
            using (var sampleReader = new StreamReader(new FileStream(sampleInfoFile, FileMode.Open)))
            {
                // skip the first line
                string line;
                sampleReader.ReadLine();

                while ((line = sampleReader.ReadLine()) != null)
                {
                    var fields = line.Split('\t');
                    var info = Tuple.Create(fields[1], fields[2].Equals("female"));
                    _sampleInfo[fields[0]] = info;
                }
            }

            // read population information
            using (var popReader = new StreamReader(new FileStream(popInfoFile, FileMode.Open)))
            {
                // skip the first line
                string line;
                popReader.ReadLine();

                while ((line = popReader.ReadLine()) != null)
                {
                    var fields = line.Split('\t');
                    var population = fields[0];
                    var superPopulation = fields[2];
                    _availablePopulations.Add(population);

                    if (_superPopulation.ContainsKey(superPopulation))
                    {
                        _superPopulation[superPopulation].Add(population);
                    }
                    else
                    {
                        _superPopulation[superPopulation] = new List<string>() { population };
                    }
                }

                // add super populations
                foreach (var key in _superPopulation.Keys)
                {
                    _availablePopulations.Add(key);
                }
            }
        }

        public void DividAndUnifyVcfFiles()
        {
            // write the header for structrual variants
            var svHeaderStringBuilder = BuildSvHeaderString();
            var vcfHeaderSb           = BuildVcfHeader();

            using (var smallVariantWriter      = new StreamWriter(new GZipStream(new FileStream(_smallVariantOutputFile, FileMode.Create), CompressionMode.Compress, false)))
            using (var structuralVariantWriter = new StreamWriter(new GZipStream(new FileStream(_structuralVariantOutputFile, FileMode.Create), CompressionMode.Compress, false)))
            {
                structuralVariantWriter.WriteLine(svHeaderStringBuilder.ToString());
                smallVariantWriter.Write(vcfHeaderSb.ToString());

                foreach (var fileName in Directory.EnumerateFiles(_inputDirectory))
                {
                    if (!fileName.EndsWith(".vcf.gz")) continue; //only work for GRCh37

                    Console.WriteLine("reading file " + fileName);

                    int smallVarCount     = 0;
                    int structualVarCount = 0;
                    _problematicSv        = 0;

                    using (var inFile = GZipUtilities.GetAppropriateStreamReader(fileName))
                    {
                        var vcfLine = inFile.ReadLine();
                        while (vcfLine != null)
                        {
                            if (vcfLine.StartsWith("##"))
                            {
                                vcfLine = inFile.ReadLine();
                                continue;
                            }

                            // read the header and record the column number for each sample
                            if (vcfLine.StartsWith("#"))
                            {
                                ReadHeader(vcfLine);

                                vcfLine = inFile.ReadLine();
                                continue;
                            }

                            var vcfFields = vcfLine.Split('\t');

                            var altAlleles = vcfFields[VcfCommon.AltIndex].Split(',');
                            var hasSymbolicAllele = altAlleles.Any(x => x.StartsWith("<") && x.EndsWith(">"));

                            if (!hasSymbolicAllele)
                            {
                                smallVarCount++;

                                // process the sample field to extract the 
                                var infoBuilder = new StringBuilder(vcfFields[VcfCommon.InfoIndex]);

                                ParseSmallVariantSamples(vcfFields, infoBuilder);

                                vcfFields[VcfCommon.InfoIndex] = infoBuilder.ToString();

                                for (var i = 0; i < KeptColumnCount; i++)
                                {
                                    smallVariantWriter.Write(vcfFields[i] + '\t');
                                }

                                smallVariantWriter.Write('\n');
                                vcfLine = inFile.ReadLine();
                                continue;
                            }

                            bool hasCnvAllele = altAlleles.Any(x => x.StartsWith("<CN") && x.EndsWith(">"));
                            var newSvLine = ParseStructuralVariatVcfLine(vcfFields, hasCnvAllele);

                            if (newSvLine != null)
                            {
                                structuralVariantWriter.WriteLine(newSvLine);
                                structualVarCount++;
                            }

                            vcfLine = inFile.ReadLine();
                        }

                        Console.WriteLine("small variants:                     {0}", smallVarCount);
                        Console.WriteLine("structural variants:                {0}", structualVarCount);
                        Console.WriteLine("problematic structural variants:    {0}", _problematicSv);
                        Console.WriteLine("--------------------------------------------------------------------------------");
                        Console.WriteLine("\n");
                    }
                }
            }

            using (var summaryWriter = new StreamWriter(new FileStream(@"E:\Data\Nirvana\bugfixes\oneKGenSvTypeSummary.txt", FileMode.Create)))
            {
                foreach (var type in _svTypeSet)
                {
                    summaryWriter.WriteLine(type.Item1 + "\t" + type.Item2);
                }
            }
        }

        private void ParseSmallVariantSamples(string[] vcfFields, StringBuilder infoBuilder)
        {
            var sampleRecorder = new Dictionary<string, int[]>(); //record AN and AC for each population 
                                                                  //get the number of alternative alleles
            var altAlleles = vcfFields[VcfCommon.AltIndex].Split(',');
            int numberOfAlts = altAlleles.Length;

            foreach (var col in _sampleColumn.Keys)
            {
                int[] alleleCounts = new int[numberOfAlts + 1]; //pos 0 for total counts
                var sampleId = _sampleColumn[col];

                var samplePopulation = _sampleInfo[sampleId].Item1;
                string sampleGenotype = GetGenotype(vcfFields[VcfCommon.FormatIndex], vcfFields[col]);

                if (!sampleRecorder.ContainsKey(samplePopulation))
                    sampleRecorder[samplePopulation] = new int[numberOfAlts + 1];

                if (sampleGenotype.Equals(".") || sampleGenotype.Equals(".|.") || sampleGenotype.Equals("./.")) continue;

                if (sampleGenotype.Equals("0/0") || sampleGenotype.Equals("0|0"))
                {
                    alleleCounts[0] += 2;
                }
                else if (sampleGenotype.Equals("0"))
                {
                    alleleCounts[0]++;
                }
                else
                {
                    if (sampleGenotype.Contains("|"))
                    {
                        var sampleAlleles = sampleGenotype.Split('|');
                        foreach (var sampleAllele in sampleAlleles)
                        {
                            if (sampleAllele.Equals("."))
                                continue;

                            alleleCounts[0]++;
                            var sampleAlleleIndex = Convert.ToInt16(sampleAllele);
                            if (sampleAlleleIndex > 0) alleleCounts[sampleAlleleIndex]++;
                        }
                    }
                    else if (sampleGenotype.Contains("/"))
                    {
                        var sampleAlleles = sampleGenotype.Split('/');
                        foreach (var sampleAllele in sampleAlleles)
                        {
                            if (sampleAllele.Equals("."))
                                continue;
                            alleleCounts[0]++;
                            var sampleAlleleIndex = Convert.ToInt16(sampleAllele);
                            if (sampleAlleleIndex > 0) alleleCounts[sampleAlleleIndex]++;
                        }
                    }
                    else
                    {
                        alleleCounts[0]++;
                        var alt = int.Parse(sampleGenotype);
                        if (alt > 0) alleleCounts[alt]++;
                    }
                }

                if (sampleRecorder.ContainsKey(samplePopulation))
                {
                    //sampleRecorder[samplePopulation][0] += (alleleCounts[0] + failedAlleleCount);
                    for (var i = 0; i <= numberOfAlts; i++)
                        sampleRecorder[samplePopulation][i] += alleleCounts[i];
                }
                else
                {
                    throw new Exception($"unknown error {vcfFields[0]} {vcfFields[1]} {vcfFields[3]}");
                }
            }

            var superpopulationSampleRecorder = new Dictionary<string, int[]>();

            foreach (var population in _availablePopulations)
            {
                if (_superPopulation.ContainsKey(population))
                {
                    int[] superPopulationAlleleCounts = new int[numberOfAlts + 1];

                    foreach (var subPopulation in _superPopulation[population])
                    {
                        for (int i = 0; i <= numberOfAlts; i++)
                            superPopulationAlleleCounts[i] += sampleRecorder[subPopulation][i];
                    }
                    superpopulationSampleRecorder[population] = superPopulationAlleleCounts;
                }

            }

            // validate the AC AN from each populations by comparing the total AN, AC and AF of each super population to info field
            bool validated = ValidateAcAndAn(superpopulationSampleRecorder, vcfFields[VcfCommon.InfoIndex], numberOfAlts);

            if (!validated)
            {
                Console.WriteLine($"Fail to Validate: {vcfFields[VcfCommon.ChromIndex]}  {vcfFields[VcfCommon.PosIndex]}  {vcfFields[VcfCommon.AltIndex]} ");
                return;
            }

            // append information to infoFiled

            infoBuilder.Clear();

            infoBuilder.Append("AN=" + _totalAlleleNumber + ";AC=" +
                               string.Join(",", _altAlleleCounts.Select(x => x.ToString()).ToArray()));
            infoBuilder.Append(";AF=" + string.Join(",", _allFrequencies.Select(x => x.ToString(JsonCommon.FrequencyRoundingFormat))) + ";AA=" +
                               _ancestralAllele);

            foreach (var population in _superPopulation.Keys)
            {
                infoBuilder.Append(";");
                infoBuilder.Append(population + "_AN=" + superpopulationSampleRecorder[population][0] + ";");
                infoBuilder.Append(population + "_AC=");

                for (int i = 1; i < numberOfAlts; i++)
                    infoBuilder.Append(superpopulationSampleRecorder[population][i] + ",");

                infoBuilder.Append(superpopulationSampleRecorder[population][numberOfAlts]);

                infoBuilder.Append(";" + population + "_AF=" +
                                   string.Join(",",
                                       _originalFrequencies[population].Select(x => x.ToString(JsonCommon.FrequencyRoundingFormat))));
            }
        }

        private bool ValidateAcAndAn(Dictionary<string, int[]> superpopulationSampleRecorder, string infoFields, int numberOfAlts)
        {
            //parse vcf info fields to extract AC, AN, and AF
            ParseInfoField(infoFields);

            int totalAn = 0;
            int[] totalAc = new int[numberOfAlts];
            foreach (var population in _superPopulation.Keys)
            {
                int currentAn = superpopulationSampleRecorder[population][0];
                totalAn += currentAn;
                for (int i = 0; i < numberOfAlts; i++)
                {
                    var currentAc = superpopulationSampleRecorder[population][i + 1];
                    totalAc[i] += currentAc;

                    //check if Frequencies match for each allele for only autosomes

                    var currentFreq = currentAn == 0 ? 0 : currentAc / (double)currentAn;

                    _originalFrequencies[population][i] = currentFreq;
                }
            }

            if (totalAn != _totalAlleleNumber) return false;

            for (var i = 0; i < numberOfAlts; i++)
            {
                if (totalAc[i] != _altAlleleCounts[i]) return false;

                var currentTotalFreq = totalAc[i] / (double)totalAn;
                if (Math.Abs(currentTotalFreq - _allFrequencies[i]) > 0.00001) return false;
            }

            return true;
        }

        private void ClearInfo()
        {
            _allFrequencies      = null;
            _altAlleleCounts     = null;
            _totalAlleleNumber   = 0;
            _originalFrequencies = new Dictionary<string, double[]>();
            _ancestralAllele     = null;
        }

        private void ParseInfoField(string infoFields)
        {
            ClearInfo();
            if (infoFields == "" || infoFields == ".") return;

            var infoItems = infoFields.Split(';');
            foreach (var infoItem in infoItems)
            {
                var infoKeyValue = infoItem.Split('=');
                if (infoKeyValue.Length == 2) //sanity check
                {
                    var key = infoKeyValue[0];
                    var value = infoKeyValue[1];

                    switch (key)
                    {
                        case "AC":
                            _altAlleleCounts = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
                            break;
                        case "EAS_AF":
                            _originalFrequencies["EAS"] = value.Split(',').Select(Convert.ToDouble).ToArray();
                            break;
                        case "AMR_AF":
                            _originalFrequencies["AMR"] = value.Split(',').Select(Convert.ToDouble).ToArray();
                            break;
                        case "AFR_AF":
                            _originalFrequencies["AFR"] = value.Split(',').Select(Convert.ToDouble).ToArray();
                            break;
                        case "EUR_AF":
                            _originalFrequencies["EUR"] = value.Split(',').Select(Convert.ToDouble).ToArray();
                            break;
                        case "SAS_AF":
                            _originalFrequencies["SAS"] = value.Split(',').Select(Convert.ToDouble).ToArray();
                            break;
                        case "AF":
                            _allFrequencies = value.Split(',').Select(Convert.ToDouble).ToArray();
                            break;
                        case "AN":
                            _totalAlleleNumber = Convert.ToInt32(value);
                            break;
                        case "AA":
                            _ancestralAllele = value;
                            break;
                    }

                }
            }
        }

        private static StringBuilder BuildVcfHeader()
        {
            var headerSb = new StringBuilder("##fileformat=VCFv4.1" + "\n");
            headerSb.Append(
                "##INFO=<ID=AC,Number=A,Type=Integer,Description=\"Total number of alternate alleles in called genotypes\">" + "\n");
            headerSb.Append("##INFO=<ID=AF,Number=A,Type=Float,Description=\"Estimated allele frequency in the range(0, 1)\">" + "\n");
            headerSb.Append("##INFO=<ID=AN,Number=1,Type=Integer,Description=\"Total number of alleles in called genotypes\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=EAS_AF,Number=A,Type=Float,Description=\"Allele frequency in the EAS populations calculated from AC and AN, in the range(0, 1)\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=EUR_AF,Number=A,Type=Float,Description=\"Allele frequency in the EUR populations calculated from AC and AN, in the range (0,1)\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=AFR_AF,Number=A,Type=Float,Description=\"Allele frequency in the AFR populations calculated from AC and AN, in the range (0,1)\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=AMR_AF,Number=A,Type=Float,Description=\"Allele frequency in the AMR populations calculated from AC and AN, in the range (0,1)\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=SAS_AF,Number=A,Type=Float,Description=\"Allele frequency in the SAS populations calculated from AC and AN, in the range (0,1)\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=EAS_AC,Number=A,Type=Integer,Description=\"Total number of alternate alleles in called genotypes in the EAS populations\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=EUR_AC,Number=A,Type=Integer,Description=\"Total number of alternate alleles in called genotypes in the EUR populations\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=AFR_AC,Number=A,Type=Integer,Description=\"Total number of alternate alleles in called genotypes in the AFR populations\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=AMR_AC,Number=A,Type=Integer,Description=\"Total number of alternate alleles in called genotypes in the AMR populations\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=SAS_AC,Number=A,Type=Integer,Description=\"Total number of alternate alleles in called genotypes in the SAS populations\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=EAS_AN,Number=A,Type=Integer,Description=\"Total number of alleles in called genotypes in the EAS populations\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=EUR_AN,Number=A,Type=Integer,Description=\"Total number of alleles in called genotypes in the EUR populations\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=AFR_AN,Number=A,Type=Integer,Description=\"Total number of alleles in called genotypes in the AFR populations\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=AMR_AN,Number=A,Type=Integer,Description=\"Total number of alleles in called genotypes in the AMR populations\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=SAS_AN,Number=A,Type=Integer,Description=\"Total number of alleles in called genotypes in the SAS populations\">" + "\n");
            headerSb.Append(
                "##INFO=<ID=AA,Number=1,Type=String,Description=\"Ancestral Allele. Format: AA|REF|ALT|IndelType. AA: Ancestral allele, REF:Reference Allele, ALT:Alternate Allele, IndelType:Type of Indel (REF, ALT and IndelType are only defined for indels)\">" + "\n");

            headerSb.Append("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO" + "\n");

            return headerSb;

        }

        private StringBuilder BuildSvHeaderString()
        {
            StringBuilder headerStringBuilder =
                new StringBuilder("#Id\tChr\tStart\tEnd\tSvType\tSampleSize\tObservedGains\tObservedLosses\tVariantFreqAll");
            foreach (var population in _availablePopulations)
            {
                headerStringBuilder.Append("\t");
                headerStringBuilder.Append($"{population}_size");
                headerStringBuilder.Append("\t");
                headerStringBuilder.Append($"{population}_Freq");
            }
            return headerStringBuilder;
        }

        private void ReadHeader(string vcfLine)
        {
            _sampleColumn.Clear();

            var fields = vcfLine.Split('\t');

            for (var i = 0; i < fields.Length; i++)
            {
                if (_sampleInfo.ContainsKey(fields[i]))
                {
                    _sampleColumn[i] = fields[i];
                }
                else if (i > 9)
                {
                    throw new Exception($"unknown sample {fields[i]}");
                }
            }
        }

        private string ParseStructuralVariatVcfLine(string[] vcfFields, bool isCnv)
        {
            var outStringBuilder = new StringBuilder(vcfFields[VcfCommon.IdIndex] + "\t" + vcfFields[VcfCommon.ChromIndex]);

            // add the start, shoud be pos+1 
            int start = Convert.ToInt32(vcfFields[VcfCommon.PosIndex]) + 1;
            outStringBuilder.Append("\t" + start);

            // decide the type of variants
            string svType;
            int? svEnd;

            ParseSvInfoField(vcfFields[VcfCommon.InfoIndex], out svType, out svEnd);

            _svTypeSet.Add(Tuple.Create(svType, isCnv));

            if (svEnd == null)
            {
                _problematicSv++;
                return null;
            }
            outStringBuilder.Append("\t" + svEnd);

            // add svType
            outStringBuilder.Append("\t" + svType);

            try
            {
                ParseStructuralVarSamples(vcfFields, outStringBuilder, isCnv);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(outStringBuilder.ToString());
            }

            return outStringBuilder.ToString();
        }

        private void ParseStructuralVarSamples(string[] vcfFields, StringBuilder sb, bool isCnv)
        {
            List<int> hapCopyNumbers = new List<int>() { 1 };
            if (isCnv)
            {
                //parse the copynumber
                var altAlleles = vcfFields[VcfCommon.AltIndex].Split(',');

                foreach (var altallele in altAlleles)
                {
                    var match = Regex.Match(altallele, @"<CN(\d+)>");
                    var copyNum = Convert.ToInt32(match.Groups[1].ToString());
                    hapCopyNumbers.Add(copyNum);
                }
            }
            else
            {
                if (vcfFields[VcfCommon.AltIndex].Contains(","))
                {
                    Console.WriteLine($"multiple sv alleles: {vcfFields[VcfCommon.ChromIndex]} {vcfFields[VcfCommon.PosIndex]} {vcfFields[VcfCommon.RefIndex]} {vcfFields[VcfCommon.AltIndex]}");
                }
            }

            // parse the sample information
            int totalSampleSize = 0;
            int nonRefSample    = 0;
            int observedGains   = 0;
            int observedLosses  = 0;
            var sampleRecorder  = new Dictionary<string, int[]>();

            foreach (var col in _sampleColumn.Keys)
            {
                var sampleId          = _sampleColumn[col];
                var samplePopulation  = _sampleInfo[sampleId].Item1;
                var isSampleFemale    = _sampleInfo[sampleId].Item2;
                string sampleGenotype = GetGenotype(vcfFields[VcfCommon.FormatIndex], vcfFields[col]);

                if (sampleGenotype.Contains("."))
                {
                    if (!sampleGenotype.Equals(".") || !sampleGenotype.Equals(".|.") || !sampleGenotype.Equals("./."))
                    {
                        Console.WriteLine($"unexpected genotype {sampleGenotype} at  {vcfFields[VcfCommon.ChromIndex]} {vcfFields[VcfCommon.PosIndex]} {vcfFields[VcfCommon.RefIndex]} {vcfFields[VcfCommon.AltIndex]}");
                    }
                    continue;
                }

                if (vcfFields[VcfCommon.ChromIndex].Contains("Y") && isSampleFemale) continue;

                totalSampleSize++;

                var sampleCopynumber = GetCopyNumber(vcfFields[VcfCommon.FormatIndex], vcfFields[col], isCnv, hapCopyNumbers);
                int expectedCopyNumber = GetExpectedCopyNumber(vcfFields[VcfCommon.FormatIndex], vcfFields[col], isCnv, vcfFields[VcfCommon.IdIndex]);

                if (!vcfFields[VcfCommon.ChromIndex].Contains("Y"))
                {
                    if (IsReference(sampleGenotype))
                    {
                        AddRefSample(sampleRecorder, samplePopulation);
                    }
                    else
                    {
                        nonRefSample++;
                        AddNonRefSample(sampleRecorder, samplePopulation);
                    }
                }
                else
                {
                    if (expectedCopyNumber >= 0 && expectedCopyNumber == sampleCopynumber)
                    {
                        AddRefSample(sampleRecorder, samplePopulation);
                    }
                    else
                    {
                        nonRefSample++;
                        AddNonRefSample(sampleRecorder, samplePopulation);
                    }

                }

                if (sampleCopynumber > expectedCopyNumber) observedGains++;
                if (sampleCopynumber < expectedCopyNumber) observedLosses++;
            }

            sb.Append("\t" + totalSampleSize + "\t" + observedGains + "\t" + observedLosses);
            var freqAll = nonRefSample / (double)totalSampleSize;
            sb.Append("\t" + freqAll.ToString("0.#####"));

            foreach (var population in _availablePopulations)
            {
                if (_superPopulation.ContainsKey(population))
                {
                    var sampleSize = 0;
                    var nonRef = 0;
                    foreach (var subPopulation in _superPopulation[population])
                    {
                        sampleSize += sampleRecorder[subPopulation][0];
                        nonRef += sampleRecorder[subPopulation][1];
                    }
                    var freqSuper = nonRef / (double)sampleSize;
                    sb.Append("\t" + sampleSize + "\t" + freqSuper.ToString("0.#####"));
                    continue;
                }

                var freq = sampleRecorder[population][1] / (double)sampleRecorder[population][0];
                sb.Append("\t" + sampleRecorder[population][0] + "\t" + freq.ToString("0.#####"));
            }
        }

        private static void AddNonRefSample(Dictionary<string, int[]> sampleRecorder, string samplePopulation)
        {
            if (sampleRecorder.ContainsKey(samplePopulation))
            {
                sampleRecorder[samplePopulation][0]++;
                sampleRecorder[samplePopulation][1]++;
            }
            else
            {
                sampleRecorder[samplePopulation] = new[] { 1, 1 };
            }
        }

        private static void AddRefSample(Dictionary<string, int[]> sampleRecorder, string samplePopulation)
        {
            if (sampleRecorder.ContainsKey(samplePopulation))
            {
                sampleRecorder[samplePopulation][0]++;
            }
            else
            {
                sampleRecorder[samplePopulation] = new[] { 1, 0 };
            }
        }


        private int GetExpectedCopyNumber(string format, string sampleCol, bool isCnv, string varID)
        {
            if (!isCnv) return -1;

            //for chrY
            if (!format.Equals("GT"))
            {
                return varID.StartsWith("GS_SD_M2") ? 2 : 1;
            }

            if (!sampleCol.Contains("|") && !sampleCol.Contains("/")) return 1; // for male in chrX non PAR region

            return 2;
        }

        private bool IsReference(string sampleGenotype)
        {
            return sampleGenotype.Equals("0/0") || sampleGenotype.Equals("0|0") || sampleGenotype.Equals("0");
        }

        private int GetCopyNumber(string format, string sampleCol, bool isCnv, List<int> hapCopyNumbers)
        {
            if (!isCnv) return -1;
            if (!format.Equals("GT")) return Convert.ToInt32(sampleCol.Split(':')[1]); //chrY

            int copyNum;
            var copyNumMatch    = Regex.Match(sampleCol, @"^(\d+)[\\|\|](\d+)$");
            var hapCopyNumMatch = Regex.Match(sampleCol, @"^(\d+)$");

            if (copyNumMatch.Success)
            {
                copyNum = hapCopyNumbers[Convert.ToInt32(copyNumMatch.Groups[1].ToString())] +
                          hapCopyNumbers[Convert.ToInt32(copyNumMatch.Groups[2].ToString())];

            }
            else if (hapCopyNumMatch.Success)
            {
                copyNum = hapCopyNumbers[Convert.ToInt32(hapCopyNumMatch.Groups[1].ToString())];
            }
            else
            {
                throw new Exception($"unknown genotype {sampleCol}");
            }

            return copyNum;
        }

        private string GetGenotype(string format, string sampleCol)
        {
            if (!format.Equals("GT")) return sampleCol.Split(':')[0];

            return sampleCol;
        }

        private void ParseSvInfoField(string infoFields, out string svType, out int? svEnd)
        {
            svType = null;
            svEnd = null;

            if (infoFields == "" || infoFields == ".") return;

            var infoItems = infoFields.Split(';');
            foreach (var infoItem in infoItems)
            {
                var infoKeyValue = infoItem.Split('=');
                if (infoKeyValue.Length != 2) continue;
                var key = infoKeyValue[0];
                var value = infoKeyValue[1];
                switch (key)
                {
                    case "SVTYPE":
                        svType = value;
                        break;
                    case "END":
                        svEnd = Convert.ToInt32(value);
                        break;
                }
            }
        }
    }
}