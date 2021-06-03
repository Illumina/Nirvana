using System.IO;
using Cloud.Messages.Annotation;
using ErrorHandling;

namespace NirvanaLambda
{
    public sealed class AnnotationResultSummary
    {
        public string         ErrorMessage;
        public string         FileName;
        public int            VariantCount;
        public ErrorCategory? ErrorCategory;

        public static AnnotationResultSummary Create(AnnotationResult annotationResult, ErrorCategory? errorCategory, string errorMessage)
        {
            string fileName = Path.GetFileName(annotationResult?.filePath);

            return new AnnotationResultSummary
            {
                ErrorCategory = errorCategory,
                ErrorMessage  = errorMessage,
                FileName      = fileName,
                VariantCount  =  annotationResult?.variantCount ?? 0
            };
        }
    }
}