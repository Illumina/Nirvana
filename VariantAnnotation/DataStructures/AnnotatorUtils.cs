using System.Collections.Generic;
using VariantAnnotation.Interface;

namespace VariantAnnotation.DataStructures
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
        public string SupplementaryAnnotation { get; }
        public IEnumerable<string> CustomAnnotation { get; }
        public IEnumerable<string> CustomIntervals { get; }

        public AnnotatorPath(string cachePrefix, string compressedReference, string suppAnnotation = null,
            IEnumerable<string> customAnnotation = null, IEnumerable<string> customIntervals = null)
        {
            CachePrefix = cachePrefix;
            CompressedReference = compressedReference;
            SupplementaryAnnotation = suppAnnotation;
            CustomAnnotation = customAnnotation;
            CustomIntervals = customIntervals;
        }
    }
}
