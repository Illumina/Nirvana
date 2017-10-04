using System.Collections.Generic;

namespace SAUtils.CreateOmimTsv
{
    public static class ConfigurationSettings
    {
        public static string InputGeneMap2Path;
        public static string OutputDirectory;
        public static string HgncPath;
        public static string Mim2GenePath;
        public static readonly List<string> GeneInfoPaths = new List<string>();
    }
}