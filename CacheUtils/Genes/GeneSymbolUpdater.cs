using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.Genes.DataStructures;
using VariantAnnotation.Interface;

namespace CacheUtils.Genes
{
    public sealed class GeneSymbolUpdater
    {
        private int _numUpdatedByHgncId;
        private int _numUpdatedByEntrezGeneId;
        private int _numUpdatedByEnsemblId;
        private int _numUpdatedByRefSeqGff;

        private readonly ILogger _logger;
        private readonly Dictionary<int, string> _hgncIdToSymbol;
        private readonly Dictionary<string, string> _entrezGeneIdToSymbol;
        private readonly Dictionary<string, string> _ensemblIdToSymbol;
        private readonly Dictionary<string, string> _refseqGeneIdToSymbol;

        public GeneSymbolUpdater(ILogger logger, Dictionary<int, string> hgncIdToSymbol,
            Dictionary<string, string> entrezGeneIdToSymbol, Dictionary<string, string> ensemblIdToSymbol,
            Dictionary<string, string> refseqGeneIdToSymbol)
        {
            _logger               = logger;
            _hgncIdToSymbol       = hgncIdToSymbol;
            _entrezGeneIdToSymbol = entrezGeneIdToSymbol;
            _ensemblIdToSymbol    = ensemblIdToSymbol;
            _refseqGeneIdToSymbol = refseqGeneIdToSymbol;
        }

        public void Update(UgaGene[] mergedGenes)
        {
            _logger.Write("- updating gene symbols... ");
            foreach (var gene in mergedGenes) UpdateGeneSymbol(gene);
            _logger.WriteLine($"{_numUpdatedByHgncId} by HGNC id, {_numUpdatedByEntrezGeneId} by Entrez Gene ID, {_numUpdatedByEnsemblId} by Ensembl ID, {_numUpdatedByRefSeqGff} by RefSeq GFF");

            int numGenesMissingSymbol = mergedGenes.Count(gene => string.IsNullOrEmpty(gene.Symbol));
            if (numGenesMissingSymbol > 0) throw new InvalidDataException($"{numGenesMissingSymbol} genes are missing symbols.");
        }

        private void UpdateGeneSymbol(UgaGene gene)
        {
            string originalSymbol = gene.Symbol;
            bool isUpdated = UpdateBySymbolDict(gene, x => x.HgncId, x => x == -1, _hgncIdToSymbol);

            if (isUpdated)
            {
                if (gene.Symbol != originalSymbol) _numUpdatedByHgncId++;
                return;
            }

            isUpdated = UpdateBySymbolDict(gene, x => x.EntrezGeneId, string.IsNullOrEmpty, _entrezGeneIdToSymbol);

            if (isUpdated)
            {
                if (gene.Symbol != originalSymbol) _numUpdatedByEntrezGeneId++;
                return;
            }

            isUpdated = UpdateBySymbolDict(gene, x => x.EnsemblId, string.IsNullOrEmpty, _ensemblIdToSymbol);

            if (isUpdated)
            {
                if (gene.Symbol != originalSymbol) _numUpdatedByEnsemblId++;
                return;
            }

            isUpdated = UpdateBySymbolDict(gene, x => x.EntrezGeneId, string.IsNullOrEmpty, _refseqGeneIdToSymbol);

            // ReSharper disable once InvertIf
            if (isUpdated && gene.Symbol != originalSymbol) _numUpdatedByRefSeqGff++;
        }

        private static bool UpdateBySymbolDict<T>(UgaGene gene, Func<UgaGene, T> idFunc, Func<T, bool> isEmpty, IReadOnlyDictionary<T, string> idToSymbol)
        {
            var key = idFunc(gene);
            if (isEmpty(key)) return false;

            if (!idToSymbol.TryGetValue(idFunc(gene), out string symbol)) return false;
            gene.Symbol = symbol;
            return true;
        }
    }
}
