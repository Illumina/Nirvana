// ReSharper disable InconsistentNaming

using ErrorHandling;

namespace OrchestrationLambda
{
    public sealed class AnnotationResult
    {
        public string id;
        public string status;
        public string filePath;
        public ExitCodes exitCode;
    }
}