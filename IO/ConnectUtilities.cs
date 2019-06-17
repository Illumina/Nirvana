using System;
using System.Globalization;
using System.IO;
using System.Net;
using Amazon.S3.Model;
using ErrorHandling.Exceptions;

namespace IO
{
    public static class ConnectUtilities
    {
        public static Func<long, Stream> GetS3ConnectFunc(string bucketName, string path, IS3Client client)
        {
            var s3Client = client;

            return position =>
            {
                var getRequest = new GetObjectRequest
                {
                    BucketName = bucketName,
                    Key = path.TrimStart('/'),
                    ByteRange = new ByteRange(position, long.MaxValue)
                };

                return s3Client.GetObjectAsync(getRequest).Result.ResponseStream;
            };
        }

        public static Func<long, Stream> GetHttpConnectFunc(string url)
        {
            return position =>
            {
                var request = WebRequest.CreateHttp(url);
                if (position < 0) position = 0;

                request.AddRange(position);

                return request.TryGetResponse(url).GetResponseStream();
            };
        }

        public static Func<long, Stream> GetFileConnectFunc(string filePath)
        {
            var path = filePath;
            return position =>
            {
                var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read) {Position = position};
                return stream;
            };
        }

        public static Stream ConnectWithRetries(Func<long, Stream> connect, long position, int retryCount)
        {
            while (retryCount > 0)
            {
                retryCount--;
                try
                {
                    return connect(position);
                }
                catch (Exception e)
                {
                    Logger.LogLine($"EXCEPTION: {e.Message}");

                    if (e is UserErrorException || retryCount == 0) throw;
                }
            }

            return null;
        }

        public static bool IsHttpLocation(string path) => path.StartsWith("http", true, CultureInfo.InvariantCulture);
    }
}