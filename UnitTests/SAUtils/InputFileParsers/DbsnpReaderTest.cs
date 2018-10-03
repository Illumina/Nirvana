using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using SAUtils.InputFileParsers.DbSnp;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class DbsnpReaderTest
    {
        private static readonly IChromosome Chrom1 = new Chromosome("chr1", "1", 1);
        private static readonly IChromosome Chrom2 = new Chromosome("chr2", "2", 2);

        private readonly Dictionary<string, IChromosome> _chromDict = new Dictionary<string, IChromosome>()
        {
            { "1", Chrom1},
            { "2", Chrom2}
        };

        private Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##dbSNP");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("1\t10285\trs866375379\tT\tA,C\t.\t.\tRS=866375379;RSPOS=10285;dbSNPBuildID=147;SSR=0;SAO=0;VP=0x050100020005000002000100;GENEINFO=DDX11L1:100287102;WGT=1;VC=SNV;SLO;R5;ASP");
            writer.WriteLine("1\t10329\trs150969722\tAC\tA\t.\t.\tRS=150969722;RSPOS=10330;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000020005000002000200;GENEINFO=DDX11L1:100287102;WGT=1;VC=DIV;R5;ASP");
            
            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void GetItems_test()
        {
            var reader = new DbSnpReader(GetStream(), _chromDict);

            var items = reader.GetItems().ToList();

            Assert.Equal(3, items.Count);
            Assert.Equal("\"rs866375379\"", items[0].GetJsonString());
        }
    }
}