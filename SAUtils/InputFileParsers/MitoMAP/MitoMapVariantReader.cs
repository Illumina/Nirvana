using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.ClinVar;
using VariantAnnotation.Providers;

namespace SAUtils.InputFileParsers.MitoMap
{
    public sealed class MitoMapVariantReader
    {
        private readonly FileInfo _mitoMapFileInfo;
        private const string DelSymbol = "";
        private readonly string _dataType;
        private readonly ReferenceSequenceProvider _sequenceProvider;
        private readonly VariantAligner _variantAligner;


        private readonly Dictionary<string, int[]> _mitoMapMutationColumnDefinitions = new Dictionary<string, int[]>
        {
            {MitoMapDataTypes.MitoMapMutationsCodingControl, new[] {0, 2, 3, 6, 7, 8, -1}},
            {MitoMapDataTypes.MitoMapMutationsRNA, new[] {0, 2, 3, 5, 6, 7, 8}},
            {MitoMapDataTypes.MitoMapPolymorphismsCoding,  new[] {0, -1, 2, -1, -1, -1, -1}},
            {MitoMapDataTypes.MitoMapPolymorphismsControl,  new[] {0, -1, 2, -1, -1, -1, -1}},
            {MitoMapDataTypes.MitoMapInsertionsSimple,  new int[0]},
            {MitoMapDataTypes.MitoMapDeletionsSingle,  new int[0]}
        };

        private readonly Dictionary<(string, int), string> _clininalSigfincances = new Dictionary<(string, int), string>
        {
            {("up", 3), "confirmed pathogenic"},
            {("up", 2), "likely pathogenic"},
            {("up", 1), "possibly pathogenic"},
            {("down", 1), "possibly benign"},
            {("down", 2), "likely benign"}
        };

        private readonly Dictionary<string, bool> _symbolToBools = new Dictionary<string, bool>
        {
            {"+", true},
            {"-", false}
        };

        private readonly HashSet<string> _mitoMapDelSymbolSet = new HashSet<string> { ":", "del", "d" };


        public MitoMapVariantReader(FileInfo mitoMapFileInfo, ReferenceSequenceProvider sequenceProvider)
        {
            _mitoMapFileInfo = mitoMapFileInfo;
            _dataType = GetDataType();
            _sequenceProvider = sequenceProvider;
            _variantAligner = new VariantAligner(sequenceProvider.Sequence);
        }

        private string GetDataType()
        {
            var dataType = _mitoMapFileInfo.Name.Replace(".html", "");
            if (!_mitoMapMutationColumnDefinitions.ContainsKey(dataType)) throw new InvalidFileFormatException($"Unexpected data file: {_mitoMapFileInfo.Name}");
            return dataType;
        }

        private IEnumerable<MitoMapItem> GetMitoMapItems()
        {
            Console.WriteLine($"Processing {_dataType} file");
            bool isDataLine = false;
            using (var reader = new StreamReader(_mitoMapFileInfo.FullName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    line = line.Trim();
                    if (!isDataLine)
                    {
                        if (line == "\"data\":[") isDataLine = true;
                        continue;
                    }
                    // last item
                    if (line.StartsWith("[") && line.EndsWith("]],")) isDataLine = false;

                    foreach (var mitoMapMutItem in ParseLine(line, _dataType))
                    {
                        if (!string.IsNullOrEmpty(mitoMapMutItem.ReferenceAllele) ||
                            !string.IsNullOrEmpty(mitoMapMutItem.AlternateAllele))
                            yield return mitoMapMutItem;
                    }
                }
            }
        }

