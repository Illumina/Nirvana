using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.CombineAndUpdateGenes.DataStructures;
using VariantAnnotation.DataStructures.Transcript;

namespace CacheUtils.CombineAndUpdateGenes.Algorithms
{
    public class GeneSymbolUpdater
    {
        private readonly List<MutableGene> _genes;
        private readonly string _description;
        private readonly TranscriptDataSource _transcriptDataSource;

        private readonly SymbolDataSource _geneInfoSource;
        private readonly SymbolDataSource _hgncSource;

        /// <summary>
        /// constructor
        /// </summary>
        public GeneSymbolUpdater(List<MutableGene> genes, string description, SymbolDataSource geneInfoSource,
            SymbolDataSource hgncSource)
        {
            _genes                = genes;
            _description          = description;
            _transcriptDataSource = genes.First().TranscriptDataSource;
            _geneInfoSource       = geneInfoSource;
            _hgncSource           = hgncSource;
        }

        public void Update()
        {
            int numGenesUpdated        = 0;
            int numGenesAlreadyCurrent = 0;
            int numGenesUnableToUpdate = 0;

            foreach (var gene in _genes)
            {
                var originalSymbol = gene.Symbol;

                var geneId = _transcriptDataSource == TranscriptDataSource.Ensembl
                    ? gene.EnsemblId.ToString()
                    : gene.EntrezGeneId.ToString();

                if (_hgncSource.TryUpdateSymbol(_transcriptDataSource, gene, geneId))
                {
                    if (gene.Symbol == originalSymbol) numGenesAlreadyCurrent++;
                    else numGenesUpdated++;
                    continue;
                }

                if (_geneInfoSource.TryUpdateSymbol(_transcriptDataSource, gene, geneId))
                {
                    if (gene.Symbol == originalSymbol) numGenesAlreadyCurrent++;
                    else numGenesUpdated++;
                    continue;
                }

                numGenesUnableToUpdate++;
            }

            Console.WriteLine($"  - {_description}: {numGenesAlreadyCurrent} already current, {numGenesUpdated} updated, {numGenesUnableToUpdate} unable to update.");
        }
    }
}
