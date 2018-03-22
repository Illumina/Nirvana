using System.Collections.Generic;
using CommonUtilities;
using Phantom.Interfaces;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Phantom.DataStructures
{
    public sealed class PositionBuffer : IPositionBuffer
    {
        public ICodonInfoProvider CodonInfoProvider { get; }
        public IChromosome CurrentChromosome { get; private set; }
        public BufferedPositions BufferedPositions { get; private set; }
        public IIntervalForest<IGene> GeneIntervalForest { get; } // used to find overlap genes for now

        public PositionBuffer(ICodonInfoProvider codonInfoProvider, IIntervalForest<IGene> geneIntervalForest)
        {
            CodonInfoProvider = codonInfoProvider;
            GeneIntervalForest = geneIntervalForest;
            CurrentChromosome = new EmptyChromosome(string.Empty);
            BufferedPositions = BufferedPositions.CreatEmptyBufferedPositions();
        }

        public BufferedPositions AddPosition(ISimplePosition simplePosition)
        {
            bool recomposable = IsRecomposable(simplePosition);
            bool isPositionWithinRange = !simplePosition.Chromosome.IsEmpty() && PositionWithinRange(simplePosition);
            if (isPositionWithinRange)
            {
                BufferedPositions.SimplePositions.Add(simplePosition);
                BufferedPositions.Recomposable.Add(recomposable);
                if (recomposable) UpdateFunctionBlockRanges(simplePosition);
                return BufferedPositions.CreatEmptyBufferedPositions();
            }
            var copyOfBuffer = BufferedPositions;
            ResetBuffer(simplePosition, recomposable);
            return copyOfBuffer;
        }

        internal static bool IsRecomposable(ISimplePosition simplePosition)
        {
            string formatCol = simplePosition.VcfFields[VcfCommon.FormatIndex];
            return !VcfCommon.ReferenceAltAllele.Contains(simplePosition.VcfFields[VcfCommon.AltIndex]) && (formatCol.StartsWith("GT:") || formatCol.Equals("GT")) ;
        }

        private void ResetBuffer(ISimplePosition simplePosition, bool recomposable)
        {
            var functionBlockRanges = recomposable ? new List<int> { CodonInfoProvider.GetFunctionBlockRanges(simplePosition) } : new List<int>();
            BufferedPositions = new BufferedPositions(new List<ISimplePosition> { simplePosition }, new List<bool> { recomposable }, functionBlockRanges);
            CurrentChromosome = simplePosition.Chromosome;
        }

        public bool PositionWithinRange(ISimplePosition simplePosition)
        {
            int blockRangesCount = BufferedPositions.FunctionBlockRanges.Count;
            return CurrentChromosome.Index == simplePosition.Chromosome.Index && blockRangesCount != 0  && simplePosition.Start <= BufferedPositions.FunctionBlockRanges[blockRangesCount - 1] && InGeneRegion(simplePosition);
        }

        public void UpdateFunctionBlockRanges(ISimplePosition simplePosition)
        {
            BufferedPositions.FunctionBlockRanges.Add(CodonInfoProvider.GetFunctionBlockRanges(simplePosition));
        }

        public BufferedPositions Purge()
        {
            var copyOfBuffer = BufferedPositions;
            BufferedPositions = BufferedPositions.CreatEmptyBufferedPositions();
            return copyOfBuffer;
        }

        public bool InGeneRegion(ISimplePosition simplePosition) =>  GeneIntervalForest.OverlapsAny(simplePosition.Chromosome.Index, simplePosition.Start, simplePosition.End);
    }
}
