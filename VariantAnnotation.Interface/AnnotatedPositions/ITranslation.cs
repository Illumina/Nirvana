using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface ITranslation
    {
	    ICdnaCoordinateMap CodingRegion { get; }
		ICompactId ProteinId { get; }
		byte ProteinVersion { get; }
	    string PeptideSeq { get; }
        void Write(IExtendedBinaryWriter writer, int peptideIndex);
    }
}