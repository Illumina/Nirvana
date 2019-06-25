using System.Collections.Generic;
using System.IO;
using System.Linq;
using CacheUtils.TranscriptCache;
using Genome;
using Intervals;
using Moq;
using SAUtils.SpliceAi;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO.Caches;
using Xunit;

namespace UnitTests.SAUtils.SpliceAi
{
    public sealed class SpliceAiTests
    {
        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##fileformat=VCFv4.0");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            //this line should not produce any objects since all values are <0.10 and its far from splice sites
            writer.WriteLine("10\t92900\t.\tC\tT\t.\t.\tSYMBOL=TUBB8;STRAND=-;TYPE=E;DIST=-53;DS_AG=0.0000;DS_AL=0.0000;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-26;DP_AL=-10;DP_DG=3;DP_DL=35");
            // values are small but it is close to a splice site. So we report all of it
            writer.WriteLine("10\t92946\t.\tC\tT\t.\t.\tSYMBOL=TUBB8;STRAND=-;TYPE=E;DIST=-53;DS_AG=0.0000;DS_AL=0.0000;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-26;DP_AL=-10;DP_DG=3;DP_DL=35");
            // not around a splice site but has higher than 0.1 value. So, we report the one that is significant 
            writer.WriteLine("10\t93389\t.\tC\tA\t.\t.\tSYMBOL=TUBB8;STRAND=-;TYPE=E;DIST=-496;DS_AG=0.1062;DS_AL=0.0000;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-11;DP_AL=-29;DP_DG=-11;DP_DL=-32");
            //should be reported back with 4 object since it is within splice interval;
            writer.WriteLine("10\t93816\t.\tC\tG\t.\t.\tSYMBOL=TUBB8;STRAND=-;TYPE=E;DIST=238;DS_AG=0.1909;DS_AL=0.0000;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-7;DP_AL=-50;DP_DG=-7;DP_DL=-6");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        private static IChromosome _chr10 = new Chromosome("chr10", "10", 9);
        private static IChromosome _chr3 = new Chromosome("chr3", "3", 2);
        private static ISequenceProvider GetSequenceProvider()
        {
            var refNameToChrom = new Dictionary<string, IChromosome>()
            {
                {"3", _chr3},
                {"10", _chr10}
            };
            var refIndexToChrom = new Dictionary<ushort, IChromosome>()
            {
                { _chr3.Index, _chr3},
                { _chr10.Index, _chr10} 
            };

            var mockProvider = new Mock<ISequenceProvider>();
            mockProvider.SetupGet(x => x.RefNameToChromosome).Returns(refNameToChrom);
            mockProvider.SetupGet(x => x.RefIndexToChromosome).Returns(refIndexToChrom);
            return mockProvider.Object;
        }

        private static ISequenceProvider GetCacheSequenceProvider()
        {
            var refNameToChrom = new Dictionary<string, IChromosome>()
            {
                {"3", _chr3}
            };
            var refIndexToChrom = new Dictionary<ushort, IChromosome>()
            {
                { _chr3.Index, _chr3}
            };

            var mockProvider = new Mock<ISequenceProvider>();
            mockProvider.SetupGet(x => x.RefNameToChromosome).Returns(refNameToChrom);
            mockProvider.SetupGet(x => x.RefIndexToChromosome).Returns(refIndexToChrom);
            return mockProvider.Object;
        }

        private static Dictionary<ushort, IntervalArray<byte>> GetSpliceIntervals()
        {
            var intervals = new[]
            {
                new Interval<byte>(92946 - SpliceUtilities.SpliceFlankLength, 92946 + SpliceUtilities.SpliceFlankLength, 0),
                new Interval<byte>(93816 - SpliceUtilities.SpliceFlankLength, 93816 + SpliceUtilities.SpliceFlankLength, 0),
            };

            return new Dictionary<ushort, IntervalArray<byte>>()
            {
                {_chr10.Index, new IntervalArray<byte>(intervals)}
            };  
        }

        [Fact]
        public void Parse_standard_lines()
        {
            using (var spliceParser = new SpliceAiParser(GetStream(), GetSequenceProvider(), GetSpliceIntervals()))
            {
                var spliceItems = spliceParser.GetItems().ToList();

                Assert.Equal(3,spliceItems.Count);
                Assert.Equal("\"type\":\"acceptor gain\",\"distance\":-26,\"score\":0},{\"type\":\"acceptor loss\",\"distance\":-10,\"score\":0},{\"type\":\"donor gain\",\"distance\":3,\"score\":0},{\"type\":\"donor loss\",\"distance\":35,\"score\":0", spliceItems[0].GetJsonString());
                Assert.Equal("\"type\":\"acceptor gain\",\"distance\":-11,\"score\":0.1", spliceItems[1].GetJsonString());
                Assert.Equal("\"type\":\"acceptor gain\",\"distance\":-7,\"score\":0.2},{\"type\":\"acceptor loss\",\"distance\":-50,\"score\":0},{\"type\":\"donor gain\",\"distance\":-7,\"score\":0},{\"type\":\"donor loss\",\"distance\":-6,\"score\":0", spliceItems[2].GetJsonString());
            }
        }

