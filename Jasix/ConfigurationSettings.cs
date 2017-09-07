using System.Collections.Generic;

namespace Jasix
{
    public static class ConfigurationSettings
    {
        public static string InputJson;
        public static string OutputFile;
        public static readonly List<string> Queries=new List<string>();
        public static bool PrintHeader;
        public static bool PrintHeaderOnly;
        public static bool ListChromosomeName;
        public static bool CreateIndex;
    }
}