using Genome;

namespace Variants
{
	public interface IBreakEnd
	{
	    IBreakEndPiece Piece1 { get; }
	    IBreakEndPiece Piece2 { get; }
	}

    public interface IBreakEndPiece
    {
        IChromosome Chromosome { get; }
        int Position { get; }
        // true means from position to end, false means from 1 to position
        bool IsSuffix { get; }
        char Orientation { get; }
    }
}