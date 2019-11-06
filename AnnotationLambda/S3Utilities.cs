namespace AnnotationLambda
{
    public static class S3Utilities
    {
        public static string GetKey(string outputDir, string filename)
        {
            outputDir = outputDir?.Trim('/');
            if (string.IsNullOrEmpty(outputDir)) return filename;
            return outputDir + '/' + filename;
        }
    }
}
