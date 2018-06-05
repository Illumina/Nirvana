using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;

namespace Phantom.Recomposer
{
    public sealed class BufferedPositions : IBufferedPositions
    {
        public List<ISimplePosition> SimplePositions { get; }
        public List<bool> Recomposable { get; }
        public List<int> FunctionBlockRanges { get; } // only calculated for recomposable positions

        public BufferedPositions(List<ISimplePosition> simplePositions, List<bool> recomposable, List<int> functionBlockRanges)
        {
            SimplePositions = simplePositions;
            Recomposable = recomposable;
            FunctionBlockRanges = functionBlockRanges;
        }

        public List<ISimplePosition> GetRecomposablePositions()
        {
            var recomposablePositions = new List<ISimplePosition>();
            for (int index = 0; index < SimplePositions.Count; index++)
            {
                if (Recomposable[index])
                    recomposablePositions.Add(SimplePositions[index]);
            }
            return recomposablePositions;
        }

        public  static BufferedPositions CreatEmptyBufferedPositions() => new BufferedPositions(new List<ISimplePosition>(), new List<bool>(), new List<int>());
    }
}
