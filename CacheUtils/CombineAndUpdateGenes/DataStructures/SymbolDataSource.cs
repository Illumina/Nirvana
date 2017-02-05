using System.Collections.Generic;
using VariantAnnotation.DataStructures;

namespace CacheUtils.CombineAndUpdateGenes.DataStructures
{
    public class SymbolDataSource
    {
        private readonly Dictionary<string, UniqueString> _entrezGeneIdToSymbol;
        private readonly Dictionary<string, UniqueString> _ensemblIdToSymbol;

        private readonly Dictionary<string, UniqueInt> _entrezGeneIdToHgnc;
        private readonly Dictionary<string, UniqueInt> _ensemblIdToHgnc;

        /// <summary>
        /// constructor
        /// </summary>
        public SymbolDataSource(Dictionary<string, UniqueString> entrezGeneIdToSymbol,
            Dictionary<string, UniqueString> ensemblGeneIdToSymbol, Dictionary<string, UniqueInt> entrezGeneIdToHgnc,
            Dictionary<string, UniqueInt> ensemblIdToHgnc)
        {
            _entrezGeneIdToSymbol = entrezGeneIdToSymbol;
            _ensemblIdToSymbol    = ensemblGeneIdToSymbol;
            _entrezGeneIdToHgnc   = entrezGeneIdToHgnc;
            _ensemblIdToHgnc      = ensemblIdToHgnc;
        }

        public bool TryUpdateSymbol(TranscriptDataSource source, MutableGene gene, string geneId)
        {
            var dict = source == TranscriptDataSource.Ensembl ? _ensemblIdToSymbol : _entrezGeneIdToSymbol;

            UniqueString newSymbol;
            if (!dict.TryGetValue(geneId, out newSymbol)) return false;
            if (newSymbol.HasConflict) return false;

            gene.Symbol = newSymbol.Value;
            return true;
        }

        public bool TryUpdateHgncId(string ensemblId, string entrezGeneId, MutableGene gene)
        {
            if (TryUpdateHgncId(ensemblId, _ensemblIdToHgnc, gene)) return true;
            if (TryUpdateHgncId(entrezGeneId, _entrezGeneIdToHgnc, gene)) return true;
            return false;
        }

        private static bool TryUpdateHgncId(string geneId, Dictionary<string, UniqueInt> dict, MutableGene gene)
        {
            if (geneId == null) return false;

            UniqueInt newHgncId;
            if (!dict.TryGetValue(geneId, out newHgncId)) return false;
            if (newHgncId.HasConflict) return false;

            gene.HgncId = newHgncId.Value;
            return true;
        }
    }
}
