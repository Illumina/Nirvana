using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.FileHandling;
using Genome;
using Jasix.DataStructures;
using Jist;
using Moq;
using Newtonsoft.Json.Linq;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO;
using Xunit;

namespace UnitTests.Jist
{
    public sealed class JiSTtests
    {
        private const string NirvanaHeader = "{\"header\":\"Jist test header\",\"positions\":[\n";
        private const string NirvanaGenes = JsonStitcher.GeneHeaderLine;
        private const string NirvanaFooter = JsonStitcher.FooterLine;

        private static (Stream jsonStream, Stream jasixStream) GetJsonStreams(Chromosome chromosome, bool withGenes)
        {
            var jsonStream  = new MemoryStream();
            var jasixStream = new MemoryStream();
            var annotationResources = new Mock<IAnnotationResources>();
            annotationResources.SetupGet(x => x.AnnotatorVersionTag).Returns("NirvanaTest");
            annotationResources.SetupGet(x => x.VepDataVersion).Returns("VEPTest");
            annotationResources.SetupGet(x => x.DataSourceVersions).Returns(new List<IDataSourceVersion>());
            annotationResources.SetupGet(x => x.SequenceProvider.Assembly).Returns(GenomeAssembly.GRCh38);

            using (var jsonWriter = new JsonWriter(new BlockGZipStream(jsonStream, CompressionMode.Compress, true), jasixStream, annotationResources.Object, "2020-05-17", null, true))
            {
                var position = new Mock<IPosition>();
                position.SetupGet(x => x.Chromosome).Returns(chromosome);
                
                for (int i = 100 * (chromosome.Index+1); i < 123 *(chromosome.Index +1); i++)
                {
                    position.SetupGet(x => x.Start).Returns(i);
                    position.SetupGet(x => x.RefAllele).Returns("A");
                    position.SetupGet(x => x.AltAlleles).Returns(new []{"T"});
                    jsonWriter.WritePosition(position.Object, $"{JsonObject.OpenBrace}\"chromosome\":\"{chromosome.UcscName}\",\"position\":{i}{JsonObject.CloseBrace}");
                }

                if (withGenes)
                {
                    var geneEntries = new string[]
                    {
                        $"{{\"gene{chromosome.EnsemblName}A\":\"gene annotation\"}}",
                        $"{{\"gene{chromosome.EnsemblName}B\":\"gene annotation\"}}"

                    };
                    jsonWriter.WriteGenes(geneEntries);

                }
            }

            jsonStream.Position = 0;
            jasixStream.Position = 0;
            return (jsonStream, jasixStream);
        }

        [Fact]
        public void All_jsons_with_genes()
        {
            var jsonStreams = new Stream[3];
            var jasixSteams = new Stream[3];

            (jsonStreams[0], jasixSteams[0]) = GetJsonStreams(ChromosomeUtilities.Chr1, true);
            (jsonStreams[1], jasixSteams[1]) = GetJsonStreams(ChromosomeUtilities.Chr2, true);
            (jsonStreams[2], jasixSteams[2]) = GetJsonStreams(ChromosomeUtilities.Chr3, true);

            var outStream = new MemoryStream();
            using (var stitcher = new JsonStitcher(jsonStreams, jasixSteams, outStream, true))
            {
                stitcher.Stitch();
            }

            outStream.Position = 0;
            var sb = new StringBuilder();
            using (var bgZipStream = new BlockGZipStream(outStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(bgZipStream))
            {
                string line;
                while ((line = reader.ReadLine())!=null)
                {
                    sb.Append(line+'\n');
                }
            }

            var fullJson = sb.ToString();
            //making sure all the first and last positions are present in the merged JSON
            Assert.Contains("\"header\":{\"annotator\":\"NirvanaTest\"", fullJson);
            Assert.Contains("{\"chromosome\":\"chr1\",\"position\":100}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr1\",\"position\":122}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr2\",\"position\":200}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr2\",\"position\":222}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr3\",\"position\":300}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr3\",\"position\":322}", fullJson);
            
            //checking if all the genes are there
            Assert.Contains("gene1A", fullJson);
            Assert.Contains("gene1B", fullJson);
            Assert.Contains("gene2A", fullJson);
            Assert.Contains("gene2B", fullJson);
            Assert.Contains("gene3A", fullJson);
            Assert.Contains("gene3B", fullJson);

            
            //need to check if this is a valid json
            var jObject = JObject.Parse(fullJson);
            Assert.NotNull(jObject);
        }
        
        [Fact]
        public void Some_with_genes()
        {
            var jsonStreams = new Stream[3];
            var jasixSteams = new Stream[3];

            (jsonStreams[0], jasixSteams[0]) = GetJsonStreams(ChromosomeUtilities.Chr1, true);
            (jsonStreams[1], jasixSteams[1]) = GetJsonStreams(ChromosomeUtilities.Chr2, false);
            (jsonStreams[2], jasixSteams[2]) = GetJsonStreams(ChromosomeUtilities.Chr3, true);

            var outStream = new MemoryStream();
            using (var stitcher = new JsonStitcher(jsonStreams, jasixSteams, outStream, true))
            {
                stitcher.Stitch();
            }

            outStream.Position = 0;
            var sb = new StringBuilder();
            using (var bgZipStream = new BlockGZipStream(outStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(bgZipStream))
            {
                string line;
                while ((line = reader.ReadLine())!=null)
                {
                    sb.Append(line+'\n');
                }
            }

            var fullJson = sb.ToString();
            //making sure all the first and last positions are present in the merged JSON
            Assert.Contains("\"header\":{\"annotator\":\"NirvanaTest\"", fullJson);
            Assert.Contains("{\"chromosome\":\"chr1\",\"position\":100}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr1\",\"position\":122}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr2\",\"position\":200}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr2\",\"position\":222}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr3\",\"position\":300}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr3\",\"position\":322}", fullJson);
            
            //checking if all the genes are there
            Assert.Contains("gene1A", fullJson);
            Assert.Contains("gene1B", fullJson);
            Assert.DoesNotContain("gene2A", fullJson);
            Assert.DoesNotContain("gene2B", fullJson);
            Assert.Contains("gene3A", fullJson);
            Assert.Contains("gene3B", fullJson);

            
            //need to check if this is a valid json
            var jObject = JObject.Parse(fullJson);
            Assert.NotNull(jObject);
        }


        [Fact]
        public void All_jsons_without_genes()
        {
            var jsonStreams = new Stream[3];
            var jasixSteams = new Stream[3];

            (jsonStreams[0], jasixSteams[0]) = GetJsonStreams(ChromosomeUtilities.Chr1, false);
            (jsonStreams[1], jasixSteams[1]) = GetJsonStreams(ChromosomeUtilities.Chr2, false);
            (jsonStreams[2], jasixSteams[2]) = GetJsonStreams(ChromosomeUtilities.Chr3, false);

            var outStream = new MemoryStream();
            using (var stitcher = new JsonStitcher(jsonStreams, jasixSteams, outStream, true))
            {
                stitcher.Stitch();
            }

            outStream.Position = 0;
            var sb = new StringBuilder();
            using (var bgZipStream = new BlockGZipStream(outStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(bgZipStream))
            {
                string line;
                while ((line = reader.ReadLine())!=null)
                {
                    sb.Append(line+'\n');
                }
            }

            var fullJson = sb.ToString();
            //making sure all the first and last positions are present in the merged JSON
            Assert.Contains("\"header\":{\"annotator\":\"NirvanaTest\"", fullJson);
            Assert.Contains("{\"chromosome\":\"chr1\",\"position\":100}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr1\",\"position\":122}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr2\",\"position\":200}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr2\",\"position\":222}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr3\",\"position\":300}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr3\",\"position\":322}", fullJson);
            
            //checking if all the genes are there
            Assert.DoesNotContain("gene1A", fullJson);
            Assert.DoesNotContain("gene1B", fullJson);
            Assert.DoesNotContain("gene2A", fullJson);
            Assert.DoesNotContain("gene2B", fullJson);
            Assert.DoesNotContain("gene3A", fullJson);
            Assert.DoesNotContain("gene3B", fullJson);

            
            //need to check if this is a valid json
            var jObject = JObject.Parse(fullJson);
            Assert.NotNull(jObject);
        }

        //The following tests don't use JsonWriter. They are intended to isolate issues that might be due to some 
        // error in the json writer. The following tests try to create the ideal json output.
        private static (Stream jsonStream, Stream jasixStream) GetNirvanaJsonStream(int chromNumber)
        {
            var jsonStream = new MemoryStream();
            var jasixStream = new MemoryStream();

            using (var bgZipStream = new BlockGZipStream(jsonStream, CompressionMode.Compress, true))
            using (var writer = new BgzipTextWriter(bgZipStream))
            using(var jasixIndex = new JasixIndex())
            {
                writer.Write(NirvanaHeader);
                writer.Flush();
                jasixIndex.BeginSection(JasixCommons.PositionsSectionTag, writer.Position);
                for (int i = 100*chromNumber; i < 123*chromNumber; i++)
                {
                    writer.WriteLine($"{JsonObject.OpenBrace}\"chromosome\":\"chr{chromNumber}\",\"position\":{i}{JsonObject.CloseBrace},");
                    if(i%50==0) writer.Flush();//creating another block
                }
                writer.WriteLine($"{JsonObject.OpenBrace}\"chromosome\":\"chr{chromNumber}\",\"position\":{100*chromNumber+25}{JsonObject.CloseBrace}");
                writer.Flush();
                jasixIndex.EndSection(JasixCommons.PositionsSectionTag, writer.Position);
                
                writer.Write(NirvanaGenes);
                writer.Flush();
                
                jasixIndex.BeginSection(JasixCommons.GenesSectionTag, writer.Position);
                writer.WriteLine($"{{\"gene{chromNumber}A\":\"gene annotation\"}},");
                writer.WriteLine($"{{\"gene{chromNumber}B\":\"gene annotation\"}}");
                writer.Flush();
                jasixIndex.EndSection(JasixCommons.GenesSectionTag, writer.Position);
                writer.Write(NirvanaFooter);
                jasixIndex.Write(jasixStream);
            }

            jsonStream.Position = 0;
            jasixStream.Position = 0;
            return (jsonStream, jasixStream);
        }

        [Fact]
        public void EndToEndStitching()
        {
            var jsonStreams = new Stream[3];
            var jasixSteams = new Stream[3];

            (jsonStreams[0], jasixSteams[0]) = GetNirvanaJsonStream(1);
            (jsonStreams[1], jasixSteams[1]) = GetNirvanaJsonStream(2);
            (jsonStreams[2], jasixSteams[2]) = GetNirvanaJsonStream(3);

            var outStream = new MemoryStream();
            using (var stitcher = new JsonStitcher(jsonStreams, jasixSteams, outStream, true))
            {
                stitcher.Stitch();
            }

            outStream.Position = 0;
            var sb = new StringBuilder();
            using (var bgZipStream = new BlockGZipStream(outStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(bgZipStream))
            {
                string line;
                while ((line = reader.ReadLine())!=null)
                {
                    sb.Append(line+'\n');
                }
            }

            var fullJson = sb.ToString();
            //making sure all the first and last positions are present in the merged JSON
            Assert.Contains(NirvanaHeader, fullJson);
            Assert.Contains("{\"chromosome\":\"chr1\",\"position\":100}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr1\",\"position\":125}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr2\",\"position\":200}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr2\",\"position\":225}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr3\",\"position\":300}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr3\",\"position\":325}", fullJson);
            
            //checking if all the genes are there
            Assert.Contains("gene1A", fullJson);
            Assert.Contains("gene1B", fullJson);
            Assert.Contains("gene2A", fullJson);
            Assert.Contains("gene2B", fullJson);
            Assert.Contains("gene3A", fullJson);
            Assert.Contains("gene3B", fullJson);

            
            //need to check if this is a valid json
            var jObject = JObject.Parse(fullJson);
            Assert.NotNull(jObject);
        }

        private static (Stream jsonStream, Stream jasixStream) GetNirvanaJsonStreamWithoutGenes(int chromNumber)
        {
            var jsonStream  = new MemoryStream();
            var jasixStream = new MemoryStream();

            using (var bgZipStream = new BlockGZipStream(jsonStream, CompressionMode.Compress, true))
            using (var writer = new BgzipTextWriter(bgZipStream))
            using(var jasixIndex = new JasixIndex())
            {
                writer.Write(NirvanaHeader);
                writer.Flush();
                jasixIndex.BeginSection(JasixCommons.PositionsSectionTag, writer.Position);
                for (int i = 100 *chromNumber; i < 123 *chromNumber; i++)
                {
                    writer.WriteLine($"{JsonObject.OpenBrace}\"chromosome\":\"chr{chromNumber}\",\"position\":{i}{JsonObject.CloseBrace},");
                    if(i %50 ==0) writer.Flush();//creating another block
                }
                writer.WriteLine($"{JsonObject.OpenBrace}\"chromosome\":\"chr{chromNumber}\",\"position\":{100 *chromNumber +25}{JsonObject.CloseBrace}");
                writer.Flush();
                jasixIndex.EndSection(JasixCommons.PositionsSectionTag, writer.Position);
                
                writer.Write(NirvanaFooter);
                jasixIndex.Write(jasixStream);
            }

            jsonStream.Position  = 0;
            jasixStream.Position = 0;
            return (jsonStream, jasixStream);
        }
        [Fact]
        public void StitchingWithoutGenes()
        {
            var jsonStreams = new Stream[3];
            var jasixSteams = new Stream[3];

            (jsonStreams[0], jasixSteams[0]) = GetNirvanaJsonStream(1);
            (jsonStreams[1], jasixSteams[1]) = GetNirvanaJsonStreamWithoutGenes(2);
            (jsonStreams[2], jasixSteams[2]) = GetNirvanaJsonStream(3);

            var outStream = new MemoryStream();
            using (var stitcher = new JsonStitcher(jsonStreams, jasixSteams, outStream, true))
            {
                stitcher.Stitch();
            }

            outStream.Position = 0;
            var sb = new StringBuilder();
            using (var bgZipStream = new BlockGZipStream(outStream, CompressionMode.Decompress))
            using (var reader = new StreamReader(bgZipStream))
            {
                string line;
                while ((line = reader.ReadLine())!=null)
                {
                    sb.Append(line+'\n');
                }
            }

            var fullJson = sb.ToString();
            //making sure all the first and last positions are present in the merged JSON
            Assert.Contains(NirvanaHeader, fullJson);
            Assert.Contains("{\"chromosome\":\"chr1\",\"position\":100}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr1\",\"position\":125}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr2\",\"position\":200}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr2\",\"position\":225}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr3\",\"position\":300}", fullJson);
            Assert.Contains("{\"chromosome\":\"chr3\",\"position\":325}", fullJson);
            
            //checking if all the genes are there
            Assert.Contains("gene1A", fullJson);
            Assert.Contains("gene1B", fullJson);
            Assert.Contains("gene3A", fullJson);
            Assert.Contains("gene3B", fullJson);

            
            //need to check if this is a valid json
            var jObject = JObject.Parse(fullJson);
            Assert.NotNull(jObject);
        }
    }
}