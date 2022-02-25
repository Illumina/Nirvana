using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using SAUtils;
using SAUtils.GenericScore;
using SAUtils.GenericScore.GenericScoreParser;
using UnitTests.TestUtilities;
using VariantAnnotation.GenericScore;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.VariantAnnotation.ScoreFile
{
    public sealed class HeaderTests
    {
        [Fact]
        public void TestFilePairId()
        {
            (
                List<GenericScoreItem> saItems1,
                WriterSettings writerSettings1,
                MemoryStream indexStream1,
                MemoryStream writeStream1,
                DataSourceVersion version1,
                _
            ) = TestDataGenerator.GetRandomSingleChromosomeData(ChromosomeUtilities.Chr1, 10_001, 15_001);
            (
                List<GenericScoreItem> saItems2,
                WriterSettings writerSettings2,
                MemoryStream indexStream2,
                MemoryStream writeStream2,
                DataSourceVersion version2,
                _
            ) = TestDataGenerator.GetRandomSingleChromosomeData(ChromosomeUtilities.Chr1, 10_001, 15_001);

            using (var scoreFileWriter1 = new ScoreFileWriter(
                       writerSettings1,
                       writeStream1,
                       indexStream1,
                       version1,
                       TestDataGenerator.GetSequenceProvider(),
                       SaCommon.SchemaVersion
                   ))
            using (var scoreFileWriter2 = new ScoreFileWriter(
                       writerSettings2,
                       writeStream2,
                       indexStream2,
                       version2,
                       TestDataGenerator.GetSequenceProvider(),
                       SaCommon.SchemaVersion
                   ))
            {
                // Write saItems to stream
                scoreFileWriter1.Write(saItems1);
                scoreFileWriter2.Write(saItems2);

                // Reset streams in preparation for reading them
                indexStream1.Position = 0;
                indexStream2.Position = 0;
                writeStream1.Position = 0;
                writeStream2.Position = 0;
                // Mixing indexes with different data files must throw exception
                Assert.Throws<UserErrorException>(() => ScoreReader.Read(writeStream2, indexStream1));
                Assert.Throws<UserErrorException>(() => ScoreReader.Read(writeStream1, indexStream2));

                indexStream1.Position = 0;
                indexStream2.Position = 0;
                writeStream1.Position = 0;
                writeStream2.Position = 0;
                // Shoud not throw any exception
                ScoreReader.Read(writeStream1, indexStream1);
                ScoreReader.Read(writeStream2, indexStream2);
            }
        }

        [Fact]
        public void TestFileType()
        {
            (
                List<GenericScoreItem> saItems1,
                WriterSettings writerSettings1,
                MemoryStream indexStream1,
                MemoryStream writeStream1,
                DataSourceVersion version1,
                _
            ) = TestDataGenerator.GetRandomSingleChromosomeData(ChromosomeUtilities.Chr1, 10_001, 15_001);
            (
                List<GenericScoreItem> saItems2,
                _,
                MemoryStream indexStream2,
                MemoryStream writeStream2,
                DataSourceVersion version2,
                _
            ) = TestDataGenerator.GetRandomSingleChromosomeData(ChromosomeUtilities.Chr1, 10_001, 15_001);

            using (var scoreFileWriter1 = new ScoreFileWriter(
                       writerSettings1,
                       writeStream1,
                       indexStream1,
                       version1,
                       TestDataGenerator.GetSequenceProvider(),
                       SaCommon.SchemaVersion
                   ))
            using (var nsaWriter = new NsaWriter(
                       writeStream2,
                       indexStream2,
                       version2,
                       TestDataGenerator.GetSequenceProvider(),
                       "TestNsa",
                       true,
                       false,
                       SaCommon.SchemaVersion,
                       false
                   ))
            {
                scoreFileWriter1.Write(saItems1);
                nsaWriter.Write(saItems2);

                // Reset streams in preparation for reading them
                indexStream1.Position = 0;
                indexStream2.Position = 0;
                writeStream1.Position = 0;
                writeStream2.Position = 0;
                // Attempting to read NSA file with this score reader must throw exception
                Assert.Throws<InvalidDataException>(() => ScoreReader.Read(writeStream2, indexStream1));
                Assert.Throws<InvalidDataException>(() => ScoreReader.Read(writeStream1, indexStream2));

                indexStream1.Position = 0;
                indexStream2.Position = 0;
                writeStream1.Position = 0;
                writeStream2.Position = 0;
                // Shoud not throw any exception
                ScoreReader.Read(writeStream1, indexStream1);
            }
        }
    }
}