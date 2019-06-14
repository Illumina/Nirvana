using System.Collections.Generic;
using System.IO;
using System.Threading;
using Downloader;
using Downloader.FileExtensions;
using Moq;
using VariantAnnotation.IO.Caches;
using Xunit;

namespace UnitTests.Downloader.FileExtensions
{
    public sealed class DownloadExtensionsTests
    {
        [Fact]
        public void Download_Nominal()
        {
            const ushort dataVersion = CacheConstants.DataVersion;

            var clientMock = new Mock<IClient>();
            clientMock.Setup(x => x.Download(It.IsAny<RemoteFile>(), It.IsAny<CancellationTokenSource>())).Verifiable();

            var files = new List<RemoteFile>
            {
                new RemoteFile($"remote/{dataVersion}/GRCh37/Both.transcripts.ndb", Path.Combine("local", "GRCh37", "Both.transcripts.ndb"), "Both.transcripts.ndb (GRCh37)"),
                new RemoteFile($"remote/{dataVersion}/GRCh37/Both.sift.ndb",        Path.Combine("local", "GRCh37", "Both.sift.ndb"),        "Both.sift.ndb (GRCh37)"),
                new RemoteFile($"remote/{dataVersion}/GRCh37/Both.polyphen.ndb",    Path.Combine("local", "GRCh37", "Both.polyphen.ndb"),    "Both.polyphen.ndb (GRCh37)"),
                new RemoteFile($"remote/{dataVersion}/GRCh38/Both.transcripts.ndb", Path.Combine("local", "GRCh38", "Both.transcripts.ndb"), "Both.transcripts.ndb (GRCh38)"),
                new RemoteFile($"remote/{dataVersion}/GRCh38/Both.sift.ndb",        Path.Combine("local", "GRCh38", "Both.sift.ndb"),        "Both.sift.ndb (GRCh38)"),
                new RemoteFile($"remote/{dataVersion}/GRCh38/Both.polyphen.ndb",    Path.Combine("local", "GRCh38", "Both.polyphen.ndb"),    "Both.polyphen.ndb (GRCh38)")
            };

            files.Download(clientMock.Object);
            clientMock.Verify(x => x.Download(It.IsAny<RemoteFile>(), It.IsAny<CancellationTokenSource>()), Times.Exactly(6));
        }
    }
}
