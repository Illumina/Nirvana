using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Jasix;
using Jasix.DataStructures;
using Xunit;
using Compression.FileHandling;
using UnitTests.TestUtilities;

namespace UnitTests.Jasix
{
    public sealed class JasixQueryProcessingTests
    {
        [Fact]
        public void Combination_of_large_and_small_variants()
        {
            var index = new JasixIndex();

            //query range 10,000- 10,020
            index.Add("chr1", 8_000, 9_900, 90_000);//SV not overlapping the query
            index.Add("chr1", 9_000, 10_005, 90_100);// partially overlapping
            index.Add("chr1", 9_500, 10_050, 90_200);//completely overlapping
            index.Add("chr1", 10_000, 10_001, 100_000);
            index.Add("chr1", 10_004, 10_006, 100_100);
            index.Add("chr1", 10_009, 10_550, 100_200);//SV starting from the middle of the range
            index.Add("chr1", 10_008, 10_010, 100_300);
            index.Add("chr1", 10_011, 10_020, 100_400);
            index.Add("chr1", 10_039, 10_550, 100_200);//SV past the range

            index.Flush();

            var firstSmallVarLocation = index.GetFirstVariantPosition("chr1", 10_000, 10_020);
            var largeVariantLocations = index.LargeVariantPositions("chr1", 10_000, 10_020);

            Assert.Equal(90_000, firstSmallVarLocation);
            Assert.True(largeVariantLocations.SequenceEqual(new List<long> { 90_100, 90_200, 100_200 }));
        }

        [Fact]
        public void Quiring_large_variants_overlapping_range_but_starting_before()
        {
            var index = new JasixIndex();

            //query range 10,000- 10,020
            index.Add("chr1", 8_000, 10_000, 80_000);//SV ending at the start of query
            index.Add("chr1", 8_000, 9_900, 90_000);//SV not overlapping the query
            index.Add("chr1", 9_000, 10_005, 90_100);// partially overlapping
            index.Add("chr1", 9_500, 10_050, 90_200);//completely overlapping
            index.Add("chr1", 10_000, 10_001, 100_000);
            index.Add("chr1", 10_000, 10_701, 100_050);//starting at the begin of query
            index.Add("chr1", 10_004, 10_006, 100_100);
            index.Add("chr1", 10_009, 10_550, 100_200);//SV starting from the middle of the range
            index.Add("chr1", 10_008, 10_010, 100_300);
            index.Add("chr1", 10_011, 10_020, 100_400);
            index.Add("chr1", 10_039, 10_550, 100_200);//SV past the range

            index.Flush();

            var largeVariantBefore = index.LargeVariantPositions("chr1", 10_000, 9_999);

            Assert.True(largeVariantBefore.SequenceEqual(new List<long> { 80_000, 90_100, 90_200 }));
        }

        [Fact]
        public void First_variant_position_when_the_first_variant_is_large()
        {
            var index = new JasixIndex();

            //query range 10,000- 10,020
            index.Add("chr1", 10_000, 10_701, 100_050);//SV at the begin of query
            index.Add("chr1", 10_004, 10_006, 100_100);
            index.Add("chr1", 10_009, 10_550, 100_200);//SV starting from the middle of the range
            index.Add("chr1", 10_008, 10_010, 100_300);
            index.Add("chr1", 10_011, 10_020, 100_400);
            index.Add("chr1", 10_039, 10_550, 100_200);//SV past the range

            index.Flush();

            var firstVariantLocation = index.GetFirstVariantPosition("chr1", 10_000, 10_010);

            Assert.Equal(100_050, firstVariantLocation);
        }

        [Fact]
        public void TestQuerySingle()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz.jsi"));

            using (var qp = new QueryProcessor(new StreamReader(readStream), indexStream))
            {
                var header = qp.GetHeader();
                Assert.Equal("\"header\":{\"annotator\":\"Illumina Annotation Engine 1.3.3.1633\",\"creationTime\":\"2016-12-09 09:49:24\",\"genomeAssembly\":\"GRCh37\",\"schemaVersion\":4,\"dataVersion\":\"84.22.36\",\"dataSources\":[{\"name\":\"VEP\",\"version\":\"84\",\"description\":\"Ensembl\",\"releaseDate\":\"2016-04-29\"},{\"name\":\"phyloP\",\"version\":\"hg19\",\"description\":\"46 way conservation score between humans and 45 other vertebrates\",\"releaseDate\":\"2009-11-10\"},{\"name\":\"OMIM\",\"version\":\"unknown\",\"description\":\"An Online Catalog of Human Genes and Genetic Disorders\",\"releaseDate\":\"2016-09-02\"},{\"name\":\"dbSNP\",\"version\":\"147\",\"description\":\"Identifiers for observed variants\",\"releaseDate\":\"2016-06-01\"},{\"name\":\"COSMIC\",\"version\":\"78\",\"description\":\"Somatic mutation and related details and information relating to human cancers\",\"releaseDate\":\"2016-09-05\"},{\"name\":\"1000 Genomes Project\",\"version\":\"Phase 3 v5a\",\"description\":\"A public catalogue of human variation and genotype data\",\"releaseDate\":\"2013-05-27\"},{\"name\":\"EVS\",\"version\":\"2\",\"releaseDate\":\"2013-11-13\"},{\"name\":\"ExAC\",\"version\":\"0.3.1\",\"description\":\"Allele frequency data from the ExAC project\",\"releaseDate\":\"2016-03-16\"},{\"name\":\"ClinVar\",\"version\":\"unknown\",\"description\":\"A freely accessible, public archive of reports of the relationships among human variations and phenotypes, with supporting evidence\",\"releaseDate\":\"2016-09-01\"},{\"name\":\"DGV\",\"version\":\"unknown\",\"description\":\"Provides a comprehensive summary of structural variation in the human genome\",\"releaseDate\":\"2016-05-15\"},{\"name\":\"ClinGen\",\"version\":\"unknown\",\"releaseDate\":\"2016-04-14\"}]}", header);

                var results =
                    qp.ReadOverlappingJsonLines(Utilities.ParseQuery("chr1:9775924"));
                Assert.Single(results);
            }
        }

        [Fact]
        public void TestQueryMultiple()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz.jsi"));

            using (var qp = new QueryProcessor(new StreamReader(readStream), indexStream))
            {
                var results =
                    qp.ReadOverlappingJsonLines(Utilities.ParseQuery("chr1:9775924-9778952"));
                Assert.Equal(3, results.Count());

            }
        }

        [Fact]
        public void TestQueryMultipleWithSkippingMiddleOne()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz.jsi"));

            using (var qp = new QueryProcessor(new StreamReader(readStream), indexStream))
            {
                var results =
                    qp.ReadOverlappingJsonLines(Utilities.ParseQuery("chr1:27023180-27023190"));
                Assert.Equal(2, results.Count());
            }
        }

        [Fact]
        public void TestQueryChr1()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz.jsi"));

            using (var qp = new QueryProcessor(new StreamReader(readStream), indexStream))
            {
                var results =
                    qp.ReadOverlappingJsonLines(Utilities.ParseQuery("chr1"));

                Assert.Equal(422, results.Count());
            }
        }

        [Fact]
        public void Report_overlapping_small_and_extending_large_variants()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("JasixTest.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("JasixTest.json.gz.jsi"));

            using (var qp = new QueryProcessor(new StreamReader(readStream), indexStream))
            {
                var results =
                    qp.ReadOverlappingJsonLines(Utilities.ParseQuery("chr1:16378-17000"));

                Assert.Equal(3, results.Count());

                results =
                    qp.ReadJsonLinesExtendingInto(Utilities.ParseQuery("chr1:16378-17000"));

                Assert.Single(results);
            }
        }

        [Fact]
        public void Report_overlapping_small_and_extending_multiple_large_variants()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("JasixTest.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("JasixTest.json.gz.jsi"));

            using (var qp = new QueryProcessor(new StreamReader(readStream), indexStream))
            {
                var results =
                    qp.ReadOverlappingJsonLines(Utilities.ParseQuery("chr1:19004-20000"));

                Assert.Equal(3, results.Count());

                results =
                    qp.ReadJsonLinesExtendingInto(Utilities.ParseQuery("chr1:19004-20000"));

                Assert.Equal(2, results.Count());
            }
        }

        [Fact]
        public void Report_overlapping_small_and_large_variants_starting_at_same_location()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("JasixTest.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("JasixTest.json.gz.jsi"));

            using (var qp = new QueryProcessor(new StreamReader(readStream), indexStream))
            {
                var results =
                    qp.ReadOverlappingJsonLines(Utilities.ParseQuery("chr1:46993-50000"));

                Assert.Equal(5, results.Count());

                results =
                    qp.ReadJsonLinesExtendingInto(Utilities.ParseQuery("chr1:46993-50000"));

                Assert.Empty(results);
            }
        }
    }
}
