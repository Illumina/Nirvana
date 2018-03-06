using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VariantAnnotation.GeneAnnotation;

namespace SAUtils.CreateOmimTsv
{
    public static class OmimPhenotype
    {
        public static List<OmimEntry.Phenotype> Parse(string line)
        {
            var phenotypes = new List<OmimEntry.Phenotype>();

            if (string.IsNullOrEmpty(line)) return phenotypes;

            var infos = line.Split(';');
            phenotypes.AddRange(infos.Select(ExtractPhenotype));

            return phenotypes;
        }

        private static OmimEntry.Phenotype ExtractPhenotype(string info)
        {
            info = info.Trim(' ').Replace(@"\\'", "'");

            if (string.IsNullOrWhiteSpace(info) || string.IsNullOrEmpty(info)) return null;

            var phenotypeRegex = new Regex(@"^(.+?)(?:,\s(\d{6}))?\s\((\d)\)(?:,\s)?(.*)?$");
            var match          = phenotypeRegex.Match(info);
            var phenotypeGroup = match.Groups[1].ToString();
            ParsePhenotypeMapping(phenotypeGroup, out var phenotype, out var comments);

            var mimNumber = string.IsNullOrEmpty(match.Groups[2].Value) ? 0 : Convert.ToInt32(match.Groups[2].Value);
            var mapping   = (OmimEntry.Mapping)Convert.ToInt16(match.Groups[3].Value);

            var inheritance  = string.IsNullOrEmpty(match.Groups[4].Value) ? null : match.Groups[4].ToString();
            var inheritances = ExtractInheritances(inheritance);
            return new OmimEntry.Phenotype(mimNumber, phenotype, mapping, comments, inheritances);
        }

        private static HashSet<string> ExtractInheritances(string inheritance)
        {
            var inheritances = new HashSet<string>();
            if (string.IsNullOrEmpty(inheritance)) return inheritances;

            foreach (var content in inheritance.Split(','))
            {
                var trimmedContent = content.Trim(' ');
                inheritances.Add(trimmedContent);
            }

            return inheritances;
        }

        private static void ParsePhenotypeMapping(string phenotypeGroup, out string phenotype, out OmimEntry.Comments comments)
        {
            phenotypeGroup = phenotypeGroup.Trim(' ');
            phenotype      = phenotypeGroup.TrimStart('?', '{', '[').TrimEnd('}', ']');
            comments       = OmimEntry.Comments.unknown;

            if (phenotypeGroup.Substring(0, 2).Contains("?"))
            {
                comments = OmimEntry.Comments.unconfirmed_or_possibly_spurious_mapping;
            }
            else
            {
                if (phenotypeGroup.StartsWith("{"))
                {
                    comments = OmimEntry.Comments.contribute_to_susceptibility_to_multifactorial_disorders_or_to_susceptibility_to_infection;
                }
                else if (phenotypeGroup.StartsWith("["))
                {
                    comments = OmimEntry.Comments.nondiseases;
                }
            }
        }
    }
}
