using System;
using System.Collections.Generic;
using VariantAnnotation.Caches;

namespace SAUtils.PrimateAi
{
    public static class PrimateAiUtilities
    {
        public static (Dictionary<string, string> entrezToHgnc, Dictionary<string, string> ensemblToHgnc)
            GetIdToSymbols(TranscriptCacheData transcriptData)
        {
            var entrezToHgnc  = new Dictionary<string, string>();
            var ensemblToHgnc = new Dictionary<string, string>();
            foreach (var gene in transcriptData.Genes)
            {
                if(gene.EntrezGeneId.WithoutVersion == "649330")
                    Console.WriteLine("bug");
                if(! string.IsNullOrEmpty(gene.EntrezGeneId.WithoutVersion))
                    entrezToHgnc[gene.EntrezGeneId.WithoutVersion] = gene.Symbol;

                if (!string.IsNullOrEmpty(gene.EnsemblId.WithoutVersion))
                    ensemblToHgnc[gene.EnsemblId.WithoutVersion] = gene.Symbol;
            }

            return (entrezToHgnc, ensemblToHgnc);
        }
    }
}