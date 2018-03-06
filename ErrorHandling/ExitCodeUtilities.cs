using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;

namespace ErrorHandling
{
	public static class ExitCodeUtilities
	{
		private static readonly Dictionary<Type, ExitCodes> ExceptionsToExitCodes;
		private static readonly HashSet<Type> UserFriendlyExceptions;
	    public const string VcfLine = "VcfLine";

		// constructor
		static ExitCodeUtilities()
		{
			// add the exception to exit code mappings
			ExceptionsToExitCodes = new Dictionary<Type, ExitCodes>
			{
				{ typeof(ArgumentNullException),              ExitCodes.BadArguments },
				{ typeof(ArgumentOutOfRangeException),        ExitCodes.BadArguments },
				{ typeof(Exception),                          ExitCodes.InvalidFunction },
				{ typeof(FileNotFoundException),              ExitCodes.FileNotFound },
				{ typeof(FileNotSortedException),             ExitCodes.FileNotSorted },
				{ typeof(FormatException),                    ExitCodes.BadFormat },
				{ typeof(InvalidDataException),               ExitCodes.InvalidData },
				{ typeof(InvalidFileFormatException),         ExitCodes.InvalidFileFormat },
				{ typeof(InvalidOperationException),          ExitCodes.InvalidFunction },
				{ typeof(NotImplementedException),            ExitCodes.CallNotImplemented },
				{ typeof(UserErrorException),                 ExitCodes.UserError },
				{ typeof(UnauthorizedAccessException),        ExitCodes.AccessDenied },
				{ typeof(ProcessLockedFileException),         ExitCodes.SharingViolation },
				{ typeof(OutOfMemoryException),               ExitCodes.OutofMemory },
				{ typeof(MissingCompressionLibraryException), ExitCodes.MissingCompressionLibrary },
			    { typeof(CompressionException),               ExitCodes.Compression }
            };

			// define which exceptions should not include a full stack trace
			UserFriendlyExceptions = new HashSet<Type>
			{
				typeof(UserErrorException),
				typeof(FileNotSortedException),
				typeof(UnauthorizedAccessException),
				typeof(InvalidFileFormatException),
				typeof(ProcessLockedFileException),
				typeof(OutOfMemoryException),
				typeof(MissingCompressionLibraryException)
			};
		}

	    internal static ExitCodes GetExitCode(Type exceptionType)
	    {
            if (!ExceptionsToExitCodes.TryGetValue(exceptionType, out ExitCodes exitCode)) exitCode = ExitCodes.InvalidFunction;
            return exitCode;
	    }

		/// <summary>
		/// Displays the details behind the exception
		/// </summary>
		public static ExitCodes ShowException(Exception e)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.Write("\nERROR: ");
			Console.ResetColor();

            while (e.InnerException != null) e = e.InnerException;

            Console.WriteLine("{0}", e.Message);

			var exceptionType = e.GetType();

		    // ReSharper disable once InvertIf
			if (!UserFriendlyExceptions.Contains(exceptionType))
			{
				// print the stack trace
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("\nStack trace:");
				Console.ResetColor();
				Console.WriteLine(e.StackTrace);

				// extract out the vcf line
			    // ReSharper disable once InvertIf
				if (e.Data.Contains(VcfLine))
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine("\nVCF line:");
					Console.ResetColor();
					Console.WriteLine(e.Data[VcfLine]);
				}
			}

		    return GetExitCode(exceptionType);
		}
	}
}