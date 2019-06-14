using System.Collections.Generic;
using System.IO;
using Downloader;
using Downloader.FileExtensions;
using Genome;
using Xunit;

namespace UnitTests.Downloader.FileExtensions
{
    public sealed class SupplementaryAnnotationFileExtensionsTests
    {
        [Fact]
        public void AddSupplementaryAnnotationFiles_Nominal()
        {
            var comparer             = new RemoteFileComparer();
            const string saDirectory = "local";

            var remotePaths37 = new List<string>
            {
                "/0bf0cb93e64824b20f0b551a629596fd-TopMed/2/GRCh37/TOPMed_freeze_5.nsa"
            };


            var remotePaths38 = new List<string>
            {
                "/43cafec8b0624b77663e2ba1dec32883-gnomAD-exome/2/GRCh38/gnomAD_exome_2.0.2.nsa",
                "/2551e067cb59c540a4da905a99ee5ff4-ClinGen/2/GRCh38/ClinGen_20160414.nsi"
            };

            var remotePathsByGenomeAssembly = new Dictionary<GenomeAssembly, List<string>>
            {
                [GenomeAssembly.GRCh37] = remotePaths37,
                [GenomeAssembly.GRCh38] = remotePaths38
            };

            var expectedFiles = new List<RemoteFile>
            {
                new RemoteFile("/0bf0cb93e64824b20f0b551a629596fd-TopMed/2/GRCh37/TOPMed_freeze_5.nsa", Path.Combine("local", "GRCh37", "TOPMed_freeze_5.nsa"), "TOPMed_freeze_5.nsa (GRCh37)"),
                new RemoteFile("/0bf0cb93e64824b20f0b551a629596fd-TopMed/2/GRCh37/TOPMed_freeze_5.nsa.idx", Path.Combine("local", "GRCh37", "TOPMed_freeze_5.nsa.idx"), "TOPMed_freeze_5.nsa.idx (GRCh37)"),
                new RemoteFile("/43cafec8b0624b77663e2ba1dec32883-gnomAD-exome/2/GRCh38/gnomAD_exome_2.0.2.nsa", Path.Combine("local", "GRCh38", "gnomAD_exome_2.0.2.nsa"), "gnomAD_exome_2.0.2.nsa (GRCh38)"),
                new RemoteFile("/43cafec8b0624b77663e2ba1dec32883-gnomAD-exome/2/GRCh38/gnomAD_exome_2.0.2.nsa.idx", Path.Combine("local", "GRCh38", "gnomAD_exome_2.0.2.nsa.idx"), "gnomAD_exome_2.0.2.nsa.idx (GRCh38)"),
                new RemoteFile("/2551e067cb59c540a4da905a99ee5ff4-ClinGen/2/GRCh38/ClinGen_20160414.nsi", Path.Combine("local", "GRCh38", "ClinGen_20160414.nsi"), "ClinGen_20160414.nsi (GRCh38)")
            };

            var files = new List<RemoteFile>();
            files.AddSupplementaryAnnotationFiles(remotePathsByGenomeAssembly, saDirectory);

            Assert.Equal(expectedFiles, files, comparer);
        }
    }
}