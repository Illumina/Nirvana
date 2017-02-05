using System.Collections.Generic;
using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.Interface;

namespace VariantAnnotation.Utilities
{
    public interface IPlugin
    {
        void AnnotateVariant(IVariantFeature variant, List<Transcript> transcripts,
            IAnnotatedVariant annotatedVariant, ICompressedSequence sequence);
    }
}
