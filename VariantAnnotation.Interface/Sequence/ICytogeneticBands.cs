using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.Sequence
{
	public interface ICytogeneticBands
	{
		/// <summary>
		/// returns the correct cytogenetic band representation for this chromosome given
		/// the start and end coordinates
		/// </summary>
		string GetCytogeneticBand(IChromosome chromosome, int start, int end);

		/// <summary>
		/// serializes a reference cytogenetic band to disk
		/// </summary>
		void Write(IExtendedBinaryWriter writer);
	}
}