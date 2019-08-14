using System.Collections.Generic;
using System.IO;
using Genome;
using Nirvana;
using UnitTests.SAUtils.InputFileParsers;
using Xunit;

namespace UnitTests.Nirvana
{
    public sealed class PreLoadUtilitiesTests
    {
        private static readonly IChromosome Chrom1 = new Chromosome("chr1", "1", 0);
        private static readonly IChromosome Chrom2 = new Chromosome("chr2", "2", 1);

        private readonly Dictionary<string, IChromosome> _chromDict = new Dictionary<string, IChromosome>
        {
            { "1", Chrom1},
            { "2", Chrom2}
        };

        private static Stream GetVcfStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##dbSNP");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("1\t10019\trs775809821\tTA\tT\t.\t.\tRS=775809821;RSPOS=10020;dbSNPBuildID=144;SSR=0;SAO=0;VP=0x050000020005000002000200;GENEINFO=DDX11L1:100287102;WGT=1;VC=DIV;R5;ASP");
            writer.WriteLine("1\t10285\trs866375379\tT\tA,C\t.\t.\tRS=866375379;RSPOS=10285;dbSNPBuildID=147;SSR=0;SAO=0;VP=0x050100020005000002000100;GENEINFO=DDX11L1:100287102;WGT=1;VC=SNV;SLO;R5;ASP");
            writer.WriteLine("1\t10329\trs150969722\tAC\tA\t.\t.\tRS=150969722;RSPOS=10330;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000020005000002000200;GENEINFO=DDX11L1:100287102;WGT=1;VC=DIV;R5;ASP");
            writer.WriteLine("2\t10019\trs775809821\tTA\tT\t.\t.\tRS=775809821;RSPOS=10020;dbSNPBuildID=144;SSR=0;SAO=0;VP=0x050000020005000002000200;GENEINFO=DDX11L1:100287102;WGT=1;VC=DIV;R5;ASP");
            writer.WriteLine("2\t10285\trs866375379\tT\tA,C\t.\t.\tRS=866375379;RSPOS=10285;dbSNPBuildID=147;SSR=0;SAO=0;VP=0x050100020005000002000100;GENEINFO=DDX11L1:100287102;WGT=1;VC=SNV;SLO;R5;ASP");
            writer.WriteLine("2\t10329\trs150969722\tAC\tA\t.\t.\tRS=150969722;RSPOS=10330;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000020005000002000200;GENEINFO=DDX11L1:100287102;WGT=1;VC=DIV;R5;ASP");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void GetAllPositions()
        {
            var seqProvider = ParserTestUtils.GetSequenceProvider(10329, "AC", 'A', _chromDict);
            var positions = PreLoadUtilities.GetPositions(GetVcfStream(), null, seqProvider);

            Assert.Equal(2, positions.Count);
            Assert.Equal(4, positions[Chrom1].Count);
            Assert.Equal(4, positions[Chrom2].Count);
        }

        [Fact]
        public void GetPositions_inRange()
        {
            var chromosome = new Chromosome("chr1", "1", 0);
            var annotationRange = new GenomicRange(new GenomicPosition(chromosome, 10019), new GenomicPosition(chromosome, 10290));
            var seqProvider = ParserTestUtils.GetSequenceProvider(10329, "AC", 'A', _chromDict);
            var positions = PreLoadUtilities.GetPositions(GetVcfStream(), annotationRange, seqProvider );

            Assert.Single(positions);
            Assert.Equal(3, positions[Chrom1].Count);
        }
    }
}