        private static Stream GetMultiScoreStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##fileformat=VCFv4.0");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            //should be reported back with three objects (manually modified)
            writer.WriteLine("10\t93816\t.\tC\tG\t.\t.\tSYMBOL=TUBB8;STRAND=-;TYPE=E;DIST=238;DS_AG=0.1909;DS_AL=0.3760;DS_DG=0.0000;DS_DL=0.2480;DP_AG=-7;DP_AL=-50;DP_DG=-7;DP_DL=-6");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }


        [Fact]
        public void Parse_multiScore_entry()
        {
            using (var spliceParser = new SpliceAiParser(GetMultiScoreStream(), GetSequenceProvider(), GetSpliceIntervals()))
            {
                var spliceItems = spliceParser.GetItems().ToList();

                Assert.Single(spliceItems);
                Assert.Equal("\"type\":\"acceptor gain\",\"distance\":-7,\"score\":0.2},{\"type\":\"acceptor loss\",\"distance\":-50,\"score\":0.4},{\"type\":\"donor gain\",\"distance\":-7,\"score\":0},{\"type\":\"donor loss\",\"distance\":-6,\"score\":0.2", spliceItems[0].GetJsonString());
            }
        }

        private Stream GetCacheStream()
        {
            const GenomeAssembly genomeAssembly = GenomeAssembly.GRCh38;

            var baseHeader = new Header("test", 2, 3, Source.BothRefSeqAndEnsembl, 4, genomeAssembly);
            var customHeader = new TranscriptCacheCustomHeader(1, 2);
            var expectedHeader = new CacheHeader(baseHeader, customHeader);

            var transcriptRegions = new ITranscriptRegion[]
            {
                new TranscriptRegion(TranscriptRegionType.Exon, 1, 100, 199, 300, 399),
                new TranscriptRegion(TranscriptRegionType.Intron, 1, 200, 299, 399, 400),
                new TranscriptRegion(TranscriptRegionType.Exon, 2, 300, 399, 400, 499)
            };

            var mirnas = new IInterval[2];
            mirnas[0] = new Interval(100, 200);
            mirnas[1] = new Interval(300, 400);

            var peptideSeqs = new[] { "MASE*" };

            var genes = new IGene[1];
            genes[0] = new Gene(_chr3, 100, 200, true, "TP53", 300, CompactId.Convert("7157"),
                CompactId.Convert("ENSG00000141510"));

            var regulatoryRegions = new IRegulatoryRegion[2];
            regulatoryRegions[0] = new RegulatoryRegion(_chr3, 1200, 1300, CompactId.Convert("123"), RegulatoryRegionType.enhancer);
            regulatoryRegions[1] = new RegulatoryRegion(_chr3, 1250, 1450, CompactId.Convert("456"), RegulatoryRegionType.enhancer);
            var regulatoryRegionIntervalArrays = regulatoryRegions.ToIntervalArrays(3);

            var transcripts = GetTranscripts(_chr3, genes, transcriptRegions, mirnas);
            var transcriptIntervalArrays = transcripts.ToIntervalArrays(3);

            var expectedCacheData = new TranscriptCacheData(expectedHeader, genes, transcriptRegions, mirnas, peptideSeqs,
                transcriptIntervalArrays, regulatoryRegionIntervalArrays);

            var ms = new MemoryStream();
            using (var writer = new TranscriptCacheWriter(ms, expectedHeader, true))
            {
                writer.Write(expectedCacheData);
            }

            ms.Position = 0;

            return ms;
        }

        private static ITranscript[] GetTranscripts(IChromosome chromosome, IGene[] genes, ITranscriptRegion[] regions,
            IInterval[] mirnas)
        {
            return new ITranscript[]
            {
                new Transcript(chromosome, 120, 180, CompactId.Convert("789"), null, BioType.IG_D_gene, genes[0], 0, 0,
                    false, regions, 0, mirnas, -1, -1, Source.None, false, false, null, null)
            };
        }

        [Fact]
        public void GetSpliceIntervals_standard()
        {
            var spliceIntervals = SpliceUtilities.GetSpliceIntervals(GetCacheSequenceProvider(), GetCacheStream());

            Assert.Single(spliceIntervals);
            //given 2 exons, there should be 4 splice intervals
            Assert.Equal(4, spliceIntervals[2].Array.Length);
        }
    }
}