using System;
using System.Net;
using System.Xml.Linq;
using System.Linq;
using ErrorHandling.Exceptions;

namespace IO
{
    public static class HttpUtilities
    {
        private static readonly string[] AuthenticationErrorCodes = { "InvalidAccessKeyId", "SignatureDoesNotMatch" };
        private static readonly string[] ResourceNotExistErrorCodes = { "NoSuchKey", "NoSuchBucket" };

        public static long GetLength(string url)
        {
            var request = (HttpWebRequest) WebRequest.Create(url);

            return request.TryGetResponse(url).ContentLength;
        }

        public static HttpWebResponse TryGetResponse(this HttpWebRequest request, string url)
        {
            try
            {
                return (HttpWebResponse) request.GetResponse();
            }
            catch (Exception e)
            {
                throw ProcessHttpRequestWebProtocolErrorException(e, url);
            }
        }

        // When we validate a URL, it is a user error by default if any exception has been thrown. 
        public static void ValidateUrl(string url, bool isUserProvided = true)
        {
            try
            {
                WebRequest.CreateHttp(url).TryGetResponse(url);
            }
            catch (Exception exception)
            {
                if (isUserProvided) throw new UserErrorException(exception.Message);
                throw;
            }
        }

        public static bool IsWebProtocolErrorException(Exception exception)
        {
            if (!(exception is WebException)) return false;
            var webException = (WebException)exception;

            return webException.Status == WebExceptionStatus.ProtocolError;
        }

        public static Exception ProcessHttpRequestWebProtocolErrorException(Exception exception, string url)
        {
            if (!IsWebProtocolErrorException(exception)) return exception;

            var webException = (WebException)exception;
            (string errorCode, string errorMessage) = GetWebExceptionMessage(webException);

            // Expired URL is always a user error
            if (errorMessage == "Request has expired")
                return new UserErrorException($"The provided URL {url} is expired. Exception: {exception.Message}");

            // Authentication error is always considered as a user error
            if (AuthenticationErrorCodes.Contains(errorCode))
                return new UserErrorException($"Authentication error while reading from {url}. {errorMessage}. Exception: {exception.Message}");

            // Resource not exist error is always considered as a user error
            if (ResourceNotExistErrorCodes.Contains(errorCode))
                return new UserErrorException($"Invalid URL {url}. {errorMessage}. Exception: {exception.Message}");

            // Sometimes it is difficult to figure out whether the error is caused by the user or not.
            // For example, the AccessDenied error code could be triggered by either incorrect credentials provided by the user, or network congestion while reading from S3.
            // Therefore, such errors are treated as general exceptions.
            // And we don't pass through the general error to end user to avoid possible confusion.
            Logger.LogLine($"The following error occurred while reading from {url}: {errorMessage}. Exception: {exception.Message}");
            return new WebException($"An error occurred while reading from {url}");
        }

        private static (string Code, string Message) GetWebExceptionMessage(WebException exception)
        {
            using (var stream = exception.Response.GetResponseStream())
            {
                if (stream == null) return (null, null);

                var xElement = XElement.Load(stream);
                return (xElement.Element("Code")?.Value, xElement.Element("Message")?.Value);
            }
        }
    }
}