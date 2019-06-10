using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.DbSnp;
using VariantAnnotation.Interface.SA;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class GlobalMinorReaderTests
    {
        private static readonly IChromosome Chrom1 = new Chromosome("chr1", "1", 1);
        private static readonly IChromosome Chrom2 = new Chromosome("chr2", "2", 2);

        private readonly Dictionary<string, IChromosome> _chromDict = new Dictionary<string, IChromosome>
        {
            { "1", Chrom1},
            { "2", Chrom2}
        };

        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##dbSNP");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("1\t15274\trs2758118\tA\tG,T\t.\t.\tRS=2758118;RSPOS=15274;RV;dbSNPBuildID=111;SSR=0;SAO=0;VP=0x050000080005000126000100;GENEINFO=WASH7P:653635;WGT=1;VC=SNV;INT;ASP;GNO;KGPhase3;CAF=0.01178,0.3472,0.641;COMMON=1");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void GetItems_test()
        {
            var reader = new GlobalMinorReader(GetStream(), _chromDict);

            var items = reader.GetItems().Cast<ISupplementaryDataItem>().ToList();

            var globalMinor = SuppDataUtilities.GetPositionalAnnotation(items);

            Assert.Equal("{\"globalMinorAllele\":\"G\",\"globalMinorAlleleFrequency\":0.3472}", globalMinor.GetJsonString());
        }
    }
}