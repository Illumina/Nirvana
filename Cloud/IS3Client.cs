using System.IO;
using System.Threading.Tasks;
using Amazon.S3.Model;

namespace Cloud
{
    public interface IS3Client
    {
        Stream GetStream(S3Path s3Path, ByteRange byteRange);
        long GetFileSize(S3Path s3Path);
        string Upload(S3Path targetS3Path, string localFilePath);
    }
}