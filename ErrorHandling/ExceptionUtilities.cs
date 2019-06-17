using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Xml.Linq;
using ErrorHandling.Exceptions;


namespace ErrorHandling
{
    public static class ExceptionUtilities
    {
        // define which exceptions should not include a full stack trace
        public static readonly ImmutableHashSet<Type> UserFriendlyExceptions = new HashSet<Type>
        {
            typeof(UserErrorException),
            typeof(FileNotSortedException),
            typeof(UnauthorizedAccessException),
            typeof(InvalidFileFormatException),
            typeof(ProcessLockedFileException),
            typeof(OutOfMemoryException),
            typeof(MissingCompressionLibraryException)
        }.ToImmutableHashSet();

        public static bool HasException<T>(Exception e)
        {
            if (e == null) return false;
            return e is T || HasException<T>(e.InnerException);
        }

        public static Exception GetInnermostException(Exception e)
        {
            while (e.InnerException != null) e = e.InnerException;
            return e;
        }

        public static ErrorCategory ExceptionToErrorCategory(Exception exception) => UserFriendlyExceptions.Contains(exception.GetType()) ? ErrorCategory.UserError : ErrorCategory.NirvanaError;
    }
}