        private List<MitoMapItem> ParseLine(string line, string dataType)
        {
            // line validation
            if (!(line.StartsWith("[") && line.EndsWith("],")))
                throw new InvalidFileFormatException($"Data line doesn't start with \"[\" or end with \"],\": {line}");
            /* example lines
            ["582","<a href='/MITOMAP/GenomeLoci#MTTF'>MT-TF</a>","Mitochondrial myopathy","T582C","tRNA Phe","-","+","Reported","<span style='display:inline-block;white-space:nowrap;'><a href='/cgi-bin/mitotip?pos=582&alt=C&quart=2'><u>72.90%</u></a> <i class='fa fa-arrow-up' style='color:orange' aria-hidden='true'></i></span>","0","<a href='/cgi-bin/print_ref_list?refs=90165,91590&title=RNA+Mutation+T582C' target='_blank'>2</a>"],
            ["583","<a href='/MITOMAP/GenomeLoci#MTTF'>MT-TF</a>","MELAS / MM & EXIT","G583A","tRNA Phe","-","+","Cfrm","<span style='display:inline-block;white-space:nowrap;'><a href='/cgi-bin/mitotip?pos=583&alt=A&quart=0'><u>93.10%</u></a> <i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i></span>","0","<a href='/cgi-bin/print_ref_list?refs=2066,90532,91590&title=RNA+Mutation+G583A' target='_blank'>3</a>"],
            */
            var info = line.TrimEnd(',').TrimEnd(']').Trim('[', ']').Split("\",\"").Select(x => x.Trim('"')).ToList();
            switch (dataType)
            {
                case MitoMapDataTypes.MitoMapInsertionsSimple:
                    return ExtractVariantItemFromInsertionsSimple(info);
                case MitoMapDataTypes.MitoMapDeletionsSingle:
                    return ExtractVariantItemFromDeletionsSingle(info);
            }
            return ExtracVariantItem(info, _mitoMapMutationColumnDefinitions[dataType]);
        }

        private List<MitoMapItem> ExtractVariantItemFromDeletionsSingle(List<string> info)
        {
            var junctions = info[0].Split(':').Select(int.Parse).ToList();
            var start = junctions[0] + 1;
            var end = junctions[1] - 1;
            if (end < start)
                throw new ArgumentOutOfRangeException($"Deletions with end position smaller than start position: start: {start}, end: {end}");
            var calculatedSize = end - start + 1;
            var size = int.Parse(info[1].Substring(1));
            if (size > MitomapParsingParameters.LargeDeletionCutoff) return new List<MitoMapItem>();
            if (calculatedSize != size) Console.WriteLine($"Incorrect size of deleted region: size of {start}-{end} should be {calculatedSize}, provided size is {size}. Provided size is used.");
            var refSequence = _sequenceProvider.Sequence.Substring(start - 1, size);
            var leftAlignResults = GetLeftAlignedVariant(start, refSequence, "");
            var mitoMapItem = new MitoMapItem(leftAlignResults.RefPosition, leftAlignResults.RefAllele, "-", null, null, null, "", "", "", false, null, null, _sequenceProvider);
            return new List<MitoMapItem> { mitoMapItem };
        }

        // extract small variant from this file
        private List<MitoMapItem> ExtractVariantItemFromInsertionsSimple(List<string> info)
        {
            var altAlleleInfo = info[2];
            var dLoopPattern = new Regex(@"(?<start>^\d+)-(?<end>(\d+)) D-Loop region");
            var dLoopMatch = dLoopPattern.Match(altAlleleInfo);
            // not a small variant
            if (dLoopMatch.Success)
            {
                return new List<MitoMapItem>();
            }
            string altAllele;
            var additionalRepeatPattern = new Regex(@"additional \[(?<repeat>[ACTGN]+)\] ");
            var additionalRepeatMatch = additionalRepeatPattern.Match(altAlleleInfo);
            if (additionalRepeatMatch.Success)
                altAllele = additionalRepeatMatch.Groups["repeat"].Value;
            // expect a string of allele sequence then
            else
            {
                if (altAlleleInfo.Contains(" ")) throw new Exception($"Cannot parse {altAlleleInfo}");
                altAllele = altAlleleInfo;
            }
            var firstNumberPattern = new Regex(@"(?<firstNumber>^\d+)");
            var firstNumberMatch = firstNumberPattern.Match(info[3]);
            if (!firstNumberMatch.Success) throw new Exception($"Failed to extract variant position from {info[3]}");
            var position = int.Parse(firstNumberMatch.Groups["firstNumber"].Value);
            var leftAlgnResults = GetLeftAlignedVariant(position, "", altAllele); // insertion
            return new List<MitoMapItem>{new MitoMapItem(leftAlgnResults.RefPosition, "-", leftAlgnResults.AltAllele, null, null, null, "", "", "", false, null, null, _sequenceProvider) };
        }

