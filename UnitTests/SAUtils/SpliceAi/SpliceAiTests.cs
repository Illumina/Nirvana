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
        private IntervalForest<string> GetGeneForest()
        {
            var intervalArrays = new IntervalArray<string>[25];// 1-22, X,Y,MT
            // creating dummy interval trees for all the chromosomes
            for (var i = 0; i < intervalArrays.Length; i++)
            {
                intervalArrays[i] = new IntervalArray<string>(new[]
                {
                    new Interval<string>(1, int.MaxValue, "chr"+i),
                });
            }

            var chrom1Array = new IntervalArray<string>(new[]
            {
                new Interval<string>(1570603,1590558, "CDK11B"),
                new Interval<string>(1567060,1570639, "MMP23B"),
            });
            var chrom10Array = new IntervalArray<string>(new[]
            {
                new Interval<string>(92828,95178, "TUBB8"),
            });

            var chrom21Array = new IntervalArray<string>(new[]
            {
                new Interval<string>(31863782,30491464, "KRTAP19-3"),
                new Interval<string>(31859362,31859755, "KRTAP19-2"),
            });

            intervalArrays[0] = chrom1Array;
            intervalArrays[9] = chrom10Array;
            intervalArrays[20] = chrom21Array;
            return new IntervalForest<string>(intervalArrays);
        }

        private Dictionary<string, List<string>> GetGeneSymbolSynonyms()
        {
            return new Dictionary<string, List<string>>()
            {
                { "TUBB8", new List<string>(){"TUBB8"}},
                { "CDK11B", new List<string>(){"CDK11B"}},
                { "MMP23B", new List<string>(){"MMP23B"}},
                { "KRTAP19-3", new List<string>(){"KRTAP19-3"}},
                { "KRTAP19-2", new List<string>(){"KRTAP19-2"}},
            };
        }
        private static readonly IChromosome Chr10 = new Chromosome("chr10", "10", 9);
        private static readonly IChromosome Chr3 = new Chromosome("chr3", "3", 2);
        private static readonly IChromosome Chr1 = new Chromosome("chr1", "1", 0);
        private static readonly IChromosome Chr21 = new Chromosome("chr21", "21", 20);
        private static ISequenceProvider GetSequenceProvider()
        {
            var refNameToChrom = new Dictionary<string, IChromosome>()
            {
                {"1", Chr1 },
                {"3", Chr3},
                {"10", Chr10},
                {"21", Chr21},
            };
            var refIndexToChrom = new Dictionary<ushort, IChromosome>()
            {
                { Chr1.Index, Chr1},
                { Chr3.Index, Chr3},
                { Chr10.Index, Chr10} ,
                { Chr21.Index, Chr21},
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
                {"3", Chr3}
            };
            var refIndexToChrom = new Dictionary<ushort, IChromosome>()
            {
                { Chr3.Index, Chr3}
            };

            var mockProvider = new Mock<ISequenceProvider>();
            mockProvider.SetupGet(x => x.RefNameToChromosome).Returns(refNameToChrom);
            mockProvider.SetupGet(x => x.RefIndexToChromosome).Returns(refIndexToChrom);
            return mockProvider.Object;
        }

        private static Dictionary<ushort, IntervalArray<byte>> GetSpliceIntervals()
        {
            var intervals10 = new[]
            {
                new Interval<byte>(92946 - SpliceUtilities.SpliceFlankLength, 92946 + SpliceUtilities.SpliceFlankLength, 0),
                new Interval<byte>(93816 - SpliceUtilities.SpliceFlankLength, 93816 + SpliceUtilities.SpliceFlankLength, 0),
            };

            var intervals1 = new[]
            {
                new Interval<byte>(1577180 - SpliceUtilities.SpliceFlankLength, 1577180 + SpliceUtilities.SpliceFlankLength, 0)
            };

            var intervals21 = new[]
            {
                new Interval<byte>(31859677 - SpliceUtilities.SpliceFlankLength, 31859677 + SpliceUtilities.SpliceFlankLength, 0),
                new Interval<byte>(35275955 - SpliceUtilities.SpliceFlankLength, 35275955 + SpliceUtilities.SpliceFlankLength, 0),

            };

            return new Dictionary<ushort, IntervalArray<byte>>()
            {
                { Chr1.Index, new IntervalArray<byte>(intervals1) },
                {Chr10.Index, new IntervalArray<byte>(intervals10)},
                {Chr21.Index, new IntervalArray<byte>(intervals21)},
            };  
        }
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

        private static Stream GetPositionCachingStream()
        {
            //testing the position caching using minHeap. All entries have significant entries, so all of them should be reported
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##fileformat=VCFv4.0");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("10\t92900\t.\tC\tT\t.\t.\tSYMBOL=TUBB8;STRAND=-;TYPE=E;DIST=-53;DS_AG=0.3200;DS_AL=0.1500;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-26;DP_AL=-10;DP_DG=3;DP_DL=35");
            writer.WriteLine("10\t92946\t.\tC\tT\t.\t.\tSYMBOL=TUBB8;STRAND=-;TYPE=E;DIST=-53;DS_AG=0.0000;DS_AL=0.0000;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-26;DP_AL=-10;DP_DG=3;DP_DL=35");
            writer.WriteLine("10\t92946\t.\tC\tA\t.\t.\tSYMBOL=TUBB8;STRAND=-;TYPE=E;DIST=-53;DS_AG=0.2000;DS_AL=0.0030;DS_DG=0.2120;DS_DL=0.0000;DP_AG=-26;DP_AL=-10;DP_DG=3;DP_DL=35");
            writer.WriteLine("10\t93389\t.\tC\tA\t.\t.\tSYMBOL=TUBB8;STRAND=-;TYPE=E;DIST=-496;DS_AG=0.1062;DS_AL=0.0000;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-11;DP_AL=-29;DP_DG=-11;DP_DL=-32");
            writer.WriteLine("10\t93816\t.\tC\tG\t.\t.\tSYMBOL=TUBB8;STRAND=-;TYPE=E;DIST=238;DS_AG=0.1909;DS_AL=0.0000;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-7;DP_AL=-50;DP_DG=-7;DP_DL=-6");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        private static Stream GetMultiChromosomeStream()
        {
            //testing the position caching using minHeap. All entries have significant entries, so all of them should be reported
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##fileformat=VCFv4.0");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            //having two gene symbols at the same position should avoid updating gene symbol
            writer.WriteLine("10\t92900\t.\tC\tT\t.\t.\tSYMBOL=GENE1;STRAND=-;TYPE=E;DIST=-53;DS_AG=0.3200;DS_AL=0.1500;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-26;DP_AL=-10;DP_DG=3;DP_DL=35");
            writer.WriteLine("10\t92900\t.\tC\tT\t.\t.\tSYMBOL=GENE8;STRAND=-;TYPE=E;DIST=-53;DS_AG=0.3200;DS_AL=0.1500;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-26;DP_AL=-10;DP_DG=3;DP_DL=35");
            //The previous entries should be flushed since we changed chromosome
            writer.WriteLine("1\t92900\t.\tC\tT\t.\t.\tSYMBOL=TUBB8;STRAND=-;TYPE=E;DIST=-53;DS_AG=0.3200;DS_AL=0.1500;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-26;DP_AL=-10;DP_DG=3;DP_DL=35");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }
        private static Stream GetMultiScoreStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##fileformat=VCFv4.0");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("10\t93816\t.\tC\tG\t.\t.\tSYMBOL=TUBB8;STRAND=-;TYPE=E;DIST=238;DS_AG=0.1909;DS_AL=0.3760;DS_DG=0.0000;DS_DL=0.2480;DP_AG=-7;DP_AL=-50;DP_DG=-7;DP_DL=-6");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        private static Stream GetMultiGeneStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##fileformat=VCFv4.0");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("1\t1577180\t.\tC\tT\t.\t.\tSYMBOL=MMP23B;STRAND=+;TYPE=I;DIST=-7889;DS_AG=0.0002;DS_AL=0.0000;DS_DG=0.0000;DS_DL=0.0000;DP_AG=8;DP_AL=-16;DP_DG=-16;DP_DL=26");
            writer.WriteLine("1\t1577180\t.\tC\tT\t.\t.\tSYMBOL=CDK11B;STRAND=-;TYPE=E;DIST=1;DS_AG=0.9244;DS_AL=0.0000;DS_DG=0.0004;DS_DL=0.0000;DP_AG=-2;DP_AL=-8;DP_DG=33;DP_DL=-13");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        private static Stream GetMissingEntryStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##fileformat=VCFv4.0");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("21\t35275955\t.\tG\tA\t.\t.\tSYMBOL=AP000304.12;STRAND=-;TYPE=I;DIST=3690;DS_AG=0.1441;DS_AL=0.0000;DS_DG=0.0003;DS_DL=0.0000;DP_AG=-12;DP_AL=24;DP_DG=-41;DP_DL=5");
            writer.WriteLine("21\t35275955\t.\tG\tA\t.\t.\tSYMBOL=ATP5O;STRAND=-;TYPE=I;DIST=-12;DS_AG=0.0000;DS_AL=0.0000;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-12;DP_AL=24;DP_DG=-41;DP_DL=-12");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }
        private static Stream GetMultiGeneAtSameLocationStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##fileformat=VCFv4.0");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("21\t31859677\t.\tG\tA\t.\t.\tSYMBOL=KRTAP19-3;STRAND=-;TYPE=I;DIST=4472;DS_AG=0.0000;DS_AL=0.0000;DS_DG=0.0000;DS_DL=0.0000;DP_AG=-42;DP_AL=38;DP_DG=23;DP_DL=38");
            writer.WriteLine("21\t31859677\t.\tG\tA\t.\t.\tSYMBOL=KRTAP19-2;STRAND=-;TYPE=E;DIST=78;DS_AG=0.0061;DS_AL=0.0000;DS_DG=0.0262;DS_DL=0.0000;DP_AG=-42;DP_AL=38;DP_DG=23;DP_DL=-11");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void Check_multi_chromosome_gene_update()
        {
            using (var spliceParser = new SpliceAiParser(GetMultiChromosomeStream(), GetSequenceProvider(), GetSpliceIntervals(), GetGeneForest(),GetGeneSymbolSynonyms()))
            {
                var spliceItems = spliceParser.GetItems().ToList();

                Assert.Equal(3, spliceItems.Count);
                Assert.Equal("GENE1", spliceItems[0].Hgnc);
                Assert.Equal("GENE8", spliceItems[1].Hgnc);
                Assert.Null(spliceItems[2].Hgnc);

            }
        }
        [Fact]
        public void Parse_standard_lines()
        {
            using (var spliceParser = new SpliceAiParser(GetStream(), GetSequenceProvider(), GetSpliceIntervals()))
            {
                var spliceItems = spliceParser.GetItems().ToList();

                Assert.Equal(3,spliceItems.Count);
                Assert.Equal("\"hgnc\":\"TUBB8\",\"acceptorGainScore\":0,\"acceptorGainDistance\":-26,\"acceptorLossScore\":0,\"acceptorLossDistance\":-10,\"donorGainScore\":0,\"donorGainDistance\":3,\"donorLossScore\":0,\"donorLossDistance\":35", spliceItems[0].GetJsonString());
                Assert.Equal("\"hgnc\":\"TUBB8\",\"acceptorGainScore\":0.1,\"acceptorGainDistance\":-11", spliceItems[1].GetJsonString());
                Assert.Equal("\"hgnc\":\"TUBB8\",\"acceptorGainScore\":0.2,\"acceptorGainDistance\":-7,\"acceptorLossScore\":0,\"acceptorLossDistance\":-50,\"donorGainScore\":0,\"donorGainDistance\":-7,\"donorLossScore\":0,\"donorLossDistance\":-6", spliceItems[2].GetJsonString());
            }
        }

        [Fact]
        public void MissingEntry()
        {
            using (var spliceParser = new SpliceAiParser(GetMissingEntryStream(), GetSequenceProvider(), GetSpliceIntervals()))
            {
                var spliceItems = spliceParser.GetItems().ToList();

                Assert.Equal(2,spliceItems.Count);
                Assert.Equal("\"hgnc\":\"AP000304.12\",\"acceptorGainScore\":0.1,\"acceptorGainDistance\":-12,\"acceptorLossScore\":0,\"acceptorLossDistance\":24,\"donorGainScore\":0,\"donorGainDistance\":-41,\"donorLossScore\":0,\"donorLossDistance\":5", spliceItems[0].GetJsonString());
            }
        }

        [Fact]
        public void Parse_multiScore_entry()
        {
            using (var spliceParser = new SpliceAiParser(GetMultiScoreStream(), GetSequenceProvider(), GetSpliceIntervals()))
            {
                var spliceItems = spliceParser.GetItems().ToList();

                Assert.Single(spliceItems);
                Assert.Equal("\"hgnc\":\"TUBB8\",\"acceptorGainScore\":0.2,\"acceptorGainDistance\":-7,\"acceptorLossScore\":0.4,\"acceptorLossDistance\":-50,\"donorGainScore\":0,\"donorGainDistance\":-7,\"donorLossScore\":0.2,\"donorLossDistance\":-6", spliceItems[0].GetJsonString());
            }
        }

        [Fact]
        public void Parse_multiGene_entry()
        {
            using (var spliceParser = new SpliceAiParser(GetMultiGeneStream(), GetSequenceProvider(), GetSpliceIntervals()))
            {
                var spliceItems = spliceParser.GetItems().ToList();

                Assert.Equal(2,spliceItems.Count);
                Assert.Equal("\"hgnc\":\"MMP23B\",\"acceptorGainScore\":0,\"acceptorGainDistance\":8,\"acceptorLossScore\":0,\"acceptorLossDistance\":-16,\"donorGainScore\":0,\"donorGainDistance\":-16,\"donorLossScore\":0,\"donorLossDistance\":26", spliceItems[0].GetJsonString());
                Assert.Equal("\"hgnc\":\"CDK11B\",\"acceptorGainScore\":0.9,\"acceptorGainDistance\":-2,\"acceptorLossScore\":0,\"acceptorLossDistance\":-8,\"donorGainScore\":0,\"donorGainDistance\":33,\"donorLossScore\":0,\"donorLossDistance\":-13", spliceItems[1].GetJsonString());
            }
        }

        [Fact]
        public void Check_position_caching()
        {
            using (var spliceParser = new SpliceAiParser(GetPositionCachingStream(), GetSequenceProvider(), GetSpliceIntervals()))
            {
                var spliceItems = spliceParser.GetItems().ToList();

                Assert.Equal(5, spliceItems.Count);
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
            genes[0] = new Gene(Chr3, 100, 200, true, "TP53", 300, CompactId.Convert("7157"),
                CompactId.Convert("ENSG00000141510"));

            var regulatoryRegions = new IRegulatoryRegion[2];
            regulatoryRegions[0] = new RegulatoryRegion(Chr3, 1200, 1300, CompactId.Convert("123"), RegulatoryRegionType.enhancer);
            regulatoryRegions[1] = new RegulatoryRegion(Chr3, 1250, 1450, CompactId.Convert("456"), RegulatoryRegionType.enhancer);
            var regulatoryRegionIntervalArrays = regulatoryRegions.ToIntervalArrays(3);

            var transcripts = GetTranscripts(Chr3, genes, transcriptRegions, mirnas);
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
            using (var transcriptCacheReader = new TranscriptCacheReader(GetCacheStream()))
            {
                var seqProvider     = GetCacheSequenceProvider();
                var transcriptData  = transcriptCacheReader.Read(seqProvider.RefIndexToChromosome);
                var spliceIntervals = SpliceUtilities.GetSpliceIntervals(seqProvider, transcriptData);

                Assert.Single(spliceIntervals);
                //given 2 exons, there should be 4 splice intervals
                Assert.Equal(4, spliceIntervals[2].Array.Length);
            }
        }

        
        [Fact]
        public void Two_symbols_in_spliceAi()
        {
            using (var spliceParser = new SpliceAiParser(GetMultiGeneAtSameLocationStream(), GetSequenceProvider(), GetSpliceIntervals(), GetGeneForest(), GetGeneSymbolSynonyms()))
            {
                var spliceItems = spliceParser.GetItems().ToList();

                Assert.Equal(2, spliceItems.Count);
                Assert.Equal("\"hgnc\":\"KRTAP19-3\",\"acceptorGainScore\":0,\"acceptorGainDistance\":-42,\"acceptorLossScore\":0,\"acceptorLossDistance\":38,\"donorGainScore\":0,\"donorGainDistance\":23,\"donorLossScore\":0,\"donorLossDistance\":38", spliceItems[0].GetJsonString());
                Assert.Equal("\"hgnc\":\"KRTAP19-2\",\"acceptorGainScore\":0,\"acceptorGainDistance\":-42,\"acceptorLossScore\":0,\"acceptorLossDistance\":38,\"donorGainScore\":0,\"donorGainDistance\":23,\"donorLossScore\":0,\"donorLossDistance\":-11", spliceItems[1].GetJsonString());
            }
        }
    }
}