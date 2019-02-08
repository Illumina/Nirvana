using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.IO;
using Compression.Utilities;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Sequence;

namespace SAUtils
{
    public static class OmimUtilities
    {
        public static List<OmimItem.Phenotype> Parse(string line)
        {
            var phenotypes = new List<OmimItem.Phenotype>();

            if (string.IsNullOrEmpty(line)) return phenotypes;

            var infos = line.OptimizedSplit(';');
            phenotypes.AddRange(infos.Select(ExtractPhenotype));

            return phenotypes;
        }

        private static OmimItem.Phenotype ExtractPhenotype(string info)
        {
            info = info.Trim(' ').Replace(@"\\'", "'");

            if (string.IsNullOrWhiteSpace(info) || string.IsNullOrEmpty(info)) return null;

            var phenotypeRegex = new Regex(@"^(.+?)(?:,\s(\d{6}))?\s\((\d)\)(?:,\s)?(.*)?$");
            var match = phenotypeRegex.Match(info);
            var phenotypeGroup = match.Groups[1].ToString();
            ParsePhenotypeMapping(phenotypeGroup, out var phenotype, out var comments);

            var mimNumber = string.IsNullOrEmpty(match.Groups[2].Value) ? 0 : Convert.ToInt32(match.Groups[2].Value);
            var mapping = (OmimItem.Mapping)Convert.ToInt16(match.Groups[3].Value);

            var inheritance = string.IsNullOrEmpty(match.Groups[4].Value) ? null : match.Groups[4].ToString();
            var inheritances = ExtractInheritances(inheritance);
            return new OmimItem.Phenotype(mimNumber, phenotype, mapping, comments, inheritances);
        }

        private static HashSet<string> ExtractInheritances(string inheritance)
        {
            var inheritances = new HashSet<string>();
            if (string.IsNullOrEmpty(inheritance)) return inheritances;

            foreach (var content in inheritance.OptimizedSplit(','))
            {
                var trimmedContent = content.Trim(' ');
                inheritances.Add(trimmedContent);
            }

            return inheritances;
        }

        private static void ParsePhenotypeMapping(string phenotypeGroup, out string phenotype, out OmimItem.Comments comments)
        {
            phenotypeGroup = phenotypeGroup.Trim(' ');
            phenotype = phenotypeGroup.TrimStart('?', '{', '[').TrimEnd('}', ']');
            comments = OmimItem.Comments.unknown;

            if (phenotypeGroup.Substring(0, 2).Contains("?"))
            {
                comments = OmimItem.Comments.unconfirmed_or_possibly_spurious_mapping;
            }
            else
            {
                if (phenotypeGroup.OptimizedStartsWith('{'))
                {
                    comments = OmimItem.Comments.contribute_to_susceptibility_to_multifactorial_disorders_or_to_susceptibility_to_infection;
                }
                else if (phenotypeGroup.OptimizedStartsWith('['))
                {
                    comments = OmimItem.Comments.nondiseases;
                }
            }
        }
        public static (Dictionary<string, string> EntrezGeneIdToSymbol, Dictionary<string, string> EnsemblIdToSymbol) ParseUniversalGeneArchive(string inputReferencePath, string universalGeneArchivePath)
        {
            var (_, refNameToChromosome, _) = SequenceHelper.GetDictionaries(inputReferencePath);

            UgaGene[] genes;

            using (var reader = new UgaGeneReader(GZipUtilities.GetAppropriateReadStream(universalGeneArchivePath),
                refNameToChromosome))
            {
                genes = reader.GetGenes();
            }

            var entrezGeneIdToSymbol = genes.GetGeneIdToSymbol(x => x.EntrezGeneId);
            var ensemblIdToSymbol = genes.GetGeneIdToSymbol(x => x.EnsemblId);
            return (entrezGeneIdToSymbol, ensemblIdToSymbol);
        }
        private static Dictionary<string, string> GetGeneIdToSymbol(this UgaGene[] genes,
            Func<UgaGene, string> geneIdFunc)
        {
            var dict = new Dictionary<string, string>();
            foreach (var gene in genes)
            {
                var key = geneIdFunc(gene);
                var symbol = gene.Symbol;
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(symbol)) continue;
                dict[key] = symbol;
            }
            return dict;
        }

        public static Dictionary<string, List<ISuppGeneItem>> GetGeneToOmimEntries(IEnumerable<OmimItem> omimItems)
        {
            var geneToOmimEntries = new Dictionary<string, List<ISuppGeneItem>>();

            foreach (var item in omimItems)
            {
                if (item.GeneSymbol == null) continue;

                if (geneToOmimEntries.TryGetValue(item.GeneSymbol, out var mimList))
                {
                    if (!item.IsEmpty()) mimList.Add(item);
                }
                else
                {
                    geneToOmimEntries[item.GeneSymbol] = new List<ISuppGeneItem> { item };
                }
            }

            return geneToOmimEntries;
        }
    }
}