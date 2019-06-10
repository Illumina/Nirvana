using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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

        internal static bool IsWebProtocolErrorException(Exception exception)
        {
            if (!(exception is WebException)) return false;
            var webException = (WebException)exception;

            return webException.Status == WebExceptionStatus.ProtocolError;
        }

        public static Exception ProcessHttpRequestForbiddenException(Exception exception, string url)
        {
            if (!IsWebProtocolErrorException(exception)) return exception;

            var webException = (WebException)exception;
            string errorMessage = GetWebExceptionMessage(webException);

            string newErrorMessage = errorMessage == "Request has expired"
                ? $"The provided URL {url} is expired. {exception.Message}"
                : $"{errorMessage} when reading from {url}. {exception.Message}";

            return new UserErrorException(newErrorMessage);
        }


        private static string GetWebExceptionMessage(WebException exception)
        {
            using (var stream = exception.Response.GetResponseStream())
            {
                return stream == null ? null : XElement.Load(stream).Element("Message")?.Value;
            }
        }
    }
}