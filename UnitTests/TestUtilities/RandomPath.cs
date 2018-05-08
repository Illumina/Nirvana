using System.IO;

namespace UnitTests.TestUtilities
{
	public static class RandomPath
	{
		public static string GetRandomPath() => Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
	}
}