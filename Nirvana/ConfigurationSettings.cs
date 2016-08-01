using System.Collections.Generic;

namespace Nirvana
{
    public static class ConfigurationSettings
    {
        #region members

        // filenames
        public static string CacheDirectory;
        public static string SupplementaryAnnotationDirectory;
		public static readonly List<string> CustomAnnotationDirectories = new List<string>();
		public static readonly List<string> CustomIntervalDirectories = new List<string>();
		public static string VcfPath;
        public static string CompressedReferencePath;
        public static string OutputFileName;

	    public static bool Vcf;
	    public static bool Gvcf;
        public static bool EnableReferenceNoCalls;
        public static bool LimitReferenceNoCallsToTranscripts;
	    public static bool ForceMitochondrialAnnotation;
        public static bool EnableTelemetry;

        #endregion
    }
}
