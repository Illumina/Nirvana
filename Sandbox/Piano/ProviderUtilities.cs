using VariantAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using VariantAnnotation.Utilities;

namespace Piano
{
    public static class ProviderUtilities
    {
        public static ISequenceProvider GetSequenceProvider(string compressedReferencePath)
        {
            return new ReferenceSequenceProvider(FileUtilities.GetReadStream(compressedReferencePath));
        }

        public static IAnnotationProvider GetTranscriptAnnotationProvider(string path, ISequenceProvider sequenceProvider)
        {
            return new PianoAnnotationProvider(path, sequenceProvider);
        }

        public static IAnnotator GetAnnotator(IAnnotationProvider taProvider, ISequenceProvider sequenceProvider)
        {
            return new PianoAnnotator(taProvider, sequenceProvider);
        }
    }
}