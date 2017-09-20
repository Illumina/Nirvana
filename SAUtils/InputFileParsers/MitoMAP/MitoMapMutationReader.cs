using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Positions;

namespace SAUtils.InputFileParsers.MitoMAP
{
    public sealed class MitoMapMutationReader
    {
        private readonly FileInfo _mitoMapFileInfo;

        private readonly Dictionary<string, int[]> _mitoMapMutationDataTypes = new Dictionary<string, int[]>()
        {
            {"MutationsCodingControl", new[] {0, 2, 3, 6, 7, 8, -1}},
            {"MutationsRNA", new[] {0, 2, 3, 5, 6, 7, 8}},
            {"PolymorphismsCoding",  new[] {0, -1, 2, -1, -1, -1, -1}},
            {"PolymorphismsControl",  new[] {0, -1, 2, -1, -1, -1, -1}}
        };

        private readonly Dictionary<(string, int), string> _clininalSigfincances = new Dictionary<(string, int), string>()
        {
            {("up", 3), "confirmed pathogenic"},
            {("up", 2), "likely pathogenic"},
            {("up", 1), "possibly pathogenic"},
            {("down", 1), "possibly benign"},
            {("down", 2), "likely benign"}
        };

        private readonly Dictionary<string, bool> _symbolToBools = new Dictionary<string, bool>()
        {
            {"+", true},
            {"-", false}
        };

        public MitoMapMutationReader(FileInfo mitoMapFileInfo)
        {
            _mitoMapFileInfo = mitoMapFileInfo;
        }

        private string GetDataType()
        {
            var dataType = _mitoMapFileInfo.Name.Replace(".HTML", null);
            if (!_mitoMapMutationDataTypes.ContainsKey(dataType)) throw new InvalidFileFormatException($"Unexpected data file: {_mitoMapFileInfo.Name}");
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
                    if (line.StartsWith("[") & line.EndsWith("]],")) isDataLine = false;
                    yield return parseLine(line, dataType);
                }
            }
        }

        private MitoMapMutItem parseLine(string line, string dataType)
        {
            // line validation
            if (!(line.StartsWith("[") && line.EndsWith("],")))
                throw new InvalidFileFormatException($"Data line doesn't start with \"[\" or end with \"],\": {line}");
            /* example lines
            ["582","<a href='/MITOMAP/GenomeLoci#MTTF'>MT-TF</a>","Mitochondrial myopathy","T582C","tRNA Phe","-","+","Reported","<span style='display:inline-block;white-space:nowrap;'><a href='/cgi-bin/mitotip?pos=582&alt=C&quart=2'><u>72.90%</u></a> <i class='fa fa-arrow-up' style='color:orange' aria-hidden='true'></i></span>","0","<a href='/cgi-bin/print_ref_list?refs=90165,91590&title=RNA+Mutation+T582C' target='_blank'>2</a>"],
            ["583","<a href='/MITOMAP/GenomeLoci#MTTF'>MT-TF</a>","MELAS / MM & EXIT","G583A","tRNA Phe","-","+","Cfrm","<span style='display:inline-block;white-space:nowrap;'><a href='/cgi-bin/mitotip?pos=583&alt=A&quart=0'><u>93.10%</u></a> <i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i></span>","0","<a href='/cgi-bin/print_ref_list?refs=2066,90532,91590&title=RNA+Mutation+G583A' target='_blank'>3</a>"],
            */
            var info = line.Split(",").Select(x => x.Trim('"')).ToList();
            return ExtracMapMutItem(info, _mitoMapMutationDataTypes[dataType]);
        }

        private MitoMapMutItem ExtracMapMutItem(List<string> info, int[] fields)
        {
            int posi = int.Parse(info[fields[0]]);
            string disease = GetDiseaseInfo(info, fields[1]);
            var (refAllele, altAllele) = GetRefAltAlleles(info[fields[2]]);
            bool? homoplasmy = null;
            if (fields[3] != -1 && _symbolToBools.ContainsKey(info[fields[3]])) homoplasmy = _symbolToBools[info[fields[3]]];
            bool? heteroplasmy = null;
            if (fields[4] != -1 && _symbolToBools.ContainsKey(info[fields[4]])) heteroplasmy = _symbolToBools[info[fields[4]]];
            string status = fields[5] == -1 ? null : info[fields[5]];
            var (clinicalSignificance, scorePercentile) = GetFunctionalInfo(info, fields[6]);
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
            var arrowType = arrows[0].Contains("fa-arrow-up") ? "up" : "down";
            return _clininalSigfincances[(arrowType, nArrows)];
        }

        private (string, string) GetRefAltAlleles(string refPosiAltString)
        {
            var regexPattern = new Regex(@"(?<ref>^[A-Za-z]+)(?<posi>(\d+|-))(?<alt>[A-Za-z]+$)");
            var match = regexPattern.Match(refPosiAltString);
            if (match.Groups.Count == 0) throw new FormatException($"No reference and alternative alleles could be extracted: {refPosiAltString}");
            return (match.Groups["ref"].Value, match.Groups["alt"].Value);
        }
    }
}