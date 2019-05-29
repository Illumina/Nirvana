// ReSharper disable InconsistentNaming

using System;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using ErrorHandling.Exceptions;
using IO;

namespace Cloud
{
    public sealed class S3Path
    {
        public string bucketName;
        public string region;
        public string path;
        public string accessKey;
        public string secretKey;
        public string sessionToken;

        public void Validate(bool isDirectory)
        {
            ValidatePathFormat(path, isDirectory);
            path = FormatPath(path);

            CheckS3Region(region);

            var s3Client = GetS3Client(TimeSpan.FromMinutes(5));
            ValidateCredentials(s3Client, isDirectory);
        }

        private void CheckS3Region(string s)
        {
            if (RegionEndpoint.GetBySystemName(region).DisplayName == "Unknown")
                throw new UserErrorException($"Unknown S3 Region {region}");
        }

        private void ValidateCredentials(IS3Client s3Client, bool isDirectory)
        {
            try
            {
                if (isDirectory)
                {
                    var putRequest = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = path
                    };
                    s3Client.PutObjectAsync(putRequest).Wait();
                }
                else
                {
                    var getRequest = new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = path,
                        ByteRange = new ByteRange(0,1)
                    };
                    s3Client.GetObjectAsync(getRequest).Wait();
                }
            }
            catch (Exception exception)
            {
                var processedException = ExceptionUtilities.ProcessAmazonS3Exception(exception, this);
                if (processedException is UserErrorException) throw processedException;
            }
        }

        internal static void ValidatePathFormat(string path, bool isDirectory)
        {
            if (isDirectory == path.EndsWith('/')) return;
            string errorMessage = isDirectory
                ? $"Expect a directory, but S3 path {path} doesn't end up with a '/'"
                : $"Expect a file, but S3 path {path} ends up with a '/'";
            throw new UserErrorException(errorMessage);
        }

        public static string FormatPath(string path) => path.TrimStart(trimChar: '/');

        public IS3Client GetS3Client(TimeSpan timeOut) => new AmazonS3ClientWrapper(new AmazonS3Client(accessKey, secretKey, sessionToken, new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(region), Timeout = timeOut }));
    }
}