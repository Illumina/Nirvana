using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.InputFileParsers.DGV;
using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.FileHandling.SaFileParsers
{
    [Collection("ChromosomeRenamer")]
    public sealed class DgvReaderTests
    {
        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public DgvReaderTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        private static readonly FileInfo TestDgvFile = new FileInfo(Resources.TopPath("testDgvParser.txt"));

        private static IEnumerable<DgvItem> CreateTruthDgvItemSequence()
        {
            yield return new DgvItem("nsv945265", "1", 352306, 371739, 97, 10, 0, VariantType.complex_structural_alteration);
            yield return new DgvItem("nsv161172", "1", 88190, 89153, 24, 0, 0, VariantType.copy_number_loss);
            yield return new DgvItem("nsv951399", "1", 46501, 71800, 1, 1, 0, VariantType.copy_number_gain);
            yield return new DgvItem("nsv471522", "1", 522139, 756783, 3, 3, 0, VariantType.copy_number_gain);
            yield return new DgvItem("nsv10161", "1", 712111, 1708649, 31, 11, 7, VariantType.copy_number_variation);
            yield return new DgvItem("esv3358119", "1", 822853, 822861, 185, 2, 0, VariantType.insertion);
            yield return new DgvItem("esv6890", "1", 17006189, 17052558, 1, 0, 0, VariantType.inversion);
            yield return new DgvItem("esv6517", "1", 964760, 965579, 1, 0, 0, VariantType.copy_number_loss);
            yield return new DgvItem("esv3310333", "1", 17441132, 17441133, 185, 3, 0, VariantType.mobile_element_insertion);
            yield return new DgvItem("nsv479682", "1", 3787207, 3787207, 9, 0, 0, VariantType.novel_sequence_insertion);
            yield return new DgvItem("nsv506926", "1", 34597680, 34603680, 4, 0, 0, VariantType.structural_alteration);
            yield return new DgvItem("esv3302766", "1", 38583768, 38583926, 185, 0, 0, VariantType.tandem_duplication);
        }

        [Fact]
        public void TestDbSnpReader()
        {
            var dgvReader = new DgvReader(TestDgvFile, _renamer);
            Assert.True(dgvReader.SequenceEqual(CreateTruthDgvItemSequence()));
        }
    }
}