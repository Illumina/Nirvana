using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ErrorHandling.Exceptions;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Positions;

namespace SAUtils.InputFileParsers.MitoMAP
{
    public class MitoMapReader
    {
        private FileInfo _mitoMapFileInfo;

        private readonly HashSet<string> _mitoMapMutationDataTypes = new HashSet<string>()
        {
            "MutationsCodingControl",
            "MutationsRNA",
            "PolymorphismsCoding",
            "PolymorphismsControl"
        };

        public MitoMapReader(FileInfo mitoMapFileInfo)
        {
            _mitoMapFileInfo = mitoMapFileInfo;
        }

        private string getDataType()
        {
            var dataType = _mitoMapFileInfo.Name.Replace(".HTML", null);
            if (!_mitoMapMutationDataTypes.Contains(dataType)) throw new InvalidFileFormatException($"Unexpected data file: {_mitoMapFileInfo.Name}");
            return dataType;
        }

        public IEnumerator<MitoMapItem> GetEnumerator()
        {
            return GetMitoMapItems().GetEnumerator();
        }

        private IEnumerable<MitoMapItem> GetMitoMapItems()
        {
            var dataType = getDataType();
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

        private MitoMapItem parseLine(string line, string dataType)
        {
            // line validation
            if (!(line.StartsWith("[") && line.EndsWith("],")))
                throw new InvalidFileFormatException($"Data line doesn't start with \"[\" or end with \"],\": {line}");
            /* example lines
            ["582","<a href='/MITOMAP/GenomeLoci#MTTF'>MT-TF</a>","Mitochondrial myopathy","T582C","tRNA Phe","-","+","Reported","<span style='display:inline-block;white-space:nowrap;'><a href='/cgi-bin/mitotip?pos=582&alt=C&quart=2'><u>72.90%</u></a> <i class='fa fa-arrow-up' style='color:orange' aria-hidden='true'></i></span>","0","<a href='/cgi-bin/print_ref_list?refs=90165,91590&title=RNA+Mutation+T582C' target='_blank'>2</a>"],
            ["583","<a href='/MITOMAP/GenomeLoci#MTTF'>MT-TF</a>","MELAS / MM & EXIT","G583A","tRNA Phe","-","+","Cfrm","<span style='display:inline-block;white-space:nowrap;'><a href='/cgi-bin/mitotip?pos=583&alt=A&quart=0'><u>93.10%</u></a> <i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i><i class='fa fa-arrow-up' style='color:red' aria-hidden='true'></i></span>","0","<a href='/cgi-bin/print_ref_list?refs=2066,90532,91590&title=RNA+Mutation+G583A' target='_blank'>3</a>"],
            */
            var info = line.Split(",").Select(x => x.Trim('"')).ToList();
            int posi;
            string disease;
            string refAllele;
            string altAllele;
            switch (dataType)
            {
                case "MutationsCodingControl":

                    break;
                case "MutationsRNA":
                    posi = int.Parse(info[0]);
                    disease = info[2];
                    getRefAltAlleles(info[3]);
                    break;
                case "PolymorphismsCoding":
                    break;
                case "PolymorphismsControl":
                    break;
            }


        }

        private Tuple<string, string> getRefAltAlleles(string refPosiAltString)
        {
            var regex = new Regex(@"(^[A-Za-z]+)(\d+)([A-Za-z]+$)");
            var matches = regex.Matches(refPosiAltString)
            if 
        }

        public static string TrimEnd(string sourceString, string toTrime)
        {
            if (!sourceString.EndsWith(toTrime)) return sourceString;
            return sourceString.Remove(sourceString.LastIndexOf(toTrime, StringComparison.Ordinal));
        }


    }
}