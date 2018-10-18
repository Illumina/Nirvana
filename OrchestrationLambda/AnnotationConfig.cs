// ReSharper disable InconsistentNaming

using Cloud;
using Genome;

namespace OrchestrationLambda
{
    public sealed class AnnotationConfig
    {
        public string id;
        public string genomeAssembly;
        public S3Path inputVcf;
        public S3Path outputDir;
        public string outputPrefix;
        public string supplementaryAnnotations;
        public AnnotationRange annotationRange;
    }
}