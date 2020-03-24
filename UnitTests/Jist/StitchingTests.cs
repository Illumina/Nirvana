using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.FileHandling;
using Jasix.DataStructures;
using Jist;
using Newtonsoft.Json.Linq;
using Xunit;

namespace UnitTests.Jist
{
    public class StitchingTests
    {
        private const string NirvanaHeader = "{\"header\":\"Jist test header\",\"positions\":[\n";
        private const string NirvanaGenes = JsonStitcher.GeneHeaderLine;
        private const string NirvanaFooter = JsonStitcher.FooterLine;
        
        private (Stream jsonStream, Stream jasixStream) GetNirvanaJsonStream(int chromNumber)
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
                    writer.WriteLine($"{{\"chromosome\":\"chr{chromNumber}\",\"position\":{i}}},");
                    if(i%50==0) writer.Flush();//creating another block
                }
                writer.WriteLine($"{{\"chromosome\":\"chr{chromNumber}\",\"position\":{100*chromNumber+25}}}");
                
                jasixIndex.EndSection(JasixCommons.PositionsSectionTag, writer.Position);
                writer.Flush();
                
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
                    sb.Append(line);
                }
            }

            var fullJson = sb.ToString();
            //making sure all the first and last positions are present in the merged JSON
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
    }
}