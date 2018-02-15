using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface ITranslation
    {
        ITranscriptRegion CodingRegion { get; }
		ICompactId ProteinId { get; }
	    string PeptideSeq { get; }
        void Write(IExtendedBinaryWriter writer, int peptideIndex);
    }
}