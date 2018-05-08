using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using Jasix;
using Jasix.DataStructures;
using Xunit;
using Compression.FileHandling;
using IO;
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

            using (var qp = new QueryProcessor(FileUtilities.GetStreamReader(readStream), indexStream))
            {
                var header = qp.GetHeader();
                Assert.Equal("\"header\":{\"annotator\":\"Nirvana 2.0.9.0\",\"creationTime\":\"2018-04-30 15:44:31\",\"genomeAssembly\":\"GRCh37\",\"schemaVersion\":6,\"dataVersion\":\"91.26.45\",\"dataSources\":[{\"name\":\"VEP\",\"version\":\"91\",\"description\":\"Ensembl\",\"releaseDate\":\"2018-03-05\"},{\"name\":\"ClinVar\",\"version\":\"20180129\",\"description\":\"A freely accessible, public archive of reports of the relationships among human variations and phenotypes, with supporting evidence\",\"releaseDate\":\"2018-01-29\"},{\"name\":\"COSMIC\",\"version\":\"84\",\"description\":\"somatic mutation and related details and information relating to human cancers\",\"releaseDate\":\"2018-02-13\"},{\"name\":\"dbSNP\",\"version\":\"150\",\"description\":\"Identifiers for observed variants\",\"releaseDate\":\"2017-04-03\"},{\"name\":\"gnomAD_exome\",\"version\":\"2.0.2\",\"description\":\"Exome allele frequencies from Genome Aggregation Database (gnomAD)\",\"releaseDate\":\"2017-10-05\"},{\"name\":\"gnomAD\",\"version\":\"2.0.2\",\"description\":\"Whole genome allele frequencies from Genome Aggregation Database (gnomAD)\",\"releaseDate\":\"2017-10-05\"},{\"name\":\"MITOMAP\",\"version\":\"20180228\",\"description\":\"Small variants in the MITOMAP human mitochondrial genome database\",\"releaseDate\":\"2018-02-28\"},{\"name\":\"1000 Genomes Project\",\"version\":\"Phase 3 v5a\",\"description\":\"A public catalogue of human variation and genotype data\",\"releaseDate\":\"2013-05-27\"},{\"name\":\"TOPMed\",\"version\":\"freeze_5\",\"description\":\"Allele frequencies from TOPMed data lifted over using dbSNP ids.\",\"releaseDate\":\"2017-08-28\"},{\"name\":\"ClinGen\",\"version\":\"20160414\",\"releaseDate\":\"2016-04-14\"},{\"name\":\"DGV\",\"version\":\"20160515\",\"description\":\"Provides a comprehensive summary of structural variation in the human genome\",\"releaseDate\":\"2016-05-15\"},{\"name\":\"MITOMAP\",\"version\":\"20180228\",\"description\":\"Large structural variants in the MITOMAP human mitochondrial genome database\",\"releaseDate\":\"2018-02-28\"},{\"name\":\"ExAC\",\"version\":\"0.3.1\",\"description\":\"Gene scores from the ExAC project\",\"releaseDate\":\"2016-03-16\"},{\"name\":\"OMIM\",\"version\":\"20180213\",\"description\":\"An Online Catalog of Human Genes and Genetic Disorders\",\"releaseDate\":\"2018-02-13\"},{\"name\":\"phyloP\",\"version\":\"hg19\",\"description\":\"46 way conservation score between humans and 45 other vertebrates\",\"releaseDate\":\"2009-11-10\"}]}", header);

                var results =
                    qp.ReadOverlappingJsonLines(Utilities.ParseQuery("1:9775924"));
                Assert.Single(results);
            }
        }

        [Fact]
        public void TestQueryMultiple()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz.jsi"));

            using (var qp = new QueryProcessor(FileUtilities.GetStreamReader(readStream), indexStream))
            {
                var results =
                    qp.ReadOverlappingJsonLines(Utilities.ParseQuery("1:9775924-9778952"));
                Assert.Equal(3, results.Count());

            }
        }

        [Fact]
        public void TestQueryMultipleWithSkippingMiddleOne()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz.jsi"));

            using (var qp = new QueryProcessor(FileUtilities.GetStreamReader(readStream), indexStream))
            {
                var results =
                    qp.ReadOverlappingJsonLines(Utilities.ParseQuery("1:27023180-27023190"));
                Assert.Equal(2, results.Count());
            }
        }

        [Fact]
        public void TestQueryChr1()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz.jsi"));

            using (var qp = new QueryProcessor(FileUtilities.GetStreamReader(readStream), indexStream))
            {
                var results =
                    qp.ReadOverlappingJsonLines(Utilities.ParseQuery("1"));

                Assert.Equal(422, results.Count());
            }
        }

        [Fact]
        public void Query_onthefly_Ensembl_and_Ucsc()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("Clinvar20150901.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("Clinvar20150901.json.gz.jsi"));

            using (var qp = new QueryProcessor(FileUtilities.GetStreamReader(readStream), indexStream))
            {
                int ucscCount = qp.ProcessQuery(new[] {"chr1"});
                int ensemblCount = qp.ProcessQuery(new[] { "1" });

                Assert.Equal(13, ucscCount);
                Assert.Equal(13, ensemblCount);
            }
        }


        [Fact]
        public void Report_overlapping_small_and_extending_large_variants()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("JasixTest.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("JasixTest.json.gz.jsi"));

            using (var qp = new QueryProcessor(FileUtilities.GetStreamReader(readStream), indexStream))
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

            using (var qp = new QueryProcessor(FileUtilities.GetStreamReader(readStream), indexStream))
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

            using (var qp = new QueryProcessor(FileUtilities.GetStreamReader(readStream), indexStream))
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
