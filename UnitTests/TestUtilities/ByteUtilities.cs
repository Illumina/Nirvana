using System.Security.Cryptography;

namespace UnitTests.TestUtilities
{
	public static class ByteUtilities
	{
		public static byte[] GetRandomBytes(int numBytes)
		{
			var buffer = new byte[numBytes];
			using (var csp = RandomNumberGenerator.Create()) csp.GetBytes(buffer);
			return buffer;
		}

	}
}