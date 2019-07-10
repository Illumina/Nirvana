using System;
using System.IO;

namespace UnitTests.TestUtilities
{
	public static class Resources
	{
		// ReSharper disable once MemberCanBePrivate.Global
		public static readonly string Top;
		public static string TopPath(string path) => Path.Combine(Top, path);
		public static string EndToEnd37(string path) => Path.Combine(Top, "EndToEnd", "GRCh37", path);
		public static string InputFiles(string path) => Path.Combine(Top, "InputFiles", path);
	    public static string ClinvarXmlFiles(string path) => Path.Combine(Top, "ClinVarXmlFiles", path);
		public static string SaGRCh37(string path) => Path.Combine(Top, "SA", "GRCh37", path);
        public static string MockSaFiles => Path.Combine(Top, "SA", "MockSaFiles");

		static Resources()
		{
            var solutionDir = GetParentDirectory(AppContext.BaseDirectory);
			Top = Path.Combine(solutionDir, "UnitTests", "Resources");
		}

		private static string GetParentDirectory(string directory)
		{
		    while (true)
		    {
		        directory = Path.GetDirectoryName(directory);
		        if (directory == null) return string.Empty;

		        var unitTestDir = Path.Combine(directory, "UnitTests");
		        if (Directory.Exists(unitTestDir)) break;
		    }

			return directory;
		}
	}
}