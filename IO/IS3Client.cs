using System.Threading.Tasks;
using Amazon.S3.Model;

namespace IO
{
    public interface IS3Client
    {
        Task<GetObjectResponse> GetObjectAsync(GetObjectRequest getRequest);
        Task<PutObjectResponse> PutObjectAsync(PutObjectRequest putRequest);
        bool DoesBucketExist(string bucketName);
        string GetPreSignedUrl(GetPreSignedUrlRequest request);
    }
}