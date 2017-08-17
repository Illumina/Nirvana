using System.IO;
using VariantAnnotation.Utilities;

namespace UnitTests.TestUtilities
{
	public static class ResourceUtilities
	{
		public static Stream GetReadStream(string path, bool checkMissingFile = true)
		{
			var missingFile = !File.Exists(path);
			if (!checkMissingFile && missingFile) return null;

			if (missingFile)
			{
				throw new FileNotFoundException($"ERROR: The unit test resource file ({path}) was not found.");
			}

			return FileUtilities.GetReadStream(path);
		}
	}
}