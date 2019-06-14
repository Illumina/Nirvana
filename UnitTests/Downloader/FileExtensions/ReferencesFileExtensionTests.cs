using System.Collections.Generic;
using System.IO;
using Downloader;
using Downloader.FileExtensions;
using Genome;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.Downloader.FileExtensions
{
    public sealed class ReferencesFileExtensionsTests
    {
        [Fact]
        public void AddReferenceFiles_Nominal()
        {
            var comparer                           = new RemoteFileComparer();
            var genomeAssemblies                   = new List<GenomeAssembly> { GenomeAssembly.GRCh37, GenomeAssembly.GRCh38 };
            const string remoteReferencesDirectory = "remote";
            const string referencesDirectory       = "local";

            const ushort dataVersion = CompressedSequenceCommon.HeaderVersion;

            var expectedFiles = new List<RemoteFile>
            {
                new RemoteFile($"remote/{dataVersion}/Homo_sapiens.GRCh37.Nirvana.dat", Path.Combine("local", "Homo_sapiens.GRCh37.Nirvana.dat"), "Homo_sapiens.GRCh37.Nirvana.dat"),
                new RemoteFile($"remote/{dataVersion}/Homo_sapiens.GRCh38.Nirvana.dat", Path.Combine("local", "Homo_sapiens.GRCh38.Nirvana.dat"), "Homo_sapiens.GRCh38.Nirvana.dat")
            };

            var files = new List<RemoteFile>();
            files.AddReferenceFiles(genomeAssemblies, remoteReferencesDirectory, referencesDirectory);

            Assert.Equal(expectedFiles, files, comparer);
        }
    }
}