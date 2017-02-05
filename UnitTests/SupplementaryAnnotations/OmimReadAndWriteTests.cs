using System.IO;
using System.Linq;
using SAUtils.CreateOmimDatabase;
using SAUtils.InputFileParsers.Omim;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling.Omim;
using Xunit;

namespace UnitTests.SupplementaryAnnotations
{
    public sealed class OmimReadAndWriteTests
    {
        [Fact]
        void ReadAndWriteTests()
        {
            var omimFile    = Resources.TopPath("testOmim.txt");
            var randomPath  = Path.GetTempPath();
            var omimCreater = new CreateOmimDatabase(omimFile, randomPath);
            omimCreater.Create();

            var omimDataBaseFile = Path.Combine(randomPath, OmimDatabaseCommon.OmimDatabaseFileName);
            var omimDatabaseReader = new OmimDatabaseReader(omimDataBaseFile);

            var omimEntries = omimDatabaseReader.Read().ToList();

            var omimReader = new OmimReader(new FileInfo(omimFile));
            var expectedOmimEntries = omimReader.ToList();

            Assert.Equal(6, expectedOmimEntries.Count);

            Assert.Equal(expectedOmimEntries.Count, omimEntries.Count);
            Assert.True(omimEntries.SequenceEqual(expectedOmimEntries));
        }
    }
}