        private List<MitoMapItem> ExtracVariantItem(List<string> info, int[] fields)
        {
            List<MitoMapItem> mitoMapVarItems = new List<MitoMapItem>();
            int position = int.Parse(info[fields[0]]);
            var mitomapDiseaseString = GetDiseaseInfo(info, fields[1]);
            if (DescribedAsDuplicatedRecord(mitomapDiseaseString)) return mitoMapVarItems;
            var diseases = string.IsNullOrEmpty(mitomapDiseaseString) ? null : new List<string> {mitomapDiseaseString};
            var (refAllele, rawAltAllele, extractedPosition) = GetRefAltAlleles(info[fields[2]]);
            if (extractedPosition.HasValue && position != extractedPosition)
                Console.WriteLine($"Inconsistant positions found: annotated position: {position}; allele {info[fields[2]]}");
            if (string.IsNullOrEmpty(refAllele) && string.IsNullOrEmpty(rawAltAllele))
            {
                Console.WriteLine($"No reference and alternative alleles could be extracted: {position}; allele {info[fields[2]]}");
                return mitoMapVarItems;
            }
            if (_mitoMapDelSymbolSet.Contains(rawAltAllele)) rawAltAllele = DelSymbol;
            bool? homoplasmy = null;
            if (fields[3] != -1 && _symbolToBools.ContainsKey(info[fields[3]])) homoplasmy = _symbolToBools[info[fields[3]]];
            bool? heteroplasmy = null;
            if (fields[4] != -1 && _symbolToBools.ContainsKey(info[fields[4]])) heteroplasmy = _symbolToBools[info[fields[4]]];
            string status = fields[5] == -1 ? null : info[fields[5]];
            var (scorePercentile, clinicalSignificance) = GetFunctionalInfo(info, fields[6]);
            if (!string.IsNullOrEmpty(rawAltAllele))
            {
                foreach (var altAllele in GetAltAlleles(rawAltAllele))
                {
                    var thisLeftAlignResults = GetLeftAlignedVariant(position, refAllele, altAllele);
                    mitoMapVarItems.Add(new MitoMapItem(thisLeftAlignResults.RefPosition, thisLeftAlignResults.RefAllele, thisLeftAlignResults.AltAllele, diseases, homoplasmy,heteroplasmy, status, clinicalSignificance, scorePercentile, false, null, null, _sequenceProvider));
                }
                if (mitoMapVarItems.Count > 1) Console.WriteLine($"Multiple Alternative Allele Sequences {info[fields[2]]} at {position}");
                return mitoMapVarItems;         
            }
            var leftAlignResults = GetLeftAlignedVariant(position, refAllele, rawAltAllele);
            mitoMapVarItems.Add(new MitoMapItem(leftAlignResults.RefPosition, leftAlignResults.RefAllele, leftAlignResults.AltAllele, diseases, homoplasmy,
                    heteroplasmy, status, clinicalSignificance, scorePercentile, false, null, null, _sequenceProvider));
            return mitoMapVarItems;
        }

        // there may be multiple alt alleles concatenated by ";"
        internal static IEnumerable<string> GetAltAlleles(string rawAltAllele) => rawAltAllele.Split(";").Select(DegenerateBaseUtilities.GetAllPossibleSequences).SelectMany(x => x);


        private static bool DescribedAsDuplicatedRecord(string mitomapDiseaseString)
        {
            if (string.IsNullOrEmpty(mitomapDiseaseString)) return false;
            var altNotationPattern1 = new Regex("alternate notation$");
            var altNotationMatch = altNotationPattern1.Match(mitomapDiseaseString);
            if (altNotationMatch.Success)
            {
                Console.WriteLine($"Alternate notation found: {mitomapDiseaseString}. This record is skipped.");
                return true;
            }
            return false;
        }

        private static string GetDiseaseInfo(List<string> info, int fieldIndex)
        {
            if (fieldIndex == -1) return null;
            string diseaseString = info[fieldIndex];
            if (string.IsNullOrEmpty(diseaseString)) return diseaseString;
            var regexPattern = new Regex(@"<a href=.+>(?<disease>.+)</a>$");
            var match = regexPattern.Match(diseaseString);
            return match.Success ? match.Groups["disease"].Value : diseaseString;
        }

        private (string, string) GetFunctionalInfo(List<string> info, int fieldIndex)
        {
            if (fieldIndex == -1) return (null, null);
            string functionInfoString = info[fieldIndex];
            // <u>93.10%</u></a> <i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i></span>
            var regexPattern = new Regex(@"<u>(?<scoreString>[0-9.]+)%</u></a> (?<significanceString>.+)</span>$");
            var match = regexPattern.Match(functionInfoString);
            var clineSignificance = GetClinicalSignificance(match.Groups["significanceString"].Value);
            return (match.Groups["scoreString"].Value, clineSignificance);
        }

        private string GetClinicalSignificance(string significanceString)
        {
            // < i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i>
            // filter out the symbol for frequency alert
            var arrows = significanceString.Split(@"</i>", StringSplitOptions.RemoveEmptyEntries).Where(x => !x.Contains("fa-asterisk")).ToList();
            var nArrows = arrows.Count;
            if (nArrows == 0) return null;
            var arrowType = arrows[0].Contains("fa-arrow-up") ? "up" : "down";
            return _clininalSigfincances[(arrowType, nArrows)];
        }

        private (string, string, int?) GetRefAltAlleles(string alleleString)
        {
            // C123T, A-del or A123del
            var regexPattern1 = new Regex(@"(?<ref>^[ACGTacgtNn]+)(?<position>(\d+|-))(?<alt>([ACGTBDHKMRSVWYNacgtbdhkmrsvwyn]+|:|del[ACGTacgtNn]*|d)$)");
            var match1 = regexPattern1.Match(alleleString);
            if (match1.Success)
            {
                int? extractedPosition = null;
                if (match1.Groups["position"].Value != "-") extractedPosition = int.Parse(match1.Groups["position"].Value);
                return (match1.Groups["ref"].Value, match1.Groups["alt"].Value, extractedPosition);
            }

            // 16021_16022del
            var regexPattern2 = new Regex(@"(?<start>^\d+)[_|-](?<end>\d+)del");
            var match2 = regexPattern2.Match(alleleString);
            if (match2.Success)
            {
                var start = int.Parse(match2.Groups["start"].Value);
                var end = int.Parse(match2.Groups["end"].Value);
                return (GetRefAllelesFromReferece(_sequenceProvider, start, end - start + 1), "-", start);
            }
            // 8042del2
            var regexPattern3 = new Regex(@"(?<position>^\d+)del(?<length>\d+)");
            var match3 = regexPattern3.Match(alleleString);
            if (match3.Success)
            {
                var extractedPosition = int.Parse(match3.Groups["position"].Value);
                return (GetRefAllelesFromReferece(_sequenceProvider, extractedPosition,
                    int.Parse(match3.Groups["length"].Value)), "-", extractedPosition);
            }
            // C9537insC
            var regexPattern4 = new Regex(@"(?<ref>[ACGTacgtNn])(?<position>\d+)ins(?<extra>[ACGTacgtNn]+)");
            var match4 = regexPattern4.Match(alleleString);
            if (match4.Success)
            {
                var extractedPosition = int.Parse(match4.Groups["position"].Value);
                var refAllele = match4.Groups["ref"].Value;
                var altAllele = refAllele + match4.Groups["extra"].Value;
                return (refAllele, altAllele, extractedPosition);
            }
            // 3902_3908invACCTTGC
            var regexPattern5 = new Regex(@"(?<start>^\d+)[_|-](?<end>\d+)inv(?<seq>[ACGTacgtNn]+)");
            var match5 = regexPattern5.Match(alleleString);
            if (match5.Success)
            {
                var start = int.Parse(match5.Groups["start"].Value);
                var end = int.Parse(match5.Groups["end"].Value);
                var refSequence = GetRefAllelesFromReferece(_sequenceProvider, start, end - start + 1);
                if (refSequence != match5.Groups["seq"].Value) throw new Exception($"Inconsistent sequences: reference {refSequence}, annotation {match5.Groups["seq"].Value}");
                return (refSequence, ReverseSequence(refSequence), start);
            }
            //A-Cor CC
            var regexPattern6 = new Regex(@"(?<ref>[ACGTacgtNn]+)[_|-](?<alt1>[ACGTacgtNn]+) ?or ?(?<alt2>[ACGTacgtNn]+)");
            var match6 = regexPattern6.Match(alleleString);
            if (match6.Success)
            {
                var altAllele = match6.Groups["alt1"].Value + ";" + match6.Groups["alt2"].Value;
                return (match6.Groups["ref"].Value, altAllele, null);
            }
            // C-C(2-8)
            var regexPattern7 = new Regex(@"(?<ref>[ACGTacgtNn])[_|-](?<alt>[ACGTacgtNn])\((?<min>\d+)-(?<max>\d+)\)");
            var match7 = regexPattern7.Match(alleleString);
            if (match7.Success)
            {
                var altBase = char.Parse(match7.Groups["alt"].Value);
                int minRepeat = int.Parse(match7.Groups["min"].Value);
                int maxRepeat = int.Parse(match7.Groups["max"].Value);
                var altAlleleSequences = new List<string>();
                for (int i = minRepeat; i <= maxRepeat; i++)
                {
                    altAlleleSequences.Add(new string(altBase, i));
                }
                return (match7.Groups["ref"].Value, string.Join(";", altAlleleSequences), null);
            }
            // 8042delAT
            var regexPattern8 = new Regex(@"(?<position>^\d+)del(?<del>[ACGTacgtNn]+)");
            var match8 = regexPattern8.Match(alleleString);
            if (match8.Success)
            {
                var extractedPosition = int.Parse(match8.Groups["position"].Value);
                string deletedSeq = match8.Groups["del"].Value;
                string deletedReferenceSeq = GetRefAllelesFromReferece(_sequenceProvider, extractedPosition,deletedSeq.Length);
                if (deletedSeq != deletedReferenceSeq)
                {
                    throw new Exception($"Deleted sequence at {extractedPosition}: annoation is {deletedSeq}, reference sequence is {deletedReferenceSeq}");
                }
                return (deletedReferenceSeq, "-", extractedPosition);
            }
            return (null, null, null);
        }

        private static string GetRefAllelesFromReferece(ReferenceSequenceProvider sequenceProvider, int start, int length)
        {
            return sequenceProvider.Sequence.Substring(start - 1, length);
        }

        private static string ReverseSequence(string sequence)
        {
            var reversedNucleotide = new char[sequence.Length];
            var i = sequence.Length - 1;
            foreach (var nucleotide in sequence)
            {
                reversedNucleotide[i] = nucleotide;
                i--;
            }
            return new string(reversedNucleotide);
        }

        public static IEnumerable<MitoMapItem> MergeAndSort(List<MitoMapVariantReader> mitoMapMutationReaders)
        {
            var allItems = mitoMapMutationReaders.SelectMany(x => x.GetMitoMapItems()).ToList();
            allItems.ForEach(x => x.Trim());
            return allItems.OrderBy(x => x.Start);
        }

        private (int RefPosition, string RefAllele, string AltAllele) GetLeftAlignedVariant(int position, string refAllele, string altAllele)
        {
            if (refAllele == null || altAllele == null) return (position, refAllele, altAllele);
            if (refAllele == "-") refAllele = "";
            if (altAllele == "-") altAllele = "";
            var leftAlgnResults = _variantAligner.LeftAlign(position, refAllele, altAllele); 
            var newPosition = leftAlgnResults.RefPosition;
            var newRefAllele = leftAlgnResults.RefAllele;
            var newAltAllele = leftAlgnResults.AltAllele;
            if (position == newPosition) return leftAlgnResults;
            if (newRefAllele == "") // insertion
                Console.WriteLine(
                    $"Insertion of {altAllele}. Original start position: {position}; new position after left-alignment {newPosition}; new altAllele {newAltAllele}");
            else if (newAltAllele == "") // deletion
                Console.WriteLine($"Deletion of {newRefAllele.Length} bps. Original start start position: {position}; new position after left-alignment {newPosition}.");
            else
            {
                throw new Exception($"{position}:{refAllele}:{altAllele} becomes {newPosition}:{newRefAllele}:{newAltAllele} after left alignment. Left-alignment should be only performed for deletions and insertions");
            }
            return leftAlgnResults;
        }
    }
}