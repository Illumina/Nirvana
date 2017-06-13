using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.DataStructures;
using ErrorHandling.Exceptions;

namespace ErrorHandling.Utilities
{
    public static class ExitCodeUtilities
    {
        #region members

        private static readonly Dictionary<Type, int> ExceptionsToExitCodes;
        private static readonly HashSet<Type> UserFriendlyExceptions;

        #endregion
        
        // constructor
        static ExitCodeUtilities()
        {
            // add the exception to exit code mappings
            ExceptionsToExitCodes = new Dictionary<Type, int>
            {
                { typeof(GeneralException),                   (int)ExitCodes.InvalidFunction },
                { typeof(ArgumentNullException),              (int)ExitCodes.BadArguments },
                { typeof(ArgumentOutOfRangeException),        (int)ExitCodes.BadArguments },
                { typeof(Exception),                          (int)ExitCodes.InvalidFunction },
                { typeof(FileNotFoundException),              (int)ExitCodes.FileNotFound },
                { typeof(FileNotSortedException),             (int)ExitCodes.FileNotSorted },
                { typeof(FormatException),                    (int)ExitCodes.BadFormat },
                { typeof(InvalidDataException),               (int)ExitCodes.InvalidData },
                { typeof(InvalidFileFormatException),         (int)ExitCodes.InvalidFileFormat },
                { typeof(InvalidOperationException),          (int)ExitCodes.InvalidFunction },
                { typeof(NotImplementedException),            (int)ExitCodes.CallNotImplemented },
                { typeof(UserErrorException),                 (int)ExitCodes.UserError },
                { typeof(UnauthorizedAccessException),        (int)ExitCodes.AccessDenied },
                { typeof(ProcessLockedFileException),         (int)ExitCodes.SharingViolation },
                { typeof(OutOfMemoryException),               (int)ExitCodes.OutofMemory },
                { typeof(MissingCompressionLibraryException), (int)ExitCodes.MissingCompressionLibrary }
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

        /// <summary>
        /// Displays the details behind the exception
        /// </summary>
        public static int ShowException(Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("\nERROR: ");
            Console.ResetColor();

            Console.WriteLine("{0}", e.Message);

            var exceptionType = e.GetType();

            if (!UserFriendlyExceptions.Contains(exceptionType))
            {
                // print the stack trace
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nStack trace:");
                Console.ResetColor();
                Console.WriteLine(e.StackTrace);

                // extract out the vcf line
                if (e.Data.Contains("VcfLine"))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nVCF line:");
                    Console.ResetColor();
                    Console.WriteLine(e.Data["VcfLine"]);
                }
            }

            // return a non-zero exit code
            int exitCode;
            if (!ExceptionsToExitCodes.TryGetValue(exceptionType, out exitCode)) exitCode = 1;
            return exitCode;
        }
    }
}
