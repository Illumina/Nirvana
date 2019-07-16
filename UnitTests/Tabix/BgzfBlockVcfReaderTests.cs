using Compression.FileHandling;
using Genome;
using IO;
using Tabix;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Tabix
{
    public sealed class BgzfBlockVcfReaderTests
    {
        private readonly IChromosome _chr2  = new Chromosome("chr2", "2", 1);
        private readonly IChromosome _chr15 = new Chromosome("chr15", "15", 14);
        private const long FileOffset       = 61413;
        private readonly BgzfBlock _block   = new BgzfBlock();

        [Fact]
        public void FindVariantsInBlock_NoVariants_ReturnFalse()
        {
            using (var stream = FileUtilities.GetReadStream(Resources.TopPath("miniHEXA_minimal.vcf.gz")))
            {
                bool observedResults = BgzfBlockVcfReader.FindVariantsInBlocks(stream, FileOffset, FileOffset, _block,
                    _chr15, 1, 71589359);
                Assert.False(observedResults);
            }
        }

        [Fact]
        public void FindVariantsInBlock_ReturnTrue()
        {
            using (var stream = FileUtilities.GetReadStream(Resources.TopPath("miniHEXA_minimal.vcf.gz")))
            {
                bool observedResults = BgzfBlockVcfReader.FindVariantsInBlocks(stream, FileOffset, FileOffset, _block,
                    _chr15, 71589360, 71589361);
                Assert.False(observedResults);
            }
        }

        private const string MixedLineEndingsInput = "C\t39\t.\t.\tGT\t0/1\t.\t1/1\n1\t100\t.\tT\tC\t39\t.\t.\tGT\t0/1\t.\t1/1\r\n2\t55927\t.\tT\tC\t39\t.\t.\tGT\t0/1\t.\t1/1\n2\t55928\t.\tT\tC\t39\t.\t.\tGT\t0/1\t.\t1/1\r\n2\t55929\t.\tT\tC\t39\t.\t.\tGT\t0/1\t.\t1/1\n3\t200\t.\tT\tC\t39\t.\t.\tGT\t0/1\t.\t1/1\r\n1\t";


        [Fact]
        public void GetVcfPositions_MixedLineEndings_PartialEntries_MultipleChromosomes_ReturnTrue()
        {
            bool observedResults = BgzfBlockVcfReader.HasVcfPositionsOnInterval(MixedLineEndingsInput, _chr2, 55927, 55928);
            Assert.True(observedResults);
        }

        [Fact]
        public void GetVcfPositions_MixedLineEndings_PartialEntries_MultipleChromosomes_ReturnFalse()
        {
            bool observedResults = BgzfBlockVcfReader.HasVcfPositionsOnInterval(MixedLineEndingsInput, _chr2, 55930, 55940);
            Assert.False(observedResults);
        }

        [Fact]
        public void GetVcfPositions_SkipCorruptPositions()
        {
            const string input = "2\t55927i\t.\tT\tC\t39\t.\t.\tGT\t0/1\t.\t1/1\n2\t55928\t.\tT\tC\t39\t.\t.\tGT\t0/1\t.\t1/1";
            bool observedResults = BgzfBlockVcfReader.HasVcfPositionsOnInterval(input, _chr2, 55927, 55927);
            Assert.False(observedResults);
        }
    }
}
