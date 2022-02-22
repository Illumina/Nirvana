using System;
using System.IO;
using Genome;
using IO;
using VariantAnnotation.PSA;
using VariantAnnotation.SA;
using Versioning;
using Xunit;

namespace UnitTests.VariantAnnotation.PSA;

public sealed class TestPsaIndex
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

        index.Add(0, "NM_0001.1", 100);
        index.Add(0, "NM_0002.1", 200);
        index.Add(1, "NM_0003.1", 300);
        index.Add(1, "NM_0004.1", 400);

        index.Write(writer);

        //now reading back
        var streamBuffer = stream.GetBuffer();
        var readStream   = new MemoryStream(streamBuffer);
        var reader       = new ExtendedBinaryReader(readStream);
        var readIndex    = PsaIndex.Read(reader);

        Assert.Equal(assembly,           readIndex.Header.Assembly);
        Assert.Equal(schemaVersion,      readIndex.Header.SchemaVersion);
        Assert.Equal(version.ToString(), readIndex.Header.Version.ToString());

        Assert.Equal(100, index.GetFileLocation(0, "NM_0001.1"));
        Assert.Equal(200, index.GetFileLocation(0, "NM_0002.1"));
        Assert.Equal(300, index.GetFileLocation(1, "NM_0003.1"));
        Assert.Equal(400, index.GetFileLocation(1, "NM_0004.1"));
    }
}