using System;
using System.IO;
using System.Text;
using Cache.Data;
using IO;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Cache.Data;

public sealed class RegulatoryRegionTests
{
    [Fact]
    public void Write_EndToEnd_ExpectedResults()
    {
        int[] expectedPubMedIds = {21731768};
        RegulatoryRegion expected = new(ChromosomeUtilities.ChrX, 835_390, 835_434, "108410392", BioType.conserved_region,
            "conserved region; CRCNE00011105 more deeply conserved sub-region", expectedPubMedIds, 200);

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            expected.Write(writer);
        }

        byte[]             bytes    = ms.ToArray();
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
        RegulatoryRegion   actual   = RegulatoryRegion.Read(ref byteSpan, expected.Chromosome);

        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void Write_EndToEnd_NoNote_ExpectedResults()
    {
        int[] expectedPubMedIds = {21731768};
        RegulatoryRegion expected = new(ChromosomeUtilities.ChrX, 835_390, 835_434, "108410392", BioType.conserved_region, null,
            expectedPubMedIds, 200);

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            expected.Write(writer);
        }

        byte[]             bytes    = ms.ToArray();
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
        RegulatoryRegion   actual   = RegulatoryRegion.Read(ref byteSpan, expected.Chromosome);

        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void Write_EndToEnd_NoPubMedIds_ExpectedResults()
    {
        RegulatoryRegion expected = new(ChromosomeUtilities.ChrX, 835_390, 835_434, "108410392", BioType.conserved_region,
            "conserved region; CRCNE00011105 more deeply conserved sub-region", null, 200);

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            expected.Write(writer);
        }

        byte[]             bytes    = ms.ToArray();
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
        RegulatoryRegion   actual   = RegulatoryRegion.Read(ref byteSpan, expected.Chromosome);

        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void Write_EndToEnd_NoEcoId_ExpectedResults()
    {
        int[] expectedPubMedIds = {21731768};
        RegulatoryRegion expected = new(ChromosomeUtilities.ChrX, 835_390, 835_434, "108410392", BioType.conserved_region,
            "conserved region; CRCNE00011105 more deeply conserved sub-region", expectedPubMedIds, null);

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            expected.Write(writer);
        }

        byte[]             bytes    = ms.ToArray();
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
        RegulatoryRegion   actual   = RegulatoryRegion.Read(ref byteSpan, expected.Chromosome);

        Assert.Equal(expected, actual);
    }
}