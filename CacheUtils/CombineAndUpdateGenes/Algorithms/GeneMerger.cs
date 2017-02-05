using System;
using System.Collections.Generic;
using System.Linq;
using CacheUtils.CombineAndUpdateGenes.DataStructures;
using CacheUtils.CombineAndUpdateGenes.Utilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures;

namespace CacheUtils.CombineAndUpdateGenes.Algorithms
{
    public class GeneMerger
    {
        private readonly List<MutableGene> _genes;
        private readonly List<MutableGene> _mergedGenes;

        private readonly Dictionary<string, string> _linkedEnsemblIds;

        private int _numMergedGenes;
        private int _numOrphanEnsemblGenes;
        private int _numOrphanRefSeqGenes;

        /// <summary>
        /// constructor
        /// </summary>
        public GeneMerger(List<MutableGene> genesA, List<MutableGene> genesB,
            Dictionary<string, string> linkedEnsemblIds)
        {
            _genes            = CombineGenes(genesA, genesB);
            _mergedGenes      = new List<MutableGene>();
            _linkedEnsemblIds = linkedEnsemblIds;
        }

        private static List<MutableGene> CombineGenes(List<MutableGene> genesA, List<MutableGene> genesB)
        {
            var genes = new List<MutableGene>();
            genes.AddRange(genesA);
            genes.AddRange(genesB);
            return genes.OrderBy(x => x.ReferenceIndex).ThenBy(x => x.Start).ThenBy(x => x.End).ToList();
        }

        public List<MutableGene> Merge()
        {
            _mergedGenes.Clear();
            var genesBySymbol = GeneUtilities.GetGenesBySymbol(_genes);

            foreach (var gene in _genes)
            {
                if (gene.Invalid) continue;

                List<MutableGene> genesWithSameSymbol;
                if (!genesBySymbol.TryGetValue(gene.Symbol, out genesWithSameSymbol))
                {
                    throw new UserErrorException($"Unable to find similar genes for {gene.Symbol}");
                }

                MergesGenesWithSameSymbol(gene, genesWithSameSymbol);
            }

            Console.WriteLine($"  - {_numOrphanEnsemblGenes} orphan Ensembl genes.");
            Console.WriteLine($"  - {_numOrphanRefSeqGenes} orphan RefSeq genes.");
            Console.WriteLine($"  - {_numMergedGenes} merged genes.");

            return _mergedGenes;
        }

        private void MergesGenesWithSameSymbol(MutableGene seedGene, List<MutableGene> genesWithSameSymbol)
        {
            int overlapStart, overlapEnd;
            var validGenes = GetValidGenes(seedGene, genesWithSameSymbol, out overlapStart, out overlapEnd);

            var ensemblGenes = GeneUtilities.GetGenesByDataSource(validGenes, TranscriptDataSource.Ensembl);
            var refSeqGenes  = GeneUtilities.GetGenesByDataSource(validGenes, TranscriptDataSource.RefSeq);

            var ensemblFlattener = new GeneFlattener(ensemblGenes, "Ensembl", false);
            var flatEnsemblGenes = ensemblFlattener.Flatten(overlapStart, overlapEnd);

            var refSeqFlattener = new GeneFlattener(refSeqGenes, "RefSeq", false);
            var flatRefSeqGenes = refSeqFlattener.Flatten(overlapStart, overlapEnd);

            foreach (var ensemblGene in flatEnsemblGenes)
            {
                // add the unused Ensembl genes
                string linkedEntrezId;
                if (!_linkedEnsemblIds.TryGetValue(ensemblGene.EnsemblId.ToString(), out linkedEntrezId))
                {
                    AddEnsemblOrphan(ensemblGene);
                    continue;
                }

                var refSeqGene = GeneUtilities.GetRefSeqGeneById(flatRefSeqGenes, linkedEntrezId);

                if (refSeqGene == null)
                {
                    AddEnsemblOrphan(ensemblGene);
                    continue;
                }

                // merge the Ensembl and RefSeq gene
                var mergedGene = MutableGene.Clone(ensemblGene);
                mergedGene.TranscriptDataSource = TranscriptDataSource.BothRefSeqAndEnsembl;
                UpdateCoordinates(refSeqGene, mergedGene);

                if (mergedGene.HgncId == -1 && refSeqGene.HgncId != -1) mergedGene.HgncId = refSeqGene.HgncId;
                mergedGene.EntrezGeneId = refSeqGene.EntrezGeneId;
                _mergedGenes.Add(mergedGene);

                refSeqGene.Invalid  = true;
                ensemblGene.Invalid = true;
                _numMergedGenes++;
            }

            // add the unused RefSeq genes
            foreach (var refSeqGene in flatRefSeqGenes)
            {
                if (refSeqGene.Invalid) continue;
                AddRefSeqOrphan(refSeqGene);
            }
        }

        private static void UpdateCoordinates(MutableGene source, MutableGene dest)
        {
            if (source.Start < dest.Start) dest.Start = source.Start;
            if (source.End > dest.End) dest.End = source.End;
        }

        private void AddEnsemblOrphan(MutableGene gene)
        {
            _mergedGenes.Add(gene);
            gene.Invalid = true;
            _numOrphanEnsemblGenes++;
        }

        private void AddRefSeqOrphan(MutableGene gene)
        {
            _mergedGenes.Add(gene);
            gene.Invalid = true;
            _numOrphanRefSeqGenes++;
        }

        private List<MutableGene> GetValidGenes(MutableGene seedGene, List<MutableGene> genes, out int start,
            out int end)
        {
            var validGenes = new List<MutableGene>();
            start          = seedGene.Start;
            end            = seedGene.End;

            foreach (var gene in genes)
            {
                if (gene.Invalid || seedGene.OnReverseStrand != gene.OnReverseStrand ||
                    seedGene.ReferenceIndex != gene.ReferenceIndex || !Overlap.Partial(start, end, gene.Start, gene.End))
                    continue;

                validGenes.Add(gene);

                if (gene.Start < start) start = gene.Start;
                if (gene.End > end) end = gene.End;
            }

            return validGenes;
        }
    }
}
