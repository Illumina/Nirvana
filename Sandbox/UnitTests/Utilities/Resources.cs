using System;
using System.IO;

namespace UnitTests.Utilities
{
    public static class Resources
    {
        public static readonly string Top;
        public static string TopPath(string path) => Path.Combine(Top, path);

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
