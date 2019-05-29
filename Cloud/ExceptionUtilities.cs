using System;
using System.Linq;
using ErrorHandling.Exceptions;

namespace Cloud
{
    public static class ExceptionUtilities
    {

        public static Exception ProcessAmazonS3Exception(Exception exception, S3Path s3Path)
        {
            Amazon.S3.AmazonS3Exception s3Exception;
            while ((s3Exception = exception as Amazon.S3.AmazonS3Exception) == null)
            {
                if (exception.InnerException == null) return exception;
                exception = exception.InnerException;
            }

            string extraInfo;
            switch (s3Exception.ErrorCode)
            {
                case "ExpiredToken":
                case "InvalidToken":
                    extraInfo = s3Path?.sessionToken;
                    break;
                case "InvalidAccessKeyId":
                    extraInfo = s3Path?.accessKey;
                    break;
                case "SignatureDoesNotMatch":
                    extraInfo = s3Path?.secretKey;
                    break;
                case "NoSuchBucket":
                    extraInfo = s3Path?.bucketName;
                    break;
                case "NoSuchKey":
                    extraInfo = s3Path?.path;
                    break;
                default:
                    return s3Exception;
            }

            string errorMessage = extraInfo == null
                ? s3Exception.Message
                : $"{s3Exception.Message} ({extraInfo})";

            return new UserErrorException(errorMessage);
        }
    }
}