using System;
using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using ErrorHandling.Exceptions;

namespace Cloud
{
    public static class S3Utilities
    {
        public static void Upload(AmazonS3Client s3Client, string bucketName, string path, string localFilePath)
        {
            TransferUtility transferUtility = new TransferUtility(s3Client);

            TransferUtilityUploadRequest transferUtilityRequest = new TransferUtilityUploadRequest
            {
                BucketName = bucketName,
                FilePath = localFilePath,
                Key = path.TrimStart('/')
            };

            transferUtility.Upload(transferUtilityRequest);
        }

        public static bool FileExist(AmazonS3Client client, S3Path s3Path)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = s3Path.bucketName,
                Key = s3Path.path.TrimStart('/'),
                Expires = DateTime.Now.AddMinutes(1)
            };

            return CheckUrlExist(client.GetPreSignedURL(request));
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