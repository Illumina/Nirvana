using System.Collections.Generic;
using System.Linq;
using Phantom.DataStructures;
using Phantom.Interfaces;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.IO;
using Vcf;

namespace Phantom.Workers
{
    public sealed class PositionProcessor
    {
        private readonly ICodonInfoProvider _codonInfoProvider;
        private readonly IVariantGenerator _variantGenerator;
        private readonly IPositionBuffer _positionBuffer;

        public PositionProcessor(IPositionBuffer positionBuffer, ICodonInfoProvider codonInfoProvider, IVariantGenerator variantGenerator)
        {
            _positionBuffer = positionBuffer;
            _codonInfoProvider = codonInfoProvider;
            _variantGenerator = variantGenerator;
        }
        public IEnumerable<ISimplePosition> Process(ISimplePosition simplePosition) => GenerateOutput(_positionBuffer.AddPosition(simplePosition));

        public IEnumerable<ISimplePosition> ProcessBufferedPositions() => GenerateOutput(_positionBuffer.Purge());

        internal IEnumerable<ISimplePosition> GenerateOutput(BufferedPositions bufferedPositions)
        {

            if (bufferedPositions.SimplePositions.Count == 0) return new List<ISimplePosition>(); // nothing to output

            var recomposablePositions = bufferedPositions.GetRecomposablePositions();
            if (recomposablePositions.Count <= 1) return bufferedPositions.SimplePositions; // nothing to recompose

            var functionBlockRanges = bufferedPositions.FunctionBlockRanges;
            var recomposedPositions = _variantGenerator.Recompose(recomposablePositions, functionBlockRanges).ToList();
            if (recomposedPositions.Count == 0) return bufferedPositions.SimplePositions; // nothing has been recomposed

            return bufferedPositions.SimplePositions.Concat(recomposedPositions).OrderBy(x => x.Start);
        }
    }
}