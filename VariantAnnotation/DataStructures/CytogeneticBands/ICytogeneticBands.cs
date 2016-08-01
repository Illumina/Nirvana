using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures.CytogeneticBands
{
    public interface ICytogeneticBands
    {
        /// <summary>
        /// returns the correct cytogenetic band representation for this chromosome given
        /// the start and end coordinates
        /// </summary>
        string GetCytogeneticBand(string ensemblRefName, int start, int end);

        /// <summary>
        /// serializes a reference cytogenetic band to disk
        /// </summary>
        void Write(ExtendedBinaryWriter writer);
    }
}
