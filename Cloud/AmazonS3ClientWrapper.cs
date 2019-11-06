using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using IO;

namespace Cloud
{
    public sealed class AmazonS3ClientWrapper : IS3Client
    {
        private readonly AmazonS3Client _s3Client;

        public AmazonS3ClientWrapper(AmazonS3Client s3Client) => _s3Client = s3Client;

        public Task<GetObjectResponse> GetObjectAsync(GetObjectRequest getRequest) => _s3Client.GetObjectAsync(getRequest);

        public Task<PutObjectResponse> PutObjectAsync(PutObjectRequest putRequest) => _s3Client.PutObjectAsync(putRequest);
    }
}