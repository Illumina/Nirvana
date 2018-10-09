using System.IO;
using Amazon.S3.Model;
using IO;
using IO.StreamSource;

namespace Cloud
{
    public sealed class S3StreamSource : IStreamSource
    {
        private readonly IS3Client _s3Client;
        private readonly S3Path _s3Path;

        public S3StreamSource(IS3Client s3Client, S3Path s3Path)
        {
            _s3Client = s3Client;
            _s3Path = s3Path;
        }

        private S3StreamSource(IS3Client s3Client, string bucketName, string filePath)
        {
            _s3Client = s3Client;
            _s3Path = new S3Path
            {
                bucketName = bucketName,
                path = filePath
            };
        }

        public Stream GetStream(long start = 0) => new SeekableStream(this, start);
        public Stream GetStream(ByteRange byteRange) => new SeekableStream(this, byteRange.Start, byteRange.End);

        public Stream GetRawStream(long start, long end) => GetStreamRangeTask(start, end);

        public IStreamSource GetAssociatedStreamSource(string extraExtension) => new S3StreamSource(_s3Client, _s3Path.bucketName, _s3Path.path + extraExtension);
        public long GetLength() => _s3Client.GetFileSize(_s3Path);

        private  Stream GetStreamRangeTask(long start, long end)
        {
            var byteRange = start == 0 && end == long.MaxValue ? null : new ByteRange(start, end);
            return _s3Client.GetStream(_s3Path, byteRange);
        }
    }
}