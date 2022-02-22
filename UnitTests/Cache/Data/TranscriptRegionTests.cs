using System;
using System.IO;
using System.Text;
using Cache.Data;
using IO;
using Xunit;

namespace UnitTests.Cache.Data;

public sealed class TranscriptRegionTests
{
    [Fact]
    public void Write_EndToEnd_ExpectedResults()
    {
        var expectedCigarOps = new[]
        {
            new CigarOp(CigarType.Match,     121),
            new CigarOp(CigarType.Insertion, 1),
            new CigarOp(CigarType.Match,     10),
            new CigarOp(CigarType.Insertion, 2),
            new CigarOp(CigarType.Match,     15)
        };

        TranscriptRegion expected = new(1314869, 1315014, 810, 89, TranscriptRegionType.Exon, 6, expectedCigarOps);

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            expected.Write(writer);
        }

        byte[]             bytes    = ms.ToArray();
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
        TranscriptRegion   actual   = TranscriptRegion.Read(ref byteSpan);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Write_EndToEnd_NoCigarOps_ExpectedResults()
    {
        TranscriptRegion expected = new(1314869, 1315014, 810, 89, TranscriptRegionType.Exon, 6, null);

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            expected.Write(writer);
        }

        byte[]             bytes    = ms.ToArray();
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
        TranscriptRegion   actual   = TranscriptRegion.Read(ref byteSpan);

        Assert.Equal(expected, actual);
    }
}