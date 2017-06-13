using System.Collections.Generic;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.Transcript;
using VariantAnnotation.Interface;

namespace VariantAnnotation.Utilities
{
    public interface IPlugin
    {
        void AnnotateVariant(IVariantFeature variant, List<Transcript> transcripts,
            IAnnotatedVariant annotatedVariant, ICompressedSequence sequence);
    }
}
