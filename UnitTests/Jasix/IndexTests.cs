﻿using System.IO;
using System.IO.Compression;
using System.Text;
using Jasix;
using Jasix.DataStructures;
using Xunit;
using UnitTests.TestUtilities;
using Compression.FileHandling;
using ErrorHandling.Exceptions;

namespace UnitTests.Jasix
{
    public sealed class IndexTests
    {
        [Fact]
        public void Query_succeedes_when_it_overlaps_tail_of_previous_bin()
        {
            var chrIndex = new JasixChrIndex("chr1");

            for (var i = 100; i < 100 + JasixCommons.PreferredNodeCount; i++)
            {
                chrIndex.Add(i, i + 5, 100_000 + i);
            }

            for (int i = 102 + JasixCommons.PreferredNodeCount; i < 152 + JasixCommons.PreferredNodeCount; i++)
            {
                chrIndex.Add(i, i + 5, 100_020 + i);
            }

            //close current node
            chrIndex.Flush();

            Assert.Equal(100_100, chrIndex.FindFirstSmallVariant(102, 103));
        }

        [Fact]
        public void Add_fill_node_and_start_another()
        {
            var index = new JasixIndex();

            //creating two nodes each containing 50 entries
            for (var i = 0; i < 2 * JasixCommons.PreferredNodeCount; i++)
            {
                index.Add("chr1", 100 + i, 101 + i, 100_000 + i);
            }

            index.Add("chr1", 160 + 2 * JasixCommons.PreferredNodeCount, 166 + 2 * JasixCommons.PreferredNodeCount, 200_100);
            index.Add("chr2", 100, 100, 200_150);
            index.Add("chr2", 102, 105, 200_200);

            index.Flush();

            Assert.Equal(100_000, index.GetFirstVariantPosition("chr1", 100, 102));
            Assert.Equal(100_000 + JasixCommons.PreferredNodeCount, index.GetFirstVariantPosition("chr1", 2 * JasixCommons.PreferredNodeCount + 55, 2 * JasixCommons.PreferredNodeCount + 55));
            Assert.Equal(-1, index.GetFirstVariantPosition("chr1", 2 * JasixCommons.PreferredNodeCount + 120, 2 * JasixCommons.PreferredNodeCount + 124));
            Assert.Equal(200_100, index.GetFirstVariantPosition("chr1", 2 * JasixCommons.PreferredNodeCount + 158, 2 * JasixCommons.PreferredNodeCount + 160));
            Assert.Equal(200_150, index.GetFirstVariantPosition("chr2", 103, 105));
        }


        [Fact]
        public void GetFirstVariantPosition_multi_chrom_index()
        {
            var index = new JasixIndex();

            index.Add("chr1", 100, 101, 100000);
            index.Add("chr1", 105, 109, 100050);
            index.Add("chr1", 160, 166, 100100);
            index.Add("chr2", 100, 100, 100150);
            index.Add("chr2", 102, 105, 100200);

            index.Flush();

            var chrPos = Utilities.ParseQuery("chr1");

            Assert.Equal(100000, index.GetFirstVariantPosition(chrPos.Item1, chrPos.Item2, chrPos.Item3));

            chrPos = Utilities.ParseQuery("chr2");
            Assert.Equal(100150, index.GetFirstVariantPosition(chrPos.Item1, chrPos.Item2, chrPos.Item3));
        }

        [Fact]
        public void FindLargeVaritants_method_does_not_return_small_variants()
        {
            var index = new JasixIndex();

            index.Add("chr1", 100, 101, 100_000);
            index.Add("chr1", 105, 109, 100_050);
            index.Add("chr1", 160, 166, 100_100);
            index.Add("chr1", 200, 1000, 100_075);//large variant
            index.Add("chr2", 100, 100, 100_150);
            index.Add("chr2", 102, 105, 100_200);

            index.Flush();

            //checking large variants
            Assert.Null(index.LargeVariantPositions("chr1", 100, 199));
            var largeVariants = index.LargeVariantPositions("chr1", 100, 201);
            Assert.NotNull(largeVariants);
            Assert.Single(largeVariants);
            Assert.Equal(100075, largeVariants[0]);
        }

