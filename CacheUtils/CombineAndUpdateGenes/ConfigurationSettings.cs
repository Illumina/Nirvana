using System.Collections.Generic;

namespace CacheUtils.CombineAndUpdateGenes
{
    public static class ConfigurationSettings
    {
        public static string InputPath;
        public static string InputPath2;
        public static string OutputPath;
        public static string RefSeqGff3Path;
        public static string HgncPath;
        public static readonly List<string> GeneInfoPaths = new List<string>();
    }
}
