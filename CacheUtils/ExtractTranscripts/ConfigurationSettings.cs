using System.Collections.Generic;

namespace CacheUtils.ExtractTranscripts
{
    public static class ConfigurationSettings
    {
        // filenames
        public static string InputPrefix;
        public static string InputReferencePath;
        public static string OutputDirectory;

        // parameters
        public static string ReferenceName;
        public static string ReferenceAllele;
        public static string AlternateAllele;

        public static int ReferencePosition    = -1;
        public static int ReferenceEndPosition = -1;

        public static readonly HashSet<string> TranscriptIds = new HashSet<string>();
    }
}
