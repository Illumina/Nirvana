// ReSharper disable InconsistentNaming
using Cloud;

namespace OrchestrationLambda
{
    public sealed class NirvanaConfig
    {
        public string id;
        public string genomeAssembly;
        public S3Path inputVcf;
        public S3Path outputDir;
        public string supplementaryAnnotations;
    }
}