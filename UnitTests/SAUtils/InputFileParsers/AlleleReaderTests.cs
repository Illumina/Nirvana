using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using SAUtils.InputFileParsers;
using UnitTests.TestDataStructures;
using Variants;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class AlleleReaderTests
    {
        private static readonly IChromosome Chrom1 = new Chromosome("chr1", "1", 1);


        private readonly Dictionary<string, IChromosome> _chromDict = new Dictionary<string, IChromosome>
        {
            { "1", Chrom1}
        };

        private static Stream GetAncestralAlleleStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##AncestralAllele");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("1\t13284\trs548333521\tG\tA\t100\tPASS\tAC=7;AF=0.00139776;AN=5008;NS=2504;DP=26384;EAS_AF=0.001;AMR_AF=0;AFR_AF=0.0045;EUR_AF=0;SAS_AF=0;AA=g|||;VT=SNP;EAS_AN=1008;EAS_AC=1;EUR_AN=1006;EUR_AC=0;AFR_AN=1322;AFR_AC=6;AMR_AN=694;AMR_AC=0;SAS_AN=978;SAS_AC=0");
            writer.WriteLine("1\t13289\trs568318295\tC\tT\t100\tPASS\tAC=3;AF=0.000599042;AN=5008;NS=2504;DP=25361;EAS_AF=0.003;AMR_AF=0;AFR_AF=0;EUR_AF=0;SAS_AF=0;AA=c|||;VT=SNP;EAS_AN=1008;EAS_AC=3;EUR_AN=1006;EUR_AC=0;AFR_AN=1322;AFR_AC=0;AMR_AN=694;AMR_AC=0;SAS_AN=978;SAS_AC=0");
            writer.WriteLine("1\t13313\trs527952245\tT\tG\t100\tPASS\tAC=1;AF=0.000199681;AN=5008;NS=2504;DP=20943;EAS_AF=0;AMR_AF=0;AFR_AF=0;EUR_AF=0.001;SAS_AF=0;AA=t|||;VT=SNP;EAS_AN=1008;EAS_AC=0;EUR_AN=1006;EUR_AC=1;AFR_AN=1322;AFR_AC=0;AMR_AN=694;AMR_AC=0;SAS_AN=978;SAS_AC=0");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void GetItems_test()
        {
            var sequence = new SimpleSequence(new string('T', VariantUtils.MaxUpstreamLength) + "G" + new string('T', 13289 - 13284) + "C" + new string('T', 13313 - 13289) + "T", 13284 - 1 - VariantUtils.MaxUpstreamLength);

            var seqProvider = new SimpleSequenceProvider(GenomeAssembly.GRCh37, sequence, _chromDict);
            var reader = new AncestralAlleleReader(new StreamReader(GetAncestralAlleleStream()), seqProvider);

            var items = reader.GetItems().ToList();

            Assert.Equal(3, items.Count);
            Assert.Equal("\"g\"", items[0].GetJsonString());
        }

    }
}