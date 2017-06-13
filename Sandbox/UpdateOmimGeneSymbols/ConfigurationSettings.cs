using System.Collections.Generic;

namespace UpdateOmimGeneSymbols
{
    public static class ConfigurationSettings
    {
        public static string InputGeneMap2Path;
        public static string OutputGeneMap2Path;
        public static string HgncPath;
        public static readonly List<string> GeneInfoPaths = new List<string>();
    }
}
