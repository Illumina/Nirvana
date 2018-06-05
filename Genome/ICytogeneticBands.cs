using IO;

namespace Genome
{
    public interface ICytogeneticBands : ISerializable
    {
        /// <summary>
        /// returns the correct cytogenetic band representation for this chromosome given
        /// the start and end coordinates
        /// </summary>
        string GetCytogeneticBand(IChromosome chromosome, int start, int end);
    }
}