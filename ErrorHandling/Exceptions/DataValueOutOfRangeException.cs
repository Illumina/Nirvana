using System;

namespace ErrorHandling.Exceptions
{
	public class DataValueOutOfRangeException : Exception
	{
		// constructor
		public DataValueOutOfRangeException(string message) : base(message) { }
	}
}
