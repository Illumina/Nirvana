using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nirvana;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Nirvana
{
    public sealed class AnnotationFilesTests
    {
        [Fact]
        public void GetFiles_FromDirectory_AsExpected()
        {
            var files = new AnnotationFiles();
            var saDirectory = Resources.MockSaFiles;
            files.AddFiles(saDirectory);

            var expectedNsaFiles = new List<(string, string)>
            {
                (Path.Combine(saDirectory, "sa1.nsa"), Path.Combine(saDirectory, "sa1.nsa.idx")),
                (Path.Combine(saDirectory, "sa2.nsa"), Path.Combine(saDirectory, "sa2.nsa.idx"))
            };

            var expectedNsiFiles = new List<string>
            {
                Path.Combine(saDirectory, "sa3.nsi"),
                Path.Combine(saDirectory, "sa4.nsi")
            };

            var expectedConservationFile = (Path.Combine(saDirectory, "sa5.npd"), Path.Combine(saDirectory, "sa5.npd.idx"));

            var expectedNgaFiles = new List<string>
            {
                Path.Combine(saDirectory, "sa6.nga"),
                Path.Combine(saDirectory, "sa7.nga")
            };

            var expectedRefMinorFile = (Path.Combine(saDirectory, "sa8.rma"), Path.Combine(saDirectory, "sa8.rma.idx"));

            Assert.Equal(expectedNsaFiles, files.NsaFiles.OrderBy(x=> x.Nsa));
            Assert.Equal(expectedNsiFiles, files.NsiFiles.OrderBy(x=>x));
            Assert.Equal(expectedConservationFile, files.ConservationFile);
            Assert.Equal(expectedNgaFiles, files.NgaFiles.OrderBy(x=>x));
            Assert.Equal(expectedRefMinorFile, files.RefMinorFile);
        }

        [Fact]
        public void GetFiles_FromDirectoryNoSa_NoFileAdded()
        {
            var files = new AnnotationFiles();
            files.AddFiles(".");

            Assert.Empty(files.NsaFiles);
            Assert.Empty(files.NsiFiles);
            Assert.Empty(files.NgaFiles);
            Assert.Equal(default, files.ConservationFile);
            Assert.Equal(default, files.RefMinorFile);
        }
    }
}
