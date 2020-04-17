using System.Collections.Generic;
using System.IO;
using Downloader;
using IO;
using Moq;
using Xunit;

namespace UnitTests.Downloader
{
    public class AnnotationRepositoryTests
    {
        [Fact]
        public void DownloadFiles_Nominal()
        {
            const ushort dataVersion = CacheConstants.DataVersion;

            var clientMock = new Mock<IClient>();
            clientMock.Setup(x => x.DownloadFile(It.IsAny<RemoteFile>())).Returns(true).Verifiable();
            
            var files = new List<RemoteFile>
            {
                new RemoteFile($"remote/{dataVersion}/GRCh37/Both.transcripts.ndb", Path.Combine("local", "GRCh37", "Both.transcripts.ndb"), "Both.transcripts.ndb (GRCh37)"),
                new RemoteFile($"remote/{dataVersion}/GRCh37/Both.sift.ndb",        Path.Combine("local", "GRCh37", "Both.sift.ndb"),        "Both.sift.ndb (GRCh37)"),
                new RemoteFile($"remote/{dataVersion}/GRCh37/Both.polyphen.ndb",    Path.Combine("local", "GRCh37", "Both.polyphen.ndb"),    "Both.polyphen.ndb (GRCh37)"),
                new RemoteFile($"remote/{dataVersion}/GRCh38/Both.transcripts.ndb", Path.Combine("local", "GRCh38", "Both.transcripts.ndb"), "Both.transcripts.ndb (GRCh38)"),
                new RemoteFile($"remote/{dataVersion}/GRCh38/Both.sift.ndb",        Path.Combine("local", "GRCh38", "Both.sift.ndb"),        "Both.sift.ndb (GRCh38)"),
                new RemoteFile($"remote/{dataVersion}/GRCh38/Both.polyphen.ndb",    Path.Combine("local", "GRCh38", "Both.polyphen.ndb"),    "Both.polyphen.ndb (GRCh38)")
            };
            
            AnnotationRepository.DownloadFiles(clientMock.Object, files);
            clientMock.Verify(x => x.DownloadFile(It.IsAny<RemoteFile>()), Times.Exactly(6));
        }
    }
}