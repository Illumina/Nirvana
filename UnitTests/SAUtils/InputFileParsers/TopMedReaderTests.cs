using System.IO;
using System.Linq;
using Genome;
using SAUtils.InputFileParsers.TOPMed;
using UnitTests.TestDataStructures;
using UnitTests.TestUtilities;
using Variants;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class TopMedReaderTests
    {
        private Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##TopMED");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("chr1\t10128\trs796688738\tA\tAC\t255\tSVM;DISC\tVRT=2;NS=62784;AN=125568;AC=334;AF=0.00265991;Het=334;Hom=0\tNA:FRQ\t125568:0.00265991");
            writer.WriteLine("chr1\t10146\trs779258992\tAC\tA\t255\tSVM;DISC;EXHET\tVRT=2;NS=62784;AN=125568;AC=2897;AF=0.0230712;Het=2897;Hom=0\tNA:FRQ\t125568:0.0230712");
            writer.WriteLine("chr1\t10177\trs201752861\tA\tC\t255\tSVM;DISC\tVRT=1;NS=62784;AN=125568;AC=488;AF=0.00388634;Het=488;Hom=0\tNA:FRQ\t125568:0.00388634");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void GetItems_test()
        {
            var sequence = new SimpleSequence(new string('T', VariantUtils.MaxUpstreamLength) + "A" +new string('T', 10146- 10128) + "AC" +new string('T', 10177- 10146-1)+"A", 10128 - 1 - VariantUtils.MaxUpstreamLength);

            var seqProvider = new SimpleSequenceProvider(GenomeAssembly.GRCh37, sequence, ChromosomeUtilities.RefNameToChromosome);
            var gnomadReader = new TopMedReader(new StreamReader(GetStream()), seqProvider);

            var items = gnomadReader.GetItems().ToList();

            Assert.Equal(3, items.Count);
            Assert.Equal("\"allAf\":0.00266,\"allAn\":125568,\"allAc\":334,\"allHc\":0,\"failedFilter\":true", items[0].GetJsonString());
        }
    }
}