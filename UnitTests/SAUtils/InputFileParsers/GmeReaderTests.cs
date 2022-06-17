using System.IO;
using System.Linq;
using Genome;
using SAUtils.InputFileParsers.Gme;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using Variants;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class GmeTests
    {
        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            // file has been modified to 7 columns
            writer.WriteLine("#chrom\tpos\tref\talt\tfilter\tGME_GC\tGME_AC\tGME_AF");
            writer.WriteLine("1\t69134\tA\tG\tVQSRTrancheSNP99.90to100.00\t10,192\t0.04950495049504951");
            writer.WriteLine("1\t69270\tA\tG\tPASS\t518,224\t0.6981132075471698");
            writer.WriteLine("1\t69428\tT\tG\tPASS\t74,1396\t0.050340136054421766");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void GetItems_test()
        {
            var sequence = new SimpleSequence(new string('T', VariantUtils.MaxUpstreamLength) + "A" +new string('T', 69270- 69134) + "A" +new string('T', 69428- 69270-1)+ "T", 69134 - 1 - VariantUtils.MaxUpstreamLength);

            var seqProvider  = new SimpleSequenceProvider(GenomeAssembly.GRCh37, sequence, ChromosomeUtilities.RefNameToChromosome);
            var gmeReader = new GmeParser(new StreamReader(GetStream()), seqProvider);

            var items = gmeReader.GetItems().ToList();

            Assert.Equal(3,                                                                   items.Count);
            Assert.Equal("\"allAc\":10,\"allAn\":202,\"allAf\":0.0495,\"failedFilter\":true", items[0].GetJsonString());
        }
    }
}