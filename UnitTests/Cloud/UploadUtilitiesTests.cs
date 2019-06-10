using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Cloud;
using IO;
using Moq;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Cloud
{
    public sealed class UploadUtilitiesTests
    {
        private readonly FileMetadata _metadata = new FileMetadata(new byte[] { 0, 1, 2, 3, 4, 5, 6 }, 1);
        private readonly string _filePath = Resources.TopPath("clinvar.dict");
        private readonly AesCryptoServiceProvider _aes = new AesCryptoServiceProvider();

        private static Mock<IS3Client> GetS3ClientMock()
        {
            var s3ClientMock = new Mock<IS3Client>();
            s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>())).ReturnsAsync(new PutObjectResponse());
            return s3ClientMock;
        }

        private static Mock<IS3Client> GetS3ClientMockAlwaysFail()
        {
            var s3ClientMock = new Mock<IS3Client>();
            s3ClientMock.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>())).ThrowsAsync(new WebException());
            return s3ClientMock;
        }

        private static Mock<IS3Client> GetS3ClientMockCanWorkAfterRetries()
        {
            var s3ClientMock = new Mock<IS3Client>();
            s3ClientMock.SetupSequence(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>()))
                .ThrowsAsync(new WebException())
                .ThrowsAsync(new WebException())
                .ReturnsAsync(new PutObjectResponse())
                .ThrowsAsync(new WebException());

            return s3ClientMock;
        }

        [Fact]
        public void TryDecryptUpload_AsExpected()
        {
            var s3ClientMock = GetS3ClientMock();
            Assert.True(s3ClientMock.Object.TryDecryptUpload("bucket", "bob.json.gz", _filePath, _aes, _metadata));
            s3ClientMock.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>()), Times.Once);
        }

        [Fact]
        public void TryDecryptUpload_FileNotFound()
        {
            var s3ClientMock = GetS3ClientMock();
            Assert.False(s3ClientMock.Object.TryDecryptUpload("bucket", "bob.json.gz", "bob123", _aes, _metadata));
            s3ClientMock.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>()), Times.Never);
        }

        [Fact]
        public void DecryptUpload_OnlyPutOnceWhenSuccess()
        {
            var s3ClientMock = GetS3ClientMock();

            s3ClientMock.Object.DecryptUpload("bucket", "bob.json.gz", _filePath, _aes, _metadata, 1);
            s3ClientMock.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>()), Times.Once);
        }

        [Fact]
        public void DecryptUpload_SuccessWithRetries()
        {
            var s3ClientMock = GetS3ClientMockCanWorkAfterRetries();

            s3ClientMock.Object.DecryptUpload("bucket", "bob.json.gz", _filePath, _aes, _metadata, 1);
            s3ClientMock.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>()), Times.Exactly(3));
        }

        [Fact]
        public void DecryptUpload_TimeOutWhenFail()
        {
            var timeOut = TimeSpan.FromMilliseconds(500);
            var s3ClientMockAlwaysFail = GetS3ClientMockAlwaysFail();
            var failTask = Task.Run(() => s3ClientMockAlwaysFail.Object.DecryptUpload("bucket", "bob.json.gz", _filePath, _aes, _metadata, 1));
      
            Assert.False(Task.WaitAll(new[] { failTask }, timeOut));
            s3ClientMockAlwaysFail.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>()), Times.AtLeast(2));
        }
    }
}