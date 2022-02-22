using System.IO;
using Compression.Algorithms;
using IO;
using VariantAnnotation.PSA;
using Xunit;

namespace UnitTests.VariantAnnotation.PSA
{
    public sealed class PsaGeneBlockTests
    {
        [Fact]
        public void ReadWriteTest()
        {
            var    geneBlock = PsaTestUtilities.GetGene1Block();
            var    stream    = new MemoryStream();
            byte[] writeBuffer;
            using (var writer = new ExtendedBinaryWriter(stream))
            {
                geneBlock.Write(writer);
                writer.Flush();
                writeBuffer = stream.GetBuffer();
            }
            
            using (var reader = new ExtendedBinaryReader(new MemoryStream(writeBuffer)))
            {
                var readBlock = PsaGeneBlock.Read(reader, new Zstandard());
                
                Assert.True(PsaTestUtilities.Equals(geneBlock,readBlock));
            }
        }
    }
}