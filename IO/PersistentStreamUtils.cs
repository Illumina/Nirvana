using System.Collections.Generic;
using System.IO;
using Amazon.S3;
using Amazon.S3.Model;

namespace IO
{
    public static class PersistentStreamUtils
    {
        private const int MaxRetryCount = 5;

        public static Stream GetReadStream(string location, long position = 0)
        {
            if (string.IsNullOrEmpty(location)) return null;
            if (!ConnectUtilities.IsHttpLocation(location))
                return File.Exists(location) ? FileUtilities.GetReadStream(location) : null;

            var connector = ConnectUtilities.GetHttpConnectFunc(location);
            var stream = ConnectUtilities.ConnectWithRetries(connector, position, MaxRetryCount);
            return new PersistentStream(stream, connector, position);
        }

        public static Stream GetS3ReadStream(IS3Client s3Client, string bucketName, string fileName, long position)
        {
            var connector = ConnectUtilities.GetS3ConnectFunc(bucketName, fileName, s3Client);
            var stream = ConnectUtilities.ConnectWithRetries(connector, position, MaxRetryCount);
            return new PersistentStream(stream, connector, position);
        }

        public static long GetLength(AmazonS3Client s3Client, string bucketName, string fileName)
        {
            var metadataRequest = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = fileName.TrimStart('/')
            };

            var getMetadataResponse = s3Client.GetObjectMetadataAsync(metadataRequest).Result;

            return getMetadataResponse.ContentLength;
        }

        public static IEnumerable<Stream> GetStreams(IEnumerable<string> locations)
        {
            if (locations == null) yield break;

            foreach (string location in locations)
            {
                yield return GetReadStream(location);
            }
        }
    }
}