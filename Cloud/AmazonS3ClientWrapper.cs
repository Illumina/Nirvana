using System;
using System.IO;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using IO;

namespace Cloud
{
    public sealed class AmazonS3ClientWrapper : IS3Client
    {
        private readonly AmazonS3Client _s3Client;

        public AmazonS3ClientWrapper(string accessKey, string secrectKey, RegionEndpoint regionEndpoint)
        {
            _s3Client = new AmazonS3Client(accessKey, secrectKey, regionEndpoint);
        }

        public Stream GetStream(S3Path s3Path, ByteRange byteRange)
        {
            var getRequestInfo = GetBucketAndFileNamesForS3Request(s3Path);
            return GetStreamRange(getRequestInfo.BuckNameForS3Request, getRequestInfo.FileName, byteRange);
        }

        public long GetFileSize(S3Path input)
        {
            var getRequestInfo = GetBucketAndFileNamesForS3Request(input);

            var metadataRequest = new GetObjectMetadataRequest
            {
                BucketName = getRequestInfo.BuckNameForS3Request,
                Key = getRequestInfo.FileName
            };

            var getMetadataResponse = FailureRecovery.CallWithRetry(() => _s3Client.GetObjectMetadataAsync(metadataRequest).Result, out int retryCounter);

            return getMetadataResponse.ContentLength;
        }

        public string Upload(S3Path targetS3FilePath, string localFilePath)
        {
            var requestInfo = GetBucketAndFileNamesForS3Request(targetS3FilePath);

            TransferUtility transferUtility = new TransferUtility(_s3Client);

            TransferUtilityUploadRequest transferUtilityRequest = new TransferUtilityUploadRequest
            {
                BucketName = requestInfo.BuckNameForS3Request,
                FilePath = localFilePath,
                Key = requestInfo.FileName
            };

            transferUtility.Upload(transferUtilityRequest);

            return targetS3FilePath.path;
        }

        private Stream GetStreamRange(string bucketNameForGetRequest, string s3FileName, ByteRange byteRange)
        {
            GetObjectRequest getRequest = new GetObjectRequest
            {
                BucketName = bucketNameForGetRequest,
                Key = s3FileName,
                ByteRange = byteRange
            };

            return FailureRecovery.CallWithRetry(() => GetResponseStream(getRequest), out int retryCounter);
        }

        private Stream GetResponseStream(GetObjectRequest getRequest) =>_s3Client.GetObjectAsync(getRequest).Result.ResponseStream;


        public static (string BuckNameForS3Request, string FileName) GetBucketAndFileNamesForS3Request(S3Path s3Path)
        {
            var pathParts = s3Path.path.TrimStart('/').Split('/');
            int nParts = pathParts.Length;
            string s3Directory = string.Join('/', new ArraySegment<string>(pathParts, 0, nParts - 1));
            string s3FileName = pathParts[nParts - 1];
            string bucketNameForS3Request = $"{s3Path.bucketName}/{s3Directory}";

            return (bucketNameForS3Request, s3FileName);
        }
    }
}