using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using OptimizedCore;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.ClinVar;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;

namespace SAUtils.MitoMap
{
    public sealed class MitoMapVariantReader
    {
        private readonly FileInfo _mitoMapFileInfo;
        private const string DelSymbol = "";
        private readonly string _dataType;
        private readonly ReferenceSequenceProvider _sequenceProvider;
        private readonly VariantAligner _variantAligner;
        private readonly IChromosome _chromosome;
        private readonly MitoMapInputDb _mitoMapInputDb;

        private static readonly Dictionary<string, int[]> MitoMapMutationColumnDefinitions = new Dictionary<string, int[]>
        {
            {MitoMapDataTypes.MitoMapMutationsCodingControl, new[] {0, 2, 3, 6, 7, 8, -1, 10, 11}},
            {MitoMapDataTypes.MitoMapMutationsRNA, new[] {0, 2, 3, 5, 6, 7, 8, 10, 11}},
            {MitoMapDataTypes.MitoMapPolymorphismsCoding,  new[] {0, -1, 2, -1, -1, -1, -1, 7, 8}},
            {MitoMapDataTypes.MitoMapPolymorphismsControl,  new[] {0, -1, 2, -1, -1, -1, -1, 4, 5}},
            {MitoMapDataTypes.MitoMapInsertionsSimple,  new int[0]},
            {MitoMapDataTypes.MitoMapDeletionsSingle,  new int[0]}
        };

        private static readonly Dictionary<(string, int), string> ClinicalSignificances = new Dictionary<(string, int), string>
        {
            {("up", 3), "confirmed pathogenic"},
            {("up", 2), "likely pathogenic"},
            {("up", 1), "possibly pathogenic"},
            {("down", 1), "possibly benign"},
            {("down", 2), "likely benign"}
        };

        private static readonly Dictionary<string, bool> SymbolToBools = new Dictionary<string, bool>
        {
            {"+", true},
            {"-", false}
        };

        private static readonly HashSet<string> MitoMapDelSymbolSet = new HashSet<string> { ":", "del", "d" };
        private static readonly HashSet<string> IgnoredStatus = new HashSet<string> { "See 7471insC", "Reported  (alt loc)" };

        public MitoMapVariantReader(FileInfo mitoMapFileInfo, MitoMapInputDb mitoMapInputDb, ReferenceSequenceProvider sequenceProvider)
        {
            _mitoMapFileInfo = mitoMapFileInfo;
            _mitoMapInputDb = mitoMapInputDb;
            _dataType = GetDataType();
            _sequenceProvider = sequenceProvider;
            _chromosome = sequenceProvider.RefNameToChromosome["chrM"];
            _variantAligner = new VariantAligner(sequenceProvider.Sequence);
        }

        private string GetDataType()
        {
            var dataType = _mitoMapFileInfo.Name.Replace(".html", "");
            if (!MitoMapMutationColumnDefinitions.ContainsKey(dataType)) throw new InvalidDataException($"Unexpected data file: {_mitoMapFileInfo.Name}");
            return dataType;
        }

        private IEnumerable<MitoMapItem> GetMitoMapItems()
        {
            Console.WriteLine($"Processing {_dataType} file");
            bool isDataLine = false;
            using (var reader = FileUtilities.GetStreamReader(FileUtilities.GetReadStream(_mitoMapFileInfo.FullName)))
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
                    if (line.OptimizedStartsWith('[') && line.EndsWith("]],")) isDataLine = false;

                    foreach (var mitoMapMutItem in ParseLine(line, _dataType, _sequenceProvider, _variantAligner, _chromosome, _mitoMapInputDb))
                    {
                        if (!string.IsNullOrEmpty(mitoMapMutItem.RefAllele) ||
                            !string.IsNullOrEmpty(mitoMapMutItem.AltAllele))
                            yield return mitoMapMutItem;
                    }
                }
            }
        }

        internal static List<MitoMapItem> ParseLine(string line, string dataType, ISequenceProvider sequenceProvider,
            VariantAligner variantAligner, IChromosome chromosome, MitoMapInputDb mitoMapInputDb)
        {
            // line validation
            if (!(line.OptimizedStartsWith('[') && line.EndsWith("],")))
                throw new InvalidFileFormatException($"Data line doesn't start with \"[\" or end with \"],\": {line}");
            /* example lines
            ["582","<a href='/MITOMAP/GenomeLoci#MTTF'>MT-TF</a>","Mitochondrial myopathy","T582C","tRNA Phe","-","+","Reported","<span style='display:inline-block;white-space:nowrap;'><a href='/cgi-bin/mitotip?pos=582&alt=C&quart=2'><u>72.90%</u></a> <i class='fa fa-arrow-up' style='color:orange' aria-hidden='true'></i></span>","0","<a href='/cgi-bin/print_ref_list?refs=90165,91590&title=RNA+Mutation+T582C' target='_blank'>2</a>"],
            ["583","<a href='/MITOMAP/GenomeLoci#MTTF'>MT-TF</a>","MELAS / MM & EXIT","G583A","tRNA Phe","-","+","Cfrm","<span style='display:inline-block;white-space:nowrap;'><a href='/cgi-bin/mitotip?pos=583&alt=A&quart=0'><u>93.10%</u></a> <i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i></span>","0","<a href='/cgi-bin/print_ref_list?refs=2066,90532,91590&title=RNA+Mutation+G583A' target='_blank'>3</a>"],
            */
            var info = line.TrimEnd(',').TrimEnd(']').Trim('[', ']').Split("\",\"").Select(x => x.Trim('"')).ToList();
            switch (dataType)
            {
                case MitoMapDataTypes.MitoMapInsertionsSimple:
                    return ExtractVariantItemFromInsertionsSimple(info, sequenceProvider, variantAligner, chromosome, mitoMapInputDb);
                case MitoMapDataTypes.MitoMapDeletionsSingle:
                    return ExtractVariantItemFromDeletionsSingle(info, sequenceProvider, variantAligner, chromosome, mitoMapInputDb);
            }
            return ExtractVariantItem(info, dataType, sequenceProvider, variantAligner, chromosome, mitoMapInputDb);
        }

        private static List<MitoMapItem> ExtractVariantItemFromDeletionsSingle(List<string> info,
            ISequenceProvider sequenceProvider, VariantAligner variantAligner, IChromosome chromosome,
            MitoMapInputDb mitoMapInputDb)
        {
            var junctions = info[0].OptimizedSplit(':').Select(int.Parse).ToList();
            var start = junctions[0] + 1;
            var end = junctions[1] - 1;
            if (end < start)
                throw new ArgumentOutOfRangeException($"Deletions with end position smaller than start position: start: {start}, end: {end}");
            var calculatedSize = end - start + 1;
            var size = int.Parse(info[1].Substring(1));
            if (size > MitomapParsingParameters.LargeDeletionCutoff) return new List<MitoMapItem>();
            if (calculatedSize != size) Console.WriteLine($"Incorrect size of deleted region: size of {start}-{end} should be {calculatedSize}, provided size is {size}. Provided size is used.");
            var refSequence = sequenceProvider.Sequence.Substring(start - 1, size);
            var leftAlignResults = GetLeftAlignedVariant(start, refSequence, "", variantAligner);
            var pubMedIds = ParsingUtilities.GetPubMedIds(info[4], mitoMapInputDb);
            var mitoMapItem = new MitoMapItem(chromosome, leftAlignResults.RefPosition, leftAlignResults.RefAllele, "-", null, null, null, "", "", "", sequenceProvider, default, pubMedIds);
            return new List<MitoMapItem> { mitoMapItem };
        }

        // extract small variant from this file
        private static List<MitoMapItem> ExtractVariantItemFromInsertionsSimple(List<string> info,
            ISequenceProvider sequenceProvider, VariantAligner variantAligner, IChromosome chromosome,
            MitoMapInputDb mitoMapInputDb)
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
                if (altAlleleInfo.Contains(" ")) throw new InvalidDataException($"Cannot parse {altAlleleInfo}");
                altAllele = altAlleleInfo;
            }
            var firstNumberPattern = new Regex(@"(?<firstNumber>^\d+)");
            var firstNumberMatch = firstNumberPattern.Match(info[3]);
            if (!firstNumberMatch.Success) throw new InvalidDataException($"Failed to extract variant position from {info[3]}");
            var position = int.Parse(firstNumberMatch.Groups["firstNumber"].Value);
            var leftAlgnResults = GetLeftAlignedVariant(position, "", altAllele, variantAligner); // insertion
            var pubMedIds = ParsingUtilities.GetPubMedIds(info[6], mitoMapInputDb);
            return new List<MitoMapItem>{new MitoMapItem(chromosome, leftAlgnResults.RefPosition, "-", leftAlgnResults.AltAllele, null, null, null, "", "", "", sequenceProvider, default, pubMedIds) };
        }

        private static List<MitoMapItem> ExtractVariantItem(List<string> info, string dataType,
            ISequenceProvider sequenceProvider, VariantAligner variantAligner, IChromosome chromosome,
            MitoMapInputDb mitoMapInputDb)
        {
            int[] fields = MitoMapMutationColumnDefinitions[dataType];
            List<MitoMapItem> mitoMapVarItems = new List<MitoMapItem>();
            int position = int.Parse(info[fields[0]]);
            var mitomapDiseaseString = GetDiseaseInfo(info, fields[1]);
            if (DescribedAsDuplicatedRecord(mitomapDiseaseString)) return mitoMapVarItems;

            var diseases = string.IsNullOrEmpty(mitomapDiseaseString) ? null : new List<string> {mitomapDiseaseString};
            var (refAllele, rawAltAllele, extractedPosition) = GetRefAltAlleles(info[fields[2]], sequenceProvider);

            if (extractedPosition.HasValue && position != extractedPosition)
                Console.WriteLine($"Inconsistant positions found: annotated position: {position}; allele {info[fields[2]]}");

            if (string.IsNullOrEmpty(refAllele) && string.IsNullOrEmpty(rawAltAllele))
            {
                Console.WriteLine($"No reference and alternative alleles could be extracted: {position}; allele {info[fields[2]]}");
                return mitoMapVarItems;
            }

            if (MitoMapDelSymbolSet.Contains(rawAltAllele)) rawAltAllele = DelSymbol;

            var homoplasmy   = GetPlasmy(info, fields[3]);
            var heteroplasmy = GetPlasmy(info, fields[4]);

            string status = GetStatus(info, fields);
            (string scorePercentile, string clinicalSignificance) = GetFunctionalInfo(info, fields[6]);
            int numFullLengthSeqs = GetNumFullLengthSequences(info[fields[7]], dataType);
            var pubMedIds = ParsingUtilities.GetPubMedIds(info[fields[8]], mitoMapInputDb);

            if (!string.IsNullOrEmpty(rawAltAllele))
            {
                foreach (var altAllele in GetAltAlleles(rawAltAllele))
                {
                    var thisLeftAlignResults = GetLeftAlignedVariant(position, refAllele, altAllele, variantAligner);
                    mitoMapVarItems.Add(new MitoMapItem(chromosome, thisLeftAlignResults.RefPosition, thisLeftAlignResults.RefAllele, thisLeftAlignResults.AltAllele, diseases, homoplasmy,heteroplasmy, status, clinicalSignificance, scorePercentile, sequenceProvider, numFullLengthSeqs, pubMedIds));
                }
                if (mitoMapVarItems.Count > 1) Console.WriteLine($"Multiple Alternative Allele Sequences {info[fields[2]]} at {position}");
                return mitoMapVarItems;         
            }

            var leftAlignResults = GetLeftAlignedVariant(position, refAllele, rawAltAllele, variantAligner);
            mitoMapVarItems.Add(new MitoMapItem(chromosome, leftAlignResults.RefPosition, leftAlignResults.RefAllele, leftAlignResults.AltAllele, diseases, homoplasmy,
                    heteroplasmy, status, clinicalSignificance, scorePercentile, sequenceProvider, numFullLengthSeqs, pubMedIds));

            return mitoMapVarItems;
        }

        private static string GetStatus(List<string> info, int[] fields)
        {
            string status = fields[5] == -1 ? null : info[fields[5]];
            return IgnoredStatus.Contains(status) ? null : status;
        }

        internal static int GetNumFullLengthSequences(string field, string dataType)
        {
            if (!field?.OptimizedStartsWith('<') ?? true) return 0;

            int leadingCharIndex = -1;
            int trailingCharIndex = -1;
            switch (dataType)
            {
                case MitoMapDataTypes.MitoMapMutationsRNA:
                case MitoMapDataTypes.MitoMapMutationsCodingControl:
                    leadingCharIndex = field.IndexOf('>');
                    trailingCharIndex = field.IndexOf(" (", StringComparison.Ordinal);
                    break;

                case MitoMapDataTypes.MitoMapPolymorphismsCoding:
                    leadingCharIndex = field.IndexOf('>');
                    trailingCharIndex = field.IndexOf("</", StringComparison.Ordinal);
                break;

                case MitoMapDataTypes.MitoMapPolymorphismsControl:
                    leadingCharIndex = field.IndexOf('(');
                    trailingCharIndex = field.IndexOf('/', leadingCharIndex+1);
                    break;
            }
            string numFullLengthString = field.Substring(leadingCharIndex + 1, trailingCharIndex - leadingCharIndex - 1);
            if (int.TryParse(numFullLengthString, out int numFullLength)) return numFullLength;
            
            throw new InvalidDataException($"Can't extract number of full length GenBank sequences from {field} in the {dataType} dataset.");
        }

        private static bool? GetPlasmy(List<string> info, int fields)
        {
            if (fields == -1 || !SymbolToBools.TryGetValue(info[fields], out bool b)) return null;
            return b;
        }

        // there may be multiple alt alleles concatenated by ";"
        internal static IEnumerable<string> GetAltAlleles(string rawAltAllele) => rawAltAllele.OptimizedSplit(';').Select(DegenerateBaseUtilities.GetAllPossibleSequences).SelectMany(x => x);


        private static bool DescribedAsDuplicatedRecord(string mitomapDiseaseString)
        {
            if (string.IsNullOrEmpty(mitomapDiseaseString)) return false;
            var altNotationPattern1 = new Regex("alternate notation$");
            var altNotationMatch = altNotationPattern1.Match(mitomapDiseaseString);
            if (!altNotationMatch.Success) return false;
            Console.WriteLine($"Alternate notation found: {mitomapDiseaseString}. This record is skipped.");
            return true;
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

        private static (string, string) GetFunctionalInfo(List<string> info, int fieldIndex)
        {
            if (fieldIndex == -1) return (null, null);
            string functionInfoString = info[fieldIndex];
            // <u>93.10%</u></a> <i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i></span>
            var regexPattern = new Regex(@"<u>(?<scoreString>[0-9.]+)%</u></a> (?<significanceString>.+)</span>$");
            var match = regexPattern.Match(functionInfoString);
            var clineSignificance = GetClinicalSignificance(match.Groups["significanceString"].Value);
            return (match.Groups["scoreString"].Value, clineSignificance);
        }

        private static string GetClinicalSignificance(string significanceString)
        {
            // < i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i>
            // filter out the symbol for frequency alert
            var arrows = significanceString.Split(@"</i>", StringSplitOptions.RemoveEmptyEntries).Where(x => !x.Contains("fa-asterisk")).ToList();
            var nArrows = arrows.Count;
            if (nArrows == 0) return null;
            var arrowType = arrows[0].Contains("fa-arrow-up") ? "up" : "down";
            return ClinicalSignificances[(arrowType, nArrows)];
        }

        private static (string RefAllele, string RawAltAllele, int? ExtractedPosition) GetRefAltAlleles(string alleleString, ISequenceProvider sequenceProvider)
        {
            var results = Evaluate_C123T(alleleString);
            if (results.Success) return (results.RefAllele, results.RawAltAllele, results.ExtractedPosition);

            results = Evaluate_16021_16022del(alleleString, sequenceProvider);
            if (results.Success) return (results.RefAllele, results.RawAltAllele, results.ExtractedPosition);

            results = Evaluate_8042del2(alleleString, sequenceProvider);
            if (results.Success) return (results.RefAllele, results.RawAltAllele, results.ExtractedPosition);

            results = Evaluate_C9537insC(alleleString);
            if (results.Success) return (results.RefAllele, results.RawAltAllele, results.ExtractedPosition);

            results = Evaluate_3902_3908invACCTTGC(alleleString, sequenceProvider);
            if (results.Success) return (results.RefAllele, results.RawAltAllele, results.ExtractedPosition);

            results = Evaluate_A_C_or_CC(alleleString);
            if (results.Success) return (results.RefAllele, results.RawAltAllele, results.ExtractedPosition);

            results = Evaluate_C_C_2_8(alleleString);
            if (results.Success) return (results.RefAllele, results.RawAltAllele, results.ExtractedPosition);

            results = Evaluate_8042delAT(alleleString, sequenceProvider);

            return results.Success
                ? (results.RefAllele, results.RawAltAllele, results.ExtractedPosition)
                : (null, null, null);
        }

        // 8042delAT
        private static (bool Success, string RefAllele, string RawAltAllele, int? ExtractedPosition) Evaluate_8042delAT(string alleleString, ISequenceProvider sequenceProvider)
        {
            var regex = new Regex(@"(?<position>^\d+)del(?<del>[ACGTacgtNn]+)");
            var match = regex.Match(alleleString);
            if (!match.Success) return (false, null, null, null);

            var extractedPosition      = int.Parse(match.Groups["position"].Value);
            string deletedSeq          = match.Groups["del"].Value;
            string deletedReferenceSeq = GetRefAllelesFromReference(sequenceProvider, extractedPosition, deletedSeq.Length);

            if (deletedSeq != deletedReferenceSeq)
            {
                throw new InvalidDataException($"Deleted sequence at {extractedPosition}: annoation is {deletedSeq}, reference sequence is {deletedReferenceSeq}");
            }

            return (true, deletedReferenceSeq, "-", extractedPosition);
        }

        // C-C(2-8)
        private static (bool Success, string RefAllele, string RawAltAllele, int? ExtractedPosition) Evaluate_C_C_2_8(string alleleString)
        {
            var regex = new Regex(@"(?<ref>[ACGTacgtNn])[_|-](?<alt>[ACGTacgtNn])\((?<min>\d+)-(?<max>\d+)\)");
            var match = regex.Match(alleleString);
            if (!match.Success) return (false, null, null, null);

            var altBase = char.Parse(match.Groups["alt"].Value);
            int minRepeat = int.Parse(match.Groups["min"].Value);
            int maxRepeat = int.Parse(match.Groups["max"].Value);
            var altAlleleSequences = new List<string>();

            for (int i = minRepeat; i <= maxRepeat; i++)
            {
                altAlleleSequences.Add(new string(altBase, i));
            }

            return (true, match.Groups["ref"].Value, string.Join(";", altAlleleSequences), null);
        }

        //A-Cor CC
        private static (bool Success, string RefAllele, string RawAltAllele, int? ExtractedPosition) Evaluate_A_C_or_CC(string alleleString)
        {
            var regex = new Regex(@"(?<ref>[ACGTacgtNn]+)[_|-](?<alt1>[ACGTacgtNn]+) ?or ?(?<alt2>[ACGTacgtNn]+)");
            var match = regex.Match(alleleString);
            if (!match.Success) return (false, null, null, null);

            var altAllele = match.Groups["alt1"].Value + ";" + match.Groups["alt2"].Value;
            return (true, match.Groups["ref"].Value, altAllele, null);
        }

        // 3902_3908invACCTTGC
        private static (bool Success, string RefAllele, string RawAltAllele, int? ExtractedPosition) Evaluate_3902_3908invACCTTGC(string alleleString, ISequenceProvider sequenceProvider)
        {
            var regex = new Regex(@"(?<start>^\d+)[_|-](?<end>\d+)inv(?<seq>[ACGTacgtNn]+)");
            var match = regex.Match(alleleString);
            if (!match.Success) return (false, null, null, null);

            var start       = int.Parse(match.Groups["start"].Value);
            var end         = int.Parse(match.Groups["end"].Value);
            var refSequence = GetRefAllelesFromReference(sequenceProvider, start, end - start + 1);
            if (refSequence != match.Groups["seq"].Value) throw new InvalidDataException($"Inconsistent sequences: reference {refSequence}, annotation {match.Groups["seq"].Value}");
            return (true, refSequence, ReverseSequence(refSequence), start);
        }

        // C9537insC
        private static (bool Success, string RefAllele, string RawAltAllele, int? ExtractedPosition) Evaluate_C9537insC(string alleleString)
        {
            var regex = new Regex(@"(?<ref>[ACGTacgtNn])(?<position>\d+)ins(?<extra>[ACGTacgtNn]+)");
            var match = regex.Match(alleleString);
            if (!match.Success) return (false, null, null, null);

            var extractedPosition = int.Parse(match.Groups["position"].Value);
            var refAllele         = match.Groups["ref"].Value;
            var altAllele         = refAllele + match.Groups["extra"].Value;
            return (true, refAllele, altAllele, extractedPosition);
        }

        // 8042del2
        private static (bool Success, string RefAllele, string RawAltAllele, int? ExtractedPosition) Evaluate_8042del2(string alleleString, ISequenceProvider sequenceProvider)
        {
            var regex = new Regex(@"(?<position>^\d+)del(?<length>\d+)");
            var match = regex.Match(alleleString);
            if (!match.Success) return (false, null, null, null);

            var extractedPosition = int.Parse(match.Groups["position"].Value);
            return (true, GetRefAllelesFromReference(sequenceProvider, extractedPosition, int.Parse(match.Groups["length"].Value)), "-", extractedPosition);
        }

        // 16021_16022del
        private static (bool Success, string RefAllele, string RawAltAllele, int? ExtractedPosition) Evaluate_16021_16022del(string alleleString, ISequenceProvider sequenceProvider)
        {
            var regex = new Regex(@"(?<start>^\d+)[_|-](?<end>\d+)del");
            var match = regex.Match(alleleString);
            if (!match.Success) return (false, null, null, null);

            var start = int.Parse(match.Groups["start"].Value);
            var end   = int.Parse(match.Groups["end"].Value);
            return (true, GetRefAllelesFromReference(sequenceProvider, start, end - start + 1), "-", start);
        }

        // C123T, A-del or A123del
        private static (bool Success, string RefAllele, string RawAltAllele, int? ExtractedPosition) Evaluate_C123T(string alleleString)
        {            
            var regex = new Regex(@"(?<ref>^[ACGTacgtNn]+)(?<position>(\d+|-))(?<alt>([ACGTBDHKMRSVWYNacgtbdhkmrsvwyn]+|:|del[ACGTacgtNn]*|d)$)");
            var match = regex.Match(alleleString);
            if (!match.Success) return (false, null, null, null);

            int? extractedPosition = null;
            if (match.Groups["position"].Value != "-") extractedPosition = int.Parse(match.Groups["position"].Value);
            return (true, match.Groups["ref"].Value, match.Groups["alt"].Value, extractedPosition);
        }

        private static string GetRefAllelesFromReference(ISequenceProvider sequenceProvider, int start,
            int length) => sequenceProvider.Sequence.Substring(start - 1, length);

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

        public static IEnumerable<MitoMapItem> GetMergeAndSortedItems(IEnumerable<MitoMapVariantReader> mitoMapMutationReaders)
        {
            var items = mitoMapMutationReaders.SelectMany(x => x.GetMitoMapItems()).ToList();
            items.ForEach(x => x.Trim());
            return items.ToLookup(x => x.Position).Select(x => MitoMapItem.AggregatedMutationsSamePosition(x.Select(i => i)).Values)
                .SelectMany(x => x).OrderBy(x => x.Position);
        }

        private static (int RefPosition, string RefAllele, string AltAllele) GetLeftAlignedVariant(int position, string refAllele, string altAllele, VariantAligner variantAligner)
        {
            if (refAllele == null || altAllele == null) return (position, refAllele, altAllele);
            if (refAllele == "-") refAllele = "";
            if (altAllele == "-") altAllele = "";
            var leftAlgnResults = variantAligner.LeftAlign(position, refAllele, altAllele); 
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
                throw new InvalidDataException($"{position}:{refAllele}:{altAllele} becomes {newPosition}:{newRefAllele}:{newAltAllele} after left alignment. Left-alignment should be only performed for deletions and insertions");
            }
            return leftAlgnResults;
        }
    }
}