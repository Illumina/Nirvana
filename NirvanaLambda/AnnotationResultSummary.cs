using System.IO;
using Cloud.Messages.Annotation;
using ErrorHandling;

namespace NirvanaLambda
{
    public sealed class AnnotationResultSummary
    {
        public string ErrorMessage;
        public string FileName;
        public ErrorCategory? ErrorCategory;

        public static AnnotationResultSummary Create(AnnotationResult annotationResult, ErrorCategory? errorCategory, string errorMessage)
        {
            string fileName = Path.GetFileName(annotationResult?.filePath);

            return new AnnotationResultSummary
            {
                ErrorCategory = errorCategory,
                ErrorMessage  = errorMessage,
                FileName      = fileName
            };
        }
    }
}