using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.Interface.Positions;

namespace Phantom.Recomposer
{
    public sealed class PositionProcessor
    {
        private readonly IVariantGenerator _variantGenerator;
        private readonly IPositionBuffer _positionBuffer;

        public PositionProcessor(IPositionBuffer positionBuffer, IVariantGenerator variantGenerator)
        {
            _positionBuffer = positionBuffer;
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