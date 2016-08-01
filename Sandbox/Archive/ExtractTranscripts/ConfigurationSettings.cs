using System.Collections.Generic;

namespace ExtractTranscripts
{
    public static class ConfigurationSettings
    {
        #region members

        public const int DefaultIntValue = -1;

        // filenames
        public static string InputCachePath;
        public static string OutputCachePath;

        // parameters
        public static string ReferenceName;
        public static string ReferenceAllele;
        public static int ReferencePosition         = DefaultIntValue;
        public static readonly List<string> AlternateAlleles = new List<string>();

        public static string TargetTranscriptId;

        public static string VcfLine;

        #endregion
    }
}
