using System;
using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures;

namespace CacheUtils.Helpers
{
    public static class GeneSymbolSourceHelper
    {
        private static readonly Dictionary<string, GeneSymbolSource> StringToGeneSymbolSources;

        static GeneSymbolSourceHelper()
        {
            StringToGeneSymbolSources = new Dictionary<string, GeneSymbolSource>
            {
                ["Clone_based_ensembl_gene"] = GeneSymbolSource.CloneBasedEnsemblGene,
                ["Clone_based_vega_gene"]    = GeneSymbolSource.CloneBasedVegaGene,
                ["EntrezGene"]               = GeneSymbolSource.EntrezGene,
                ["HGNC"]                     = GeneSymbolSource.HGNC,
                ["LRG"]                      = GeneSymbolSource.LRG,
                ["miRBase"]                  = GeneSymbolSource.miRBase,
                ["NCBI"]                     = GeneSymbolSource.NCBI,
                ["RFAM"]                     = GeneSymbolSource.RFAM,
                ["Uniprot_gn"]               = GeneSymbolSource.UniProtGeneName
            };
        }

        public static GeneSymbolSource GetGeneSymbolSource(string s)
        {
            if (s == null) return GeneSymbolSource.Unknown;
            if (!StringToGeneSymbolSources.TryGetValue(s, out var ret)) throw new InvalidOperationException($"The specified gene symbol source ({s}) was not found in the GeneSymbolSource enum.");
            return ret;
        }
    }
}
