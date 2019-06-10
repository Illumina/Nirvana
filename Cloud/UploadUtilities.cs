using System;
using System.Security.Cryptography;
using System.Threading;
using Amazon.S3.Model;
using ErrorHandling.Exceptions;
using IO;

namespace Cloud
{
    public static class UploadUtilities
    {
        public static void DecryptUpload(this IS3Client s3Client, string bucketName, string key, string filePath,
            AesCryptoServiceProvider aes, FileMetadata metadata, int retryDelay = 1000)
        {
            while (true)
            {
                if (s3Client.TryDecryptUpload(bucketName, key, filePath, aes, metadata)) return;

                Thread.Sleep(retryDelay);
            }
        }

        internal static bool TryDecryptUpload(this IS3Client s3Client, string bucketName, string key, string filePath, AesCryptoServiceProvider aes, FileMetadata metadata)
        {
            try
            {
                using (var fileStream = FileUtilities.GetReadStream(filePath))
                using (var cryptoStream = new CryptoStream(fileStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                using (var lengthStream = new LengthStream(cryptoStream, metadata.Length))
                {
                    string md5String = Convert.ToBase64String(metadata.MD5);

                    var request = new PutObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key,
                        InputStream = lengthStream,
                        MD5Digest = md5String
                    };

                    s3Client.PutObjectAsync(request).Wait();
                }

                return true;
            }

            catch (Exception exception)
            {
                var processedException = ExceptionUtilities.ProcessAmazonS3Exception(exception, null);

                if (processedException is UserErrorException) throw processedException;

                Logger.LogLine($"Exception: {exception.Message}.");
                return false;
            }
        }
    }
}