using System;
using System.Net;
using ErrorHandling;
using ErrorHandling.Exceptions;

namespace IO
{
    public static class HttpUtilities
    {
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
                throw ExceptionUtilities.ProcessHttpRequestForbiddenException(e, url);
            }
        }

        public static void ValidateUrl(string url) => WebRequest.CreateHttp(url).TryGetResponse(url);
    }
}