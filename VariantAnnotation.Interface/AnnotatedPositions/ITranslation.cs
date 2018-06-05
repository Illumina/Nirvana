using IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface ITranslation
    {
        ICodingRegion CodingRegion { get; }
		ICompactId ProteinId { get; }
	    string PeptideSeq { get; }
        void Write(IExtendedBinaryWriter writer, int peptideIndex);
    }
}