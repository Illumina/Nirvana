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
        private const           int      MaxRetryCount              = 10;
        
        public static long GetLength(string url)
        {
            var response       = TryGetResponse(url);
            long contentLength = response.ContentLength;
            response.Close();
            return contentLength;
        }

        // Only throw exceptions when all the three tries failed.
        private static HttpWebResponse TryGetResponse(string url)
        {
            var exceptions = new List<Exception>();

            for (var retryCounter = 0; retryCounter < MaxRetryCount; retryCounter++)
            {
                try
                {
                    if (retryCounter > 0)
                    {
                        Console.WriteLine($"Attempt {retryCounter+1} to get response from {url}");
                        Thread.Sleep(2_000);
                    }

                    var request = (HttpWebRequest) WebRequest.Create(url);
                    if (retryCounter > 0)
                    {
                        Console.WriteLine($"Succeeded at attempt#: {retryCounter+1}");
                    }

                    return (HttpWebResponse) request.GetResponse();
                }
                catch (Exception e)
                {
                    Logger.WriteLine($"TryGetResponse exception found when connecting to {url}");
                    Logger.Log(e);
                    exceptions.Add(ProcessHttpRequestWebProtocolErrorException(e, url));
                }
            }

            throw new AggregateException(exceptions);
        }

        public static void ValidateUrl(string url, bool isUserProvided = true)
        {
            try
            {
                var response = TryGetResponse(url);
                response.Close();
            }
            catch (Exception)
            {
                if (isUserProvided) throw new UserErrorException($"Unable to validate the URL for {UrlUtilities.GetFileName(url)}");
                throw new DeploymentErrorException($"Deployment issue detected. Unable to validate the URL for {url}.");
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

            string urlPath = UrlUtilities.GetPath(url);

            var webException = (WebException)exception;
            (string errorCode, string errorMessage) = GetWebExceptionMessage(webException);

            // Expired URL is always a user error
            if (errorMessage == "Request has expired") return new UserErrorException($"The provided URL for {urlPath} has expired.");

            // Authentication error is always considered as a user error
            if (AuthenticationErrorCodes.Contains(errorCode)) return new UserErrorException($"Authentication error while reading from URL for {urlPath}.");

            // Resource not exist error is always considered as a user error
            if (ResourceNotExistErrorCodes.Contains(errorCode)) return new UserErrorException($"An invalid URL for {urlPath} was specified.");

            // Sometimes it is difficult to figure out whether the error is caused by the user or not.
            // For example, the AccessDenied error code could be triggered by either incorrect credentials provided by the user, or network congestion while reading from S3.
            // Therefore, such errors are treated as general exceptions.
            // And we don't pass through the general error to end user to avoid possible confusion.
            Logger.WriteLine($"The following error occurred while reading from {url}: {errorMessage}. Exception: {exception.Message}");
            return new WebException($"An error occurred while reading from the URL for {urlPath} ({exception.GetType()})");
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
