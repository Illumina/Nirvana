using System.Collections.Generic;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures.Annotation
{
    public class AnnotatorInfo : IAnnotatorInfo
    {
        public IEnumerable<string> SampleNames { get; }
        public IEnumerable<string> BooleanArguments { get; }

        public AnnotatorInfo(IEnumerable<string> sampleNames, IEnumerable<string> booleanArguments)
        {
            SampleNames = sampleNames;
            BooleanArguments = booleanArguments;
        }
    }

    public class AnnotatorPath : IAnnotatorPaths
    {
        public string CachePrefix { get; }
        public string CompressedReference { get; }
        public IEnumerable<string> SupplementaryAnnotations { get; }

        public AnnotatorPath(string cachePrefix, string compressedReference, IEnumerable<string> suppAnnotations = null)
        {
            CachePrefix = cachePrefix;
            CompressedReference = compressedReference;
            SupplementaryAnnotations = suppAnnotations;
        }
    }
}
