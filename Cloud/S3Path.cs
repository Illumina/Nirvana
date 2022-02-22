// ReSharper disable InconsistentNaming

using System;
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using ErrorHandling.Exceptions;

namespace Cloud
{
    public sealed class S3Path
    {
        public string bucketName;
        public string path;

        public void CheckBucketExist(AmazonS3Client s3Client)
        {
            if (!AmazonS3Util.DoesS3BucketExistAsync(s3Client, bucketName).Result)
                throw new UserErrorException($"S3 bucket {bucketName} doesn't exist.");
        }

        private void CheckFileExist(AmazonS3Client s3Client)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = path.TrimStart('/'),
                Expires = DateTime.Now.AddMinutes(1)
            };

            if (!CheckUrlExist(s3Client.GetPreSignedURL(request)))
                throw new UserErrorException($"Path {path} doesn't exist.");
        }


        private static bool CheckUrlExist(string url)
        {
            try
            {
                var webRequest = WebRequest.Create(url);
                webRequest.GetResponse();
            }
            catch // An exception will be thrown if couldn't get response from address
            {
                return false;
            }

            return true;

        }
    }
}