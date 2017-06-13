using System.Collections.Generic;

namespace Nirvana
{
    public static class ConfigurationSettings
    {
        // filenames
        public static string InputCachePrefix;
        public static readonly List<string> SupplementaryAnnotationDirectories = new List<string>();
		public static string VcfPath;
        public static string CompressedReferencePath;
        public static string OutputFileName;

	    public static bool Vcf;
	    public static bool Gvcf;
        public static bool EnableReferenceNoCalls;
        public static bool LimitReferenceNoCallsToTranscripts;
	    public static bool ForceMitochondrialAnnotation;
	    public static bool ReportAllSvOverlappingTranscripts;
	    public static bool EnableLoftee;
    }
}
