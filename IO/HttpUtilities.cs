using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Xml.Linq;
using System.Linq;
using System.Threading;
using ErrorHandling.Exceptions;

namespace IO
{
    public static class HttpUtilities
    {
        private static readonly string[] AuthenticationErrorCodes   = { "InvalidAccessKeyId", "SignatureDoesNotMatch" };
        private static readonly string[] ResourceNotExistErrorCodes = { "NoSuchKey", "NoSuchBucket" };

        public static long GetLength(string url)
        {
            var request        = (HttpWebRequest)WebRequest.Create(url);
            var response       = request.TryGetResponse(url);
            long contentLength = response.ContentLength;
            response.Close();
            return contentLength;
        }

        // Only throw exceptions when all the three tries failed.
        public static HttpWebResponse TryGetResponse(this HttpWebRequest request, string url)
        {
            var exceptions = new List<Exception>();

            for (var retryCounter = 0; retryCounter < 3; retryCounter++)
            {
                try
                {
                    if (retryCounter > 0) Thread.Sleep(2_000);
                    return (HttpWebResponse)request.GetResponse();
                }
                catch (Exception e)
                {
                    exceptions.Add(ProcessHttpRequestWebProtocolErrorException(e, url));
                }
            }

            throw new AggregateException(exceptions);
        }

        public static void ValidateUrl(string url, bool isUserProvided = true)
        {
            try
            {
                var response = WebRequest.CreateHttp(url).TryGetResponse(url);
                response.Close();
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
                return new UserErrorException(
                    $"Authentication error while reading from {url}. {errorMessage}. Exception: {exception.Message}");

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

        public static bool IsUrl(string path) => path.StartsWith("http", true, CultureInfo.InvariantCulture);
    }
}
