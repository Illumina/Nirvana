using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Vcf
{
    public sealed class BreakEnd : IBreakEnd
    {
        public IBreakEndPiece Piece1 { get; }
        public IBreakEndPiece Piece2 { get; }

        public BreakEnd(IChromosome chromosome, IChromosome chromosome2, int position, int position2, bool isSuffix,
            bool isSuffix2)
        {
            var orientation1 = isSuffix ? '-' : '+';
            var orientation2 = isSuffix2 ? '+' : '-';
            Piece1 = new BreakEndPiece(chromosome, position, isSuffix, orientation1);
            Piece2 = new BreakEndPiece(chromosome2, position2, isSuffix2, orientation2);
        }

        public override string ToString() =>
            $"{Piece1.Chromosome.EnsemblName}:{Piece1.Position}:{Piece1.Orientation}:{Piece2.Chromosome.EnsemblName}:{Piece2.Position}:{Piece2.Orientation}";
    }

    public sealed class BreakEndPiece : IBreakEndPiece
    {
        public IChromosome Chromosome { get; }
        public int Position { get; }
        public bool IsSuffix { get; }
        public char Orientation { get; }

        public BreakEndPiece(IChromosome chromosome, int position, bool isSuffix, char orientation)
        {
            Chromosome  = chromosome;
            Position    = position;
            IsSuffix    = isSuffix;
            Orientation = orientation;
        }
    }
}
