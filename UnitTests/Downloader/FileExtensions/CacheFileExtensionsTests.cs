using System.Collections.Generic;
using System.IO;
using Downloader;
using Downloader.FileExtensions;
using Genome;
using VariantAnnotation.IO.Caches;
using Xunit;

namespace UnitTests.Downloader.FileExtensions
{
    public sealed class CacheFileExtensionsTests
    {
        [Fact]
        public void AddCacheFiles_Nominal()
        {
            var comparer = new RemoteFileComparer();
            var genomeAssemblies              = new List<GenomeAssembly> { GenomeAssembly.GRCh37, GenomeAssembly.GRCh38 };
            const string remoteCacheDirectory = "remote";
            const string cacheDirectory       = "local";

            const ushort dataVersion = CacheConstants.DataVersion;

            var expectedFiles = new List<RemoteFile>
            {
                new RemoteFile($"remote/{dataVersion}/GRCh37/Both.transcripts.ndb", Path.Combine("local", "GRCh37", "Both.transcripts.ndb"), "Both.transcripts.ndb (GRCh37)"),
                new RemoteFile($"remote/{dataVersion}/GRCh37/Both.sift.ndb",        Path.Combine("local", "GRCh37", "Both.sift.ndb"),        "Both.sift.ndb (GRCh37)"),
                new RemoteFile($"remote/{dataVersion}/GRCh37/Both.polyphen.ndb",    Path.Combine("local", "GRCh37", "Both.polyphen.ndb"),    "Both.polyphen.ndb (GRCh37)"),
                new RemoteFile($"remote/{dataVersion}/GRCh38/Both.transcripts.ndb", Path.Combine("local", "GRCh38", "Both.transcripts.ndb"), "Both.transcripts.ndb (GRCh38)"),
                new RemoteFile($"remote/{dataVersion}/GRCh38/Both.sift.ndb",        Path.Combine("local", "GRCh38", "Both.sift.ndb"),        "Both.sift.ndb (GRCh38)"),
                new RemoteFile($"remote/{dataVersion}/GRCh38/Both.polyphen.ndb",    Path.Combine("local", "GRCh38", "Both.polyphen.ndb"),    "Both.polyphen.ndb (GRCh38)")
            };

            var files = new List<RemoteFile>();
            files.AddCacheFiles(genomeAssemblies, remoteCacheDirectory, cacheDirectory);

            Assert.Equal(expectedFiles, files, comparer);
        }
    }
}
