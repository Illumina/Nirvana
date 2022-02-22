using System;
using System.IO;
using Genome;
using SAUtils.Psa;
using UnitTests.TestUtilities;
using VariantAnnotation.PSA;
using VariantAnnotation.SA;
using Versioning;

namespace UnitTests.VariantAnnotation.PSA;

public static class PsaTestUtilities
{
    private static Stream GetSiftTsvStream()
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);

        writer.WriteLine("chr7\tNM_005228.3\t1\tM\tA\t0.01\tdeleterious - low confidence");
        writer.WriteLine("chr7\tNM_005228.3\t3\tP\tS\t0.23\ttolerated - low confidence");
        writer.WriteLine("chr7	NM_005228.3	7	A	C	0.13	tolerated");
        writer.WriteLine("chr7\tNM_005228.3\t7\tA\tD\t0.05\tdeleterious");
        writer.WriteLine("chr10\tNM_020975.4\t1\tM\tC\t0\tdeleterious - low confidence");
        writer.WriteLine("chr10\tNM_020975.4\t2\tA\tC\t0.02\tdeleterious - low confidence");
        writer.WriteLine("chr11\tNM_001130442.2\t1\tM\tA\t0\tdeleterious");
        writer.WriteLine("chr11\tNM_001130442.2\t2\tT\tA\t0.31\ttolerated");
        writer.Flush();

        stream.Position = 0;
        return stream;
    }

    private static PsaParser GetSiftParser(Stream stream) => new PsaParser(new StreamReader(stream));

    private static PsaWriter GetSiftPsaWriter(Stream psaStream, Stream indexStream)
    {
        var schemaVersion = SaCommon.PsaSchemaVersion;
        IDataSourceVersion version =
            new DataSourceVersion("Sift", "sift as a supplementary data", "6.48", DateTime.Now.Ticks);
        var assembly = GenomeAssembly.GRCh38;
        var jsonKey  = SaCommon.SiftTag;
        var header   = new SaHeader(jsonKey, assembly, version, schemaVersion);

        var psaWriter = new PsaWriter(psaStream, indexStream, header, ChromosomeUtilities.RefNameToChromosome);
        return psaWriter;
    }

    public static PsaReader GetSiftPsaReader()
    {
        var psaStream   = new MemoryStream();
        var indexStream = new MemoryStream();


        using var psaWriter = GetSiftPsaWriter(psaStream, indexStream);
        psaWriter.Write(GetSiftParser(GetSiftTsvStream()).GetItems());

        byte[] readBuffer  = psaStream.GetBuffer();
        byte[] indexBuffer = indexStream.GetBuffer();
        psaStream   = new MemoryStream(readBuffer);
        indexStream = new MemoryStream(indexBuffer);
        return new PsaReader(psaStream, indexStream);
    }
}