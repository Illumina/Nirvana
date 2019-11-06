using System.Collections.Generic;
using System.Linq;
using Cloud;
using Genome;

namespace SingleAnnotationLambda
{
    public class SupplementaryAnnotationUtilities
    {
        private static readonly string[] SupportedValues = { "latest", "release" };

        public static bool IsValueSupported(string supplementaryAnnotations)
        {
            string sa = supplementaryAnnotations?.ToLower();
            return SupportedValues.Any(supportedValue => sa == supportedValue);
        }

        public static string GetSupportedValues() => string.Join(", ", SupportedValues);

        public static List<string> GetManifestUrls(string versionTag, GenomeAssembly genomeAssembly)
        {
            var saUrls = new List<string>();
            if (string.IsNullOrEmpty(versionTag)) return saUrls;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (versionTag)
            {
                case "latest":
                    saUrls.Add($"{LambdaUrlHelper.GetBaseUrl()}latest_SA_{genomeAssembly}.txt");
                    break;
                case "release":
                    saUrls.Add($"{LambdaUrlHelper.GetBaseUrl()}Zeus_SA_{genomeAssembly}.txt");
                    break;
            }

            return saUrls;
        }
    }
}
