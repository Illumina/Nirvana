using System;

namespace ErrorHandling.Exceptions
{
	public sealed class CompressionException : Exception
	{
		// constructor
		public CompressionException(string message) : base(message) { }
	}
}
