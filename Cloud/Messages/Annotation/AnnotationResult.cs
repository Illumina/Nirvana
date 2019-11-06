using ErrorHandling;

namespace Cloud.Messages.Annotation
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class AnnotationResult
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnassignedField.Global
        public string id;
        public string status;
        public string filePath;
        public ErrorCategory? errorCategory;
        // ReSharper restore UnassignedField.Global
        // ReSharper restore InconsistentNaming
    }
}