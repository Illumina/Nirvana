using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace Vcf
{
    public sealed class BreakEnd:IBreakEnd
    {
		#region members

		public IChromosome Chromosome1 { get; }
	    public IChromosome Chromosome2 { get; }
	    public int Position1 { get; }
	    public int Position2 { get; }
	    public bool IsSuffix1 { get; }
	    public bool IsSuffix2 { get; }
	    public char Orientation1 { get; }
	    public char Orientation2 { get; }
		#endregion

		/// <summary>
		/// constructor
		/// </summary>
		public BreakEnd(IChromosome chromosome1, IChromosome chromosome2, int position1, int position2, bool isSuffix1,
            bool isSuffix2)
        {
	        Chromosome1 = chromosome1;
	        Chromosome2 = chromosome2;

	        Position1 = position1;
            Position2 = position2;
            IsSuffix1 = isSuffix1;
            IsSuffix2 = isSuffix2;

            Orientation1 = isSuffix1 ? '-' :'+' ;
            Orientation2 = isSuffix2 ? '+' : '-';
        }

        /// <summary>
        /// returns a string representation of this breakend
        /// </summary>
        public override string ToString()
        {
	        return $"{Chromosome1.EnsemblName}:{Position1}:{Orientation1}:{Chromosome2.EnsemblName}:{Position2}:{Orientation2}";
        }
	    
    }
}
