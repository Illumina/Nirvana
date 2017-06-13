using VariantAnnotation.DataStructures.CytogeneticBands;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures.CompressedSequence
{
    public interface ICompressedSequence
    {
        ChromosomeRenamer Renamer { get; set; }
        ICytogeneticBands CytogeneticBands { get; set; }
        GenomeAssembly GenomeAssembly { get; set; }
        int NumBases { get; }
        void Set(int numBases, byte[] buffer, IIntervalSearch<MaskedEntry> maskedIntervalSearch, int sequenceOffset = 0);
        string Substring(int offset, int length);
	    bool Validate(int start, int end, string testSequence);
    }
}
