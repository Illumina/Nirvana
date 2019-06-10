using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using SAUtils.InputFileParsers.DbSnp;
using UnitTests.TestDataStructures;
using Variants;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class DbSnpReaderTests
    {

        private readonly IDictionary<string, IChromosome> _refChromDict;
        private static readonly IChromosome Chr1 = new Chromosome("chr1", "1", 0);
        private static readonly IChromosome Chr2 = new Chromosome("chr2", "2", 2);
        private static readonly IChromosome Chr4 = new Chromosome("chr4", "4", 3);
        private static readonly IChromosome Chr17 = new Chromosome("chr17", "17", 16);
        private static readonly IChromosome ChrX = new Chromosome("chrX", "X", 22);

        
        public DbSnpReaderTests()
        {
            _refChromDict = new Dictionary<string, IChromosome>
            {
                {"1",Chr1 },
                {"2",Chr2 },
                {"4",Chr4 },
                {"17",Chr17},
                {"X", ChrX}
            };
        }

        [Fact]
        public void MissingEntry()
        {
            const string vcfLine =
                "1	241369	rs11490246	C	T	.	.	RS=11490246;RSPOS=241369;dbSNPBuildID=120;SSR=0;SAO=0;VP=0x050000000005000126000100;WGT=1;VC=SNV;ASP;GNO;KGPhase3;CAF=0,1;COMMON=0";

            var sequenceProvider = ParserTestUtils.GetSequenceProvider(241369, "C", 'A', _refChromDict);
            var dbsnpReader = new DbSnpReader(null, sequenceProvider);
            var dbSnpEntry = dbsnpReader.ExtractItem(vcfLine).First();

            Assert.Equal(11490246, dbSnpEntry.RsId);
        }

        [Fact]
        public void MissingEntry2()
        {
            const string vcfLine =
                "17	828	rs62053745	T	C	.	.	RS=62053745;RSPOS=828;dbSNPBuildID=129;SSR=0;SAO=0;VP=0x050100080005140136000100;WGT=1;VC=SNV;SLO;INT;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=0.2576,0.7424;COMMON=1";

            var sequenceProvider = ParserTestUtils.GetSequenceProvider(828, "T", 'A', _refChromDict);
            var dbsnpReader = new DbSnpReader(null, sequenceProvider);
            var dbSnpEntry = dbsnpReader.ExtractItem(vcfLine).First();

            Assert.Equal(62053745, dbSnpEntry.RsId);
        }

        [Fact]
        public void MissingDbsnpId()
        {
            const string vcfLine =
                "X	21505833	rs12395602	G	A,C,T	.	.	RS=12395602;RSPOS=21505833;dbSNPBuildID=120;SSR=0;SAO=0;VP=0x05010008000505051f000101;WGT=1;VC=SNV;SLO;INT;ASP;VLD;G5;HD;GNO;KGPhase1";

            var sequenceProvider = ParserTestUtils.GetSequenceProvider(21505833, "G", 'G', _refChromDict);
            var dbsnpReader = new DbSnpReader(null, sequenceProvider);

            var dbSnpEntries = dbsnpReader.ExtractItem(vcfLine).ToList();

            Assert.Equal(3, dbSnpEntries.Count);
            Assert.Equal("A", dbSnpEntries[0].AltAllele);
            Assert.Equal(12395602, dbSnpEntries[0].RsId);
            Assert.Equal("C", dbSnpEntries[1].AltAllele);
            Assert.Equal(12395602, dbSnpEntries[1].RsId);
            Assert.Equal("T", dbSnpEntries[2].AltAllele);
            Assert.Equal(12395602, dbSnpEntries[2].RsId);
        }

        [Obsolete("We should not have skipped unit tests.")]
        [Fact(Skip = "redo test with AlleleFrequency object")]
        public void NoMinorAllele()
        {
            const string vcfLine = "17	828	rs62053745	T	C	.	.	RS=62053745;RSPOS=828;dbSNPBuildID=129;SSR=0;SAO=0;VP=0x050100080005140136000100;WGT=1;VC=SNV;SLO;INT;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=.,0.7424;COMMON=1";
            var sequenceProvider = ParserTestUtils.GetSequenceProvider(828, "T", 'G', _refChromDict);
            var dbsnpReader      = new DbSnpReader(null, sequenceProvider);
            var dbSnpEntry       = dbsnpReader.ExtractItem(vcfLine).First();

            Assert.Equal("C", dbSnpEntry.AltAllele);            
        }

        [Obsolete("We should not have skipped unit tests.")]
        [Fact(Skip = "redo test with AlleleFrequency object")]
        public void DisregardZeroFreq()
        {
            const string vcfLine = "1	241369	rs11490246	C	T	.	.	RS=11490246;RSPOS=241369;dbSNPBuildID=120;SSR=0;SAO=0;VP=0x050100000005000126000100;WGT=1;VC=SNV;SLO;ASP;GNO;KGPhase3;CAF=0,1;COMMON=0";
            var sequenceProvider = ParserTestUtils.GetSequenceProvider(241369, "C", 'G', _refChromDict);
            var dbsnpReader      = new DbSnpReader(null, sequenceProvider);
            var dbSnpEntry       = dbsnpReader.ExtractItem(vcfLine).First();

            Assert.Equal("T", dbSnpEntry.AltAllele);            
        }

        private static Stream GetStream()
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
            var sequence = new SimpleSequence(new string('A', VariantUtils.MaxUpstreamLength) + "T" + new string('G', 10329 - 10285) + "AC", 10284 - VariantUtils.MaxUpstreamLength);

            var sequenceProvider = new SimpleSequenceProvider(GenomeAssembly.GRCh37, sequence, _refChromDict);

            var reader = new DbSnpReader(GetStream(), sequenceProvider);

            var items = reader.GetItems().ToList();

            Assert.Equal(3, items.Count);
            Assert.Equal("\"rs866375379\"", items[0].GetJsonString());
        }
    }
}
