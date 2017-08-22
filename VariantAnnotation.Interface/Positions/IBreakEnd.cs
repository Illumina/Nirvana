using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Interface.Positions
{
	public interface IBreakEnd
	{
		IChromosome Chromosome1 { get; }
		IChromosome Chromosome2 { get; }

		int Position1 { get; }
		int Position2 { get; }

		bool IsSuffix1 { get; } // '+' means from position to end, '-' means from 1 to position
		bool IsSuffix2 { get; }

		char Orientation1 { get; }  // '+' means forward, "-" means reverse
		char Orientation2 { get; }

	}
}