        [Fact]
        public void Write_and_read_back()
        {
            var index = new JasixIndex();

            index.Add("chr1", 100, 101, 100000,"1");
            index.Add("chr1", 105, 109, 100050,"1");
            index.Add("chr1", 150, 1000, 100075,"1");//large variant
            index.Add("chr1", 160, 166, 100100, "1");
            index.Add("chr2", 100, 100, 100150, "2");
            index.Add("chr2", 102, 105, 100200, "2");

            var writeStream = new MemoryStream();
            using (writeStream)
            {
                index.Write(writeStream);
            }

            var readStream= new MemoryStream(writeStream.ToArray());
            readStream.Seek(0,SeekOrigin.Begin);

            JasixIndex readBackIndex;
            using (readStream)
            {
                readBackIndex = new JasixIndex(readStream);
            }

            Assert.Equal(100000, readBackIndex.GetFirstVariantPosition("chr1", 100, 102));
            Assert.Equal(100000, readBackIndex.GetFirstVariantPosition("chr1", 103, 104));
            Assert.Equal(100000, readBackIndex.GetFirstVariantPosition("chr1", 120, 124));
            Assert.Equal(100000, readBackIndex.GetFirstVariantPosition("chr1", 158, 160));
            Assert.Equal(100150, readBackIndex.GetFirstVariantPosition("chr2", 103, 105));

            //checking large variants
            Assert.Null(readBackIndex.LargeVariantPositions("chr1", 100, 149));
            var largeVariants = readBackIndex.LargeVariantPositions("chr1", 100, 201);
            Assert.NotNull(largeVariants);
            Assert.Single(largeVariants);
            Assert.Equal(100075, largeVariants[0]);
        }

        [Fact]
        public void BgzipTestReader_basic()
        {
            var stream = ResourceUtilities.GetReadStream(Resources.TopPath("TinyAnnotated.json"));

            var lineCount = 0;
            using (var jasixReader = new StreamReader(stream))
            {
                while (jasixReader.ReadLine() != null)
                {
                    lineCount++;
                }
            }

            Assert.Equal(4, lineCount);
        }

        [Fact]
        public void IndexCreation_multChromosome()
        {
            var jsonStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz")), CompressionMode.Decompress);

            var writeStream = new MemoryStream();
            using (var indexCreator = new IndexCreator(jsonStream, writeStream))
            {
                indexCreator.CreateIndex();
            }

            JasixIndex readBackIndex;
            var        readStream = new MemoryStream(writeStream.ToArray());
            readStream.Seek(0, SeekOrigin.Begin);

            using (readStream)
            {
                readBackIndex = new JasixIndex(readStream);
            }

            Assert.Equal(2268, readBackIndex.GetFirstVariantPosition("chr1", 9775924, 9775924));
            Assert.Equal(14035925971, readBackIndex.GetFirstVariantPosition("chr2", 16081096, 16081096));
            Assert.Equal(433156622693, readBackIndex.GetFirstVariantPosition("chr20", 36026164, 36026164));
            Assert.Equal(439602269527, readBackIndex.GetFirstVariantPosition("chrX", 66765044, 66765044));
        }

        [Fact]
        public void Begin_end_section_and_readback()
        {
            var index = new JasixIndex();
            const string section = "section1";
            index.BeginSection(section, 0);
            Assert.Throws<UserErrorException>(() => index.BeginSection(section, 1));
            index.EndSection(section, 100);
            Assert.Throws<UserErrorException>(() => index.EndSection(section, 101));

            Assert.Equal(0, index.GetSectionBegin(section));
            Assert.Equal(100, index.GetSectionEnd(section));
        }

        [Fact]
        public void GetChromosomeList()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("Clinvar20150901.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("Clinvar20150901.json.gz.jsi"));

            var outStream = new MemoryStream();
            using (var writer = new StreamWriter(outStream, Encoding.UTF8, 512, true))
            using (var qp = new QueryProcessor(new StreamReader(readStream), indexStream, writer))
            {
                writer.NewLine = "\r\n";
                qp.ListChromosomesAndSections();
            }

            Assert.NotEqual(0, outStream.Length);
            outStream.Position = 0;
            
            using (var reader = new StreamReader(outStream))
            {
                string chromList = reader.ReadToEnd();
                Assert.Equal("1\r\n2\r\n3\r\n4\r\n5\r\n6\r\n7\r\n8\r\n9\r\n10\r\n11\r\n12\r\n13\r\n14\r\n15\r\n16\r\n17\r\n18\r\n19\r\n20\r\n21\r\nX\r\nY\r\nheader\r\npositions\r\ngenes\r\n", chromList);
            }
        }

        [Fact]
        public void GetHeaderOnly()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("Clinvar20150901.json.gz")),
                CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("Clinvar20150901.json.gz.jsi"));

            var outStream = new MemoryStream();
            using (var writer = new StreamWriter(outStream, Encoding.UTF8, 512, true))
            using (var qp = new QueryProcessor(new StreamReader(readStream), indexStream, writer))
            {
                qp.PrintHeaderOnly();
            }

            Assert.NotEqual(0, outStream.Length);
            outStream.Position = 0;
            using (var reader = new StreamReader(outStream))
            {
                string actualHeaderLine = reader.ReadToEnd().Replace("\r\n", "\n");
                Assert.Equal(
                    "{\n  \"header\": {\n    \"annotator\": \"Nirvana 2.0.9.0\",\n    \"creationTime\": \"2018-04-30 17:17:23\",\n    \"genomeAssembly\": \"GRCh37\",\n    \"schemaVersion\": 6,\n    \"dataVersion\": \"91.26.45\",\n    \"dataSources\": [\n      {\n        \"name\": \"VEP\",\n        \"version\": \"91\",\n        \"description\": \"Ensembl\",\n        \"releaseDate\": \"2018-03-05\"\n      },\n      {\n        \"name\": \"ClinVar\",\n        \"version\": \"20180129\",\n        \"description\": \"A freely accessible, public archive of reports of the relationships among human variations and phenotypes, with supporting evidence\",\n        \"releaseDate\": \"2018-01-29\"\n      },\n      {\n        \"name\": \"COSMIC\",\n        \"version\": \"84\",\n        \"description\": \"somatic mutation and related details and information relating to human cancers\",\n        \"releaseDate\": \"2018-02-13\"\n      },\n      {\n        \"name\": \"dbSNP\",\n        \"version\": \"150\",\n        \"description\": \"Identifiers for observed variants\",\n        \"releaseDate\": \"2017-04-03\"\n      },\n      {\n        \"name\": \"gnomAD_exome\",\n        \"version\": \"2.0.2\",\n        \"description\": \"Exome allele frequencies from Genome Aggregation Database (gnomAD)\",\n        \"releaseDate\": \"2017-10-05\"\n      },\n      {\n        \"name\": \"gnomAD\",\n        \"version\": \"2.0.2\",\n        \"description\": \"Whole genome allele frequencies from Genome Aggregation Database (gnomAD)\",\n        \"releaseDate\": \"2017-10-05\"\n      },\n      {\n        \"name\": \"MITOMAP\",\n        \"version\": \"20180228\",\n        \"description\": \"Small variants in the MITOMAP human mitochondrial genome database\",\n        \"releaseDate\": \"2018-02-28\"\n      },\n      {\n        \"name\": \"1000 Genomes Project\",\n        \"version\": \"Phase 3 v5a\",\n        \"description\": \"A public catalogue of human variation and genotype data\",\n        \"releaseDate\": \"2013-05-27\"\n      },\n      {\n        \"name\": \"TOPMed\",\n        \"version\": \"freeze_5\",\n        \"description\": \"Allele frequencies from TOPMed data lifted over using dbSNP ids.\",\n        \"releaseDate\": \"2017-08-28\"\n      },\n      {\n        \"name\": \"ClinGen\",\n        \"version\": \"20160414\",\n        \"releaseDate\": \"2016-04-14\"\n      },\n      {\n        \"name\": \"DGV\",\n        \"version\": \"20160515\",\n        \"description\": \"Provides a comprehensive summary of structural variation in the human genome\",\n        \"releaseDate\": \"2016-05-15\"\n      },\n      {\n        \"name\": \"MITOMAP\",\n        \"version\": \"20180228\",\n        \"description\": \"Large structural variants in the MITOMAP human mitochondrial genome database\",\n        \"releaseDate\": \"2018-02-28\"\n      },\n      {\n        \"name\": \"ExAC\",\n        \"version\": \"0.3.1\",\n        \"description\": \"Gene scores from the ExAC project\",\n        \"releaseDate\": \"2016-03-16\"\n      },\n      {\n        \"name\": \"OMIM\",\n        \"version\": \"20180213\",\n        \"description\": \"An Online Catalog of Human Genes and Genetic Disorders\",\n        \"releaseDate\": \"2018-02-13\"\n      },\n      {\n        \"name\": \"phyloP\",\n        \"version\": \"hg19\",\n        \"description\": \"46 way conservation score between humans and 45 other vertebrates\",\n        \"releaseDate\": \"2009-11-10\"\n      }\n    ]\n  }\n}",
                    actualHeaderLine);
            }
        }

        [Fact]
        public void GetGeneSection()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("Clinvar20150901.json.gz")), CompressionMode.Decompress);
            var indexStream = ResourceUtilities.GetReadStream(Resources.TopPath("Clinvar20150901.json.gz.jsi"));

            var outStream = new MemoryStream();
            using (var writer = new StreamWriter(outStream, Encoding.UTF8, 512, true))
            using (var qp = new QueryProcessor(new StreamReader(readStream), indexStream, writer))
            {
                writer.NewLine = "\r\n";
                qp.PrintSection("genes");
            }

            Assert.NotEqual(0, outStream.Length);
            outStream.Position = 0;
            using (var reader = new StreamReader(outStream))
            {
                var count = 0;
                var line = reader.ReadLine();
                while (line != null)
                {
                    count++;
                    line = reader.ReadLine();
                }
                
                Assert.Equal(4382, count);
            }
        }

    }
}
