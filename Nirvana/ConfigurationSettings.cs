using System.Collections.Generic;

namespace Nirvana
{
	public static class ConfigurationSettings
	{
		public static string InputCachePrefix;
		public static readonly List<string> SupplementaryAnnotationDirectories = new List<string>();
		public static string VcfPath;
		public static string RefSequencePath;
		public static string OutputFileName;

		public static bool Vcf;
		public static bool Gvcf;
		public static bool ForceMitochondrialAnnotation;
		public static bool ReportAllSvOverlappingTranscripts;
		public static bool EnableLoftee;
		public static string ConservationScoreDir { get; set; }
	}
}