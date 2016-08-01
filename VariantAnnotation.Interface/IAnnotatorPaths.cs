using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    /// <summary>
    /// Encapsulates all the various paths required to initialize an annotation source
    /// </summary>
    public interface IAnnotatorPaths
    {
        string Cache { get; }
        string CompressedReference { get; }
        string SupplementaryAnnotation { get; }
        IEnumerable<string> CustomAnnotation { get; }
        IEnumerable<string> CustomIntervals { get; }
    }

    public class AnnotatorPaths : IAnnotatorPaths
    {
        public string Cache { get; }
        public string CompressedReference { get; }
        public string SupplementaryAnnotation { get; }
        public IEnumerable<string> CustomAnnotation { get; }
        public IEnumerable<string> CustomIntervals { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public AnnotatorPaths(string cache, string compressedReference, string suppAnnotation = null,
            IEnumerable<string> customAnnotation = null, IEnumerable<string> customIntervals = null)
        {
            Cache                   = cache;
            CompressedReference     = compressedReference;
            SupplementaryAnnotation = suppAnnotation;
            CustomAnnotation        = customAnnotation;
            CustomIntervals         = customIntervals;
        }
    }
}