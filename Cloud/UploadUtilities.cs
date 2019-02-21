using System;
using System.IO;
using System.Security.Cryptography;
using Amazon.S3.Model;
using IO;

namespace Cloud
{
    public static class UploadUtilities
    {
        public static void Upload(IS3Client s3Client, string bucketName, string s3Path, string localFilePath)
        {
            var putRequest = GetPutObjectRequest(bucketName, s3Path, localFilePath);

            s3Client.PutObjectAsync(putRequest).Wait();
        }

        internal static PutObjectRequest GetPutObjectRequest(string bucketName, string s3Path, string localFilePath)
        {
            string inputFileMd5 = GetMd5Base64(localFilePath);

            return new PutObjectRequest
            {
                BucketName = bucketName,
                Key = s3Path.TrimStart('/'),
                FilePath = localFilePath,
                MD5Digest = inputFileMd5
            };
        }

        internal static string GetMd5Base64(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                return Convert.ToBase64String(md5.ComputeHash(stream));
            }
        }
    }
}