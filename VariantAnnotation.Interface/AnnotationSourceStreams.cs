using System.IO;

namespace VariantAnnotation.Interface
{
    public class AnnotationSourceStreams
    {
        public readonly Stream Transcript;
        public readonly Stream Sift;
        public readonly Stream PolyPhen;
        public readonly Stream CompressedSequence;

        /// <summary>
        /// constructor
        /// </summary>
        public AnnotationSourceStreams(Stream transcript, Stream sift, Stream polyPhen, Stream compressedSequence)
        {
            Transcript         = transcript;
            Sift               = sift;
            PolyPhen           = polyPhen;
            CompressedSequence = compressedSequence;
        }
    }
}