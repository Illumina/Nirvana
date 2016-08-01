using System.Collections.Generic;

namespace VariantAnnotation.Interface
{
    /// <summary>
    /// encapsulates info fields needed for initializing the annotation source
    /// </summary>
    public interface IAnnotatorInfo
    {
        // The name of the samples to be annotated
        IEnumerable<string> SampleNames { get; }
        IEnumerable<string> BooleanArguments { get; } 
    }

    public class AnnotatorInfo : IAnnotatorInfo
    {
        public IEnumerable<string> SampleNames { get; }
        public IEnumerable<string> BooleanArguments { get; }

        /// <summary>
        /// constructor
        /// </summary>
        public AnnotatorInfo(IEnumerable<string> sampleNames, IEnumerable<string> booleanArguments)
        {
            SampleNames      = sampleNames;
            BooleanArguments = booleanArguments;
        }
    }
}