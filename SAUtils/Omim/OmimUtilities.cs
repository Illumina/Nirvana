using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CacheUtils.Genes.DataStructures;
using CacheUtils.Genes.IO;
using Compression.Utilities;
using OptimizedCore;
using SAUtils.DataStructures;
using SAUtils.Omim.EntryApiResponse;
using SAUtils.Schema;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Sequence;

namespace SAUtils.Omim
{
    public static class OmimUtilities
    {
        public static OmimItem.Phenotype GetPhenotype(PhenotypeMap phenotypeMap, SaJsonSchema jsonSchema)
        {
            var phenotypeItem = phenotypeMap.phenotypeMap;

            var (phenotype, _) = ExtractPhenotypeAndComments(phenotypeItem.phenotype);
            //Don't output any comments for now
            return new OmimItem.Phenotype(phenotypeItem.phenotypeMimNumber, phenotype, (OmimItem.Mapping)phenotypeItem.phenotypeMappingKey, OmimItem.Comments.unknown, ExtractInheritances(phenotypeItem.phenotypeInheritance), jsonSchema);
        }

        private static HashSet<string> ExtractInheritances(string inheritance)
        {
            var inheritances = new HashSet<string>();
            if (string.IsNullOrEmpty(inheritance)) return inheritances;

            foreach (var content in inheritance.OptimizedSplit(';'))
            {
                var trimmedContent = content.Trim(' ');
                inheritances.Add(trimmedContent);
            }

            return inheritances;
        }

        internal static (string Phenotype, OmimItem.Comments Comments) ExtractPhenotypeAndComments(string phenotypeString)
        {
            phenotypeString = phenotypeString.Trim(' ').Trim(',').Replace(@"\\'", "'");
            string phenotype = Regex.Replace(
                            Regex.Replace(
                            Regex.Replace(phenotypeString,
                            @"(^\?|\[|\]|{|})", ""),
                            @" \(\d\) ", " "),
                            @"^\?", "");

            var comments = OmimItem.Comments.unknown;

            if (phenotypeString.Substring(0, 2).Contains("?"))
            {
                comments = OmimItem.Comments.unconfirmed_or_possibly_spurious_mapping;
            }
            else
            {
                if (phenotypeString.OptimizedStartsWith('{'))
                {
                    comments = OmimItem.Comments.contribute_to_susceptibility_to_multifactorial_disorders_or_to_susceptibility_to_infection;
                }
                else if (phenotypeString.OptimizedStartsWith('['))
                {
                    comments = OmimItem.Comments.nondiseases;
                }
            }

            return (phenotype, comments);
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

        public static Dictionary<string, List<ISuppGeneItem>> GetGeneToOmimEntriesAndSchema(IEnumerable<OmimItem> omimItems)
        {
            var geneToOmimEntries = new Dictionary<string, List<ISuppGeneItem>>();
            SaJsonSchema jsonSchema = null;

            foreach (var item in omimItems)
            {
                if (jsonSchema == null) jsonSchema = item.JsonSchema;
                if (item.GeneSymbol == null) continue;

                if (geneToOmimEntries.TryGetValue(item.GeneSymbol, out var mimList))
                {
                    mimList.Add(item);
                }
                else
                {
                    geneToOmimEntries[item.GeneSymbol] = new List<ISuppGeneItem> { item };
                }
            }

            return geneToOmimEntries;
        }

        public static string RemoveLinksInText(string text)
        {
           if (text == null) return null;
           // remove links enclosed by parentheses with only numbers, e.g. ({12345})
           text = Regex.Replace(Regex.Replace(text, @"((and|see|;|(e\.g\.)?,) )*{\d+}", ""), @" ?\(\)", "");
           // remove format control characters
           return Regex.Replace(text, @"{(\d+:)?(.+?)}", "$2");
        }
    }
}