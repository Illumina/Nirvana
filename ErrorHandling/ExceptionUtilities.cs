using System;
using System.Collections.Generic;
using ErrorHandling.Exceptions;

namespace ErrorHandling
{
    public static class ExceptionUtilities
    {
        public const string UserError = "UserError";
        
        public static Exception MakeUserError(this Exception e)
        {
            e.Data[UserError] = true;
            return e;
        }
        
        // define which exceptions should not include a full stack trace
        public static readonly HashSet<Type> UserFriendlyExceptions = new HashSet<Type>
        {
            typeof(UserErrorException),
            typeof(FileNotSortedException),
            typeof(UnauthorizedAccessException),
            typeof(InvalidFileFormatException),
            typeof(ProcessLockedFileException),
            typeof(OutOfMemoryException),
            typeof(MissingCompressionLibraryException)
        };

        public static bool HasException<T>(Exception e)
        {
            if (e == null) return false;
            return e is T || HasException<T>(e.InnerException);
        }

        public static bool HasErrorMessage(this Exception e, string errorMessage)
        {
            if (e == null) return false;
            return e.Message  == errorMessage|| e.InnerException.HasErrorMessage(errorMessage);
        }

        public static Exception GetInnermostException(Exception e)
        {
            while (e.InnerException != null) e = e.InnerException;
            return e;
        }

        public static ErrorCategory ExceptionToErrorCategory(Exception exception) => UserFriendlyExceptions.Contains(exception.GetType()) ? ErrorCategory.UserError : ErrorCategory.NirvanaError;
    }
}