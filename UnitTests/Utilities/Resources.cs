using System;
using System.IO;

namespace UnitTests.Utilities
{
    public static class Resources
    {
        // ReSharper disable once MemberCanBePrivate.Global
        public static readonly string Top;
        public static string TopPath(string path)             => Path.Combine(Top, path);
        public static string CacheGRCh37(string path)         => Path.Combine(Top, "Caches", "GRCh37", path);
        public static string CacheGRCh38(string path)         => Path.Combine(Top, "Caches", "GRCh38", path);
        public static string CustomAnnotations(string path)   => Path.Combine(Top, "MiniSuppAnnot", "CustomAnnotations", path);
        public static string CustomIntervals(string path)     => Path.Combine(Top, "MiniSuppAnnot", "CustomIntervals", path);
        public static string InputFiles(string path)          => Path.Combine(Top, "InputFiles", path);
        public static string MiniSuppAnnot(string path)       => Path.Combine(Top, "MiniSuppAnnot", path);
        public static string MiniSuppAnnotGRCh38(string path) => Path.Combine(Top, "MiniSuppAnnot", "hg38", path);

        static Resources()
        {
            var solutionDir = GetParentDirectory(AppContext.BaseDirectory, 3);
            Top = Path.Combine(solutionDir, "UnitTests", "Resources");
        }

        private static string GetParentDirectory(string directory, int numLevels)
        {
            for (int i = 0; i < numLevels; i++) directory = Path.GetDirectoryName(directory);
            return directory;
        }
    }
}
