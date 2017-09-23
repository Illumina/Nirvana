using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;
using SAUtils.DataStructures;
using VariantAnnotation.Providers;

namespace SAUtils.InputFileParsers.MitoMAP
{
    public sealed class MitoMapMutationReader
    {
        private readonly FileInfo _mitoMapFileInfo;
        private const string _delSymbol = "-";
        public readonly string _dataType;
        private readonly ReferenceSequenceProvider _sequenceProvider;

        private readonly Dictionary<string, int[]> _mitoMapMutationColumnDefinitions = new Dictionary<string, int[]>
        {
            {MitoMapDataTypes.MitoMapMutationsCodingControl, new[] {0, 2, 3, 6, 7, 8, -1}},
            {MitoMapDataTypes.MitoMapMutationsRNA, new[] {0, 2, 3, 5, 6, 7, 8}},
            {MitoMapDataTypes.MitoMapPolymorphismsCoding,  new[] {0, -1, 2, -1, -1, -1, -1}},
            {MitoMapDataTypes.MitoMapPolymorphismsControl,  new[] {0, -1, 2, -1, -1, -1, -1}}
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

        private readonly HashSet<string> _mitoMapDelSymbolSet = new HashSet<string>() { ":", "del", "d" };


        public MitoMapMutationReader(FileInfo mitoMapFileInfo, ReferenceSequenceProvider sequenceProvider)
        {
            _mitoMapFileInfo = mitoMapFileInfo;
            _dataType = GetDataType();
            _sequenceProvider = sequenceProvider;
        }

        private string GetDataType()
        {
            var dataType = _mitoMapFileInfo.Name.Replace(".html", null);
            if (!_mitoMapMutationColumnDefinitions.ContainsKey(dataType)) throw new InvalidFileFormatException($"Unexpected data file: {_mitoMapFileInfo.Name}");
            return dataType;
        }

        public IEnumerator<MitoMapMutItem> GetEnumerator()
        {
            return GetMitoMapItems().GetEnumerator();
        }

        private IEnumerable<MitoMapMutItem> GetMitoMapItems()
        {
            var dataType = GetDataType();
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
                    var mitoMapMutItem = ParseLine(line, dataType);
                    if (!string.IsNullOrEmpty(mitoMapMutItem.ReferenceAllele) &&
                        !string.IsNullOrEmpty(mitoMapMutItem.ReferenceAllele)) yield return mitoMapMutItem;
                }
            }
        }

        private MitoMapMutItem ParseLine(string line, string dataType)
        {
            // line validation
            if (!(line.StartsWith("[") && line.EndsWith("],")))
                throw new InvalidFileFormatException($"Data line doesn't start with \"[\" or end with \"],\": {line}");
            /* example lines
            ["582","<a href='/MITOMAP/GenomeLoci#MTTF'>MT-TF</a>","Mitochondrial myopathy","T582C","tRNA Phe","-","+","Reported","<span style='display:inline-block;white-space:nowrap;'><a href='/cgi-bin/mitotip?pos=582&alt=C&quart=2'><u>72.90%</u></a> <i class='fa fa-arrow-up' style='color:orange' aria-hidden='true'></i></span>","0","<a href='/cgi-bin/print_ref_list?refs=90165,91590&title=RNA+Mutation+T582C' target='_blank'>2</a>"],
            ["583","<a href='/MITOMAP/GenomeLoci#MTTF'>MT-TF</a>","MELAS / MM & EXIT","G583A","tRNA Phe","-","+","Cfrm","<span style='display:inline-block;white-space:nowrap;'><a href='/cgi-bin/mitotip?pos=583&alt=A&quart=0'><u>93.10%</u></a> <i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i></span>","0","<a href='/cgi-bin/print_ref_list?refs=2066,90532,91590&title=RNA+Mutation+G583A' target='_blank'>3</a>"],
            */
            var info = line.TrimEnd(',').TrimEnd(']').Trim('[', ']').Split("\",\"").Select(x => x.Trim('"')).ToList();
            return ExtracMapMutItem(info, _mitoMapMutationColumnDefinitions[dataType]);
        }

        private MitoMapMutItem ExtracMapMutItem(List<string> info, int[] fields)
        {
            int posi = int.Parse(info[fields[0]]);
            string disease = GetDiseaseInfo(info, fields[1]);
            var (refAllele, altAllele, extractedPosi) = GetRefAltAlleles(info[fields[2]]);
            if (extractedPosi.HasValue && posi != extractedPosi)
                Console.WriteLine($"Inconsistant positions found: annotated position: {posi}; allele {info[fields[2]]}");
            if (string.IsNullOrEmpty(refAllele) && string.IsNullOrEmpty(altAllele))
                Console.WriteLine($"No reference and alternative alleles could be extracted: {posi}; allele {info[fields[2]]}");
            if (_mitoMapDelSymbolSet.Contains(altAllele)) altAllele = _delSymbol;
            bool? homoplasmy = null;
            if (fields[3] != -1 && _symbolToBools.ContainsKey(info[fields[3]])) homoplasmy = _symbolToBools[info[fields[3]]];
            bool? heteroplasmy = null;
            if (fields[4] != -1 && _symbolToBools.ContainsKey(info[fields[4]])) heteroplasmy = _symbolToBools[info[fields[4]]];
            string status = fields[5] == -1 ? null : info[fields[5]];
            var (scorePercentile, clinicalSignificance) = GetFunctionalInfo(info, fields[6]);
            return new MitoMapMutItem(posi, refAllele, altAllele, disease, homoplasmy, heteroplasmy, status, clinicalSignificance, scorePercentile);
        }

        private static string GetDiseaseInfo(List<string> info, int fieldIndex)
        {
            if (fieldIndex == -1) return null;
            string diseaseString = info[fieldIndex];
            if (String.IsNullOrEmpty(diseaseString)) return diseaseString;
            var regexPattern = new Regex(@"<a href=.+>(?<disease>\S+)</a>$");
            var match = regexPattern.Match(diseaseString);
            if (match.Groups.Count == 0)
                return diseaseString;
            return match.Groups["disease"].Value;
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

        private (string, string, int?) GetRefAltAlleles(string refPosiAltString)
        {
            var regexPattern1 = new Regex(@"(?<ref>^[ACGTacgtNn]+)(?<posi>(\d+|-))(?<alt>([ACGTacgtNn]+|:|del[ACGTacgtNn]*|d)$)");
            var match1 = regexPattern1.Match(refPosiAltString);
            if (match1.Success)
            {
                int? extractedPosi = null;
                if (match1.Groups["posi"].Value != "-") extractedPosi = int.Parse(match1.Groups["posi"].Value);
                return (match1.Groups["ref"].Value, match1.Groups["alt"].Value, extractedPosi);
            }
            // A-del or A123del
            /*var regexPattern5 = new Regex(@"(?<ref>^[ACGTacgtNn]+)(?<posi>(\d+|-))del$");
            var match5 = regexPattern5.Match(refPosiAltString);
            if (match5.Success)
            {
                return (match1.Groups["ref"].Value, "-", posi);
            }*/

            // 16021_16022del
            var regexPattern2 = new Regex(@"(?<start>^\d+)[_|-](?<end>\d+)del");
            var match2 = regexPattern2.Match(refPosiAltString);
            if (match2.Success)
            {
                var start = int.Parse(match2.Groups["start"].Value);
                var end = int.Parse(match2.Groups["end"].Value);

                return (GetRefAllelesFromReferece(_sequenceProvider, start, end - start + 1), "-", start);
            }
            // 8042del2
            var regexPattern3 = new Regex(@"(?<posi>^\d+)del(?<length>\d+)");
            var match3 = regexPattern3.Match(refPosiAltString);
            if (match3.Success)
            {
                var extractedPosi = int.Parse(match3.Groups["posi"].Value);
                return (GetRefAllelesFromReferece(_sequenceProvider, extractedPosi,
                    int.Parse(match3.Groups["length"].Value)), "-", extractedPosi);
            }
            // C9537insC
            var regexPattern4 = new Regex(@"(?<ref>[ACGTacgtNn])(?<posi>\d+)ins(?<extra>[ACGTacgtNn]+)");
            var match4 = regexPattern4.Match(refPosiAltString);
            if (match4.Success)
            {
                var extractedPosi = int.Parse(match4.Groups["posi"].Value);
                var refAllele = match4.Groups["ref"].Value;
                var altAllele = refAllele + match4.Groups["extra"].Value;
                return (refAllele, altAllele, extractedPosi);
            }
            return (null, null, null);
        }

        private string GetRefAllelesFromReferece(ReferenceSequenceProvider sequenceProvider, int start, int length)
        {
            return sequenceProvider.Sequence.Substring(start - 1, length);
        }
    }
}