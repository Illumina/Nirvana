using System.Linq;

namespace SingleAnnotationLambda
{
    public static class SupplementaryAnnotationUtilities
    {
        private static readonly string[] SupportedValues = { "latest", "release" };

        public static bool IsValueSupported(string supplementaryAnnotations)
        {
            string sa = supplementaryAnnotations?.ToLower();
            return SupportedValues.Any(supportedValue => sa == supportedValue);
        }

        public static string GetSupportedValues() => string.Join(", ", SupportedValues);
    }
}
