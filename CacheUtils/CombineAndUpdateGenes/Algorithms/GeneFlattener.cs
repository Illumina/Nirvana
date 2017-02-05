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
    public class GeneFlattener
    {
        private readonly List<MutableGene> _genes;
        private readonly string _description;
        private readonly bool _isEnsembl;

        private readonly bool _showOutput;

        /// <summary>
        /// constructor
        /// </summary>
        public GeneFlattener(List<MutableGene> genes, string description, bool showOutput = true)
        {
            _genes       = genes;
            _description = description;
            _isEnsembl   = genes.Count == 0 || genes.First().TranscriptDataSource == TranscriptDataSource.Ensembl;
            _showOutput  = showOutput;
        }

        public List<MutableGene> Flatten(int overlapStart = -1, int overlapEnd = -1)
        {
            var combinedGenes = new List<MutableGene>();
            var genesById     = GeneUtilities.GetGenesById(_genes, _isEnsembl);

            foreach (var gene in _genes)
            {
                if (gene.Invalid) continue;

                var geneId = _isEnsembl
                    ? gene.EnsemblId.ToString()
                    : gene.EntrezGeneId.ToString();

                List<MutableGene> genesWithSameGeneId;
                if (!genesById.TryGetValue(geneId, out genesWithSameGeneId))
                {
                    throw new UserErrorException($"Unable to find similar genes for {geneId}");
                }

                combinedGenes.Add(GetFlattenedGene(gene, genesWithSameGeneId, overlapStart, overlapEnd));
            }

            if(_showOutput) Console.WriteLine($"  - {_description}: {combinedGenes.Count} genes.");

            return combinedGenes;
        }

        private static MutableGene GetFlattenedGene(MutableGene seedGene, List<MutableGene> genesWithSameGeneId,
            int overlapStart, int overlapEnd)
        {
            var flattenedGene = MutableGene.Clone(seedGene);
            bool useOverlap   = overlapStart != -1 && overlapEnd != -1;

            foreach (var gene in genesWithSameGeneId)
            {
                if (gene.Invalid || flattenedGene.OnReverseStrand != gene.OnReverseStrand ||
                    flattenedGene.ReferenceIndex != gene.ReferenceIndex) continue;

                if (useOverlap  && !Overlap.Partial(overlapStart, overlapEnd, gene.Start, gene.End)) continue;
                if (!useOverlap && !Overlap.Partial(flattenedGene.Start, flattenedGene.End, gene.Start, gene.End)) continue;

                UpdateCoordinates(gene, flattenedGene);
                gene.Invalid = true;
            }

            return flattenedGene;
        }

        private static void UpdateCoordinates(MutableGene source, MutableGene dest)
        {
            if (source.Start < dest.Start) dest.Start = source.Start;
            if (source.End > dest.End) dest.End = source.End;
        }
    }
}
