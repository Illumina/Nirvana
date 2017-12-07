using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.DGV;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class DgvReaderTests
    {
        private static IChromosome chrom1 = new Chromosome("chr1", "1", 0);
        private static readonly IDictionary<string, IChromosome> RefChromDict = new Dictionary<string, IChromosome>
        {
            {"chr1",chrom1 },
            {"1",chrom1}
        };


        private static readonly FileInfo TestDgvFile = new FileInfo(Resources.TopPath("testDgvParser.txt"));

        private static IEnumerable<DgvItem> CreateTruthDgvItemSequence()
        {
            yield return new DgvItem("nsv945265", chrom1, 352306, 371739, 97, 10, 0, VariantType.complex_structural_alteration);
            yield return new DgvItem("nsv161172", chrom1, 88190, 89153, 24, 0, 0, VariantType.copy_number_loss);
            yield return new DgvItem("nsv951399", chrom1, 46501, 71800, 1, 1, 0, VariantType.copy_number_gain);
            yield return new DgvItem("nsv471522", chrom1, 522139, 756783, 3, 3, 0, VariantType.copy_number_gain);
            yield return new DgvItem("nsv10161", chrom1, 712111, 1708649, 31, 11, 7, VariantType.copy_number_variation);
            yield return new DgvItem("esv3358119", chrom1, 822853, 822861, 185, 2, 0, VariantType.insertion);
            yield return new DgvItem("esv6890", chrom1, 17006189, 17052558, 1, 0, 0, VariantType.inversion);
            yield return new DgvItem("esv6517", chrom1, 964760, 965579, 1, 0, 0, VariantType.copy_number_loss);
            yield return new DgvItem("esv3310333", chrom1, 17441132, 17441133, 185, 3, 0, VariantType.mobile_element_insertion);
            yield return new DgvItem("nsv479682", chrom1, 3787207, 3787207, 9, 0, 0, VariantType.novel_sequence_insertion);
            yield return new DgvItem("nsv506926", chrom1, 34597680, 34603680, 4, 0, 0, VariantType.structural_alteration);
            yield return new DgvItem("esv3302766", chrom1, 38583768, 38583926, 185, 0, 0, VariantType.tandem_duplication);
        }

        [Fact]
        public void TestDbSnpReader()
        {
            var dgvReader = new DgvReader(TestDgvFile, RefChromDict);
            Assert.True(dgvReader.GetDgvItems().SequenceEqual(CreateTruthDgvItemSequence()));
        }
    }
}