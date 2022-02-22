using System;
using System.IO;
using Genome;
using IO;
using VariantAnnotation.PSA;
using VariantAnnotation.SA;
using Versioning;
using Xunit;

namespace UnitTests.VariantAnnotation.PSA
{
    public sealed class PsaIndexTests
    {
        [Fact]
        public void ReadBackIndex()
        {
            var schemaVersion = SaCommon.PsaSchemaVersion;
            IDataSourceVersion version =
                new DataSourceVersion("Sift", "sift as a supplementary data", "6.48", DateTime.Now.Ticks);
            var assembly  = GenomeAssembly.GRCh38;
            var jsonKey   = "siftScore";
            var signature = new SaSignature(SaCommon.PsaIdentifier, 1_234_567);
            var header    = new SaHeader(jsonKey, assembly, version, schemaVersion);

            var stream = new MemoryStream();
            var writer = new ExtendedBinaryWriter(stream);
            var index  = new PsaIndex(header, signature);
            
            // add index blocks
            index.AddGeneBlock(0, "gene-1", 1000, 2000, 123);
            index.AddGeneBlock(0, "gene-2",1900, 2800, 234);
            index.AddGeneBlock(1, "gene-3",1000, 2000, 345);
            index.AddGeneBlock(1, "gene-4",1900, 2800, 456);

            index.Write(writer);
            
            var streamBuffer = stream.GetBuffer();
            var readStream   = new MemoryStream(streamBuffer);
            var reader       = new ExtendedBinaryReader(readStream);
            var readIndex    = PsaIndex.Read(reader);
            
            Assert.Equal(assembly,           readIndex.Header.Assembly);
            Assert.Equal(schemaVersion,      readIndex.Header.SchemaVersion);
            Assert.Equal(version.ToString(), readIndex.Header.Version.ToString());

            var blockPosition = readIndex.GetGeneBlockPosition(0, "gene-2");
            Assert.Equal(234, blockPosition);
            blockPosition = readIndex.GetGeneBlockPosition(1, "gene-3");
            Assert.Equal(345, blockPosition);
            
            blockPosition = readIndex.GetGeneBlockPosition(3, "gene-2");
            Assert.Equal(-1, blockPosition);
            blockPosition = readIndex.GetGeneBlockPosition(0, "gene-5");
            Assert.Equal(-1, blockPosition);
            
            writer.Dispose();
            reader.Dispose();
        }
    }
}