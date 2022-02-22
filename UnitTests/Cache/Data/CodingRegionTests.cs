using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cache.Data;
using IO;
using Xunit;

namespace UnitTests.Cache.Data;

public sealed class CodingRegionTests
{
    private const string ExpectedProteinSeq =
        "MGRLVLLWGAAVFLLGGWMALGQGGAAEGVQIQIIYFNLETVQVTWNASKYSRTNLTFHYRFNGDEAYDQCTNYLLQEGHTSGCLLDAEQRDDILYFSIRNGTHPVFTASRWMVYYLKPSSPKHVRFSWHQDAVTVTCSDLSYGDLLYEVQYRSPFDTEWQSKQENTCNVTIEGLDAEKCYSFWVRVKAMEDVYGPDTYPSDWSEVTCWQRGEIRDACAETPTPPKPKLSKFILISSLAILLMVSLLLLSLWKLWRVKKFLIPSVPDPKSIFPGLFEIHQGNFQEWITDTQNVAHLHKMAGAEQESGPEEPLVVQLAKTEAESPRMLDPQTEEKEASGGSLQLPHQPLQGGDVVTIGGFTFVMNDRSYVAL";

    private readonly Dictionary<string, int> _proteinSeqIndices = new();
    private readonly string[]                _proteinSeqs       = {ExpectedProteinSeq};

    public CodingRegionTests() => _proteinSeqIndices[ExpectedProteinSeq] = 0;

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void Write_EndToEnd_ExpectedResults(bool hasAminoAcidEdits, bool hasSlip)
    {
        AminoAcidEdit[]?   aminoAcidEdits = hasAminoAcidEdits ? new[] {new AminoAcidEdit(123, 'U')} : null;
        TranslationalSlip? slip           = hasSlip ? new TranslationalSlip(123, 2) : null;

        CodingRegion expected = new(1_314_869, 1_331_527, 16, 810, "NP_000305.3", ExpectedProteinSeq, 2, 389, 456,
            aminoAcidEdits, slip);

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            expected.Write(writer, _proteinSeqIndices);
        }

        byte[]             bytes    = ms.ToArray();
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
        CodingRegion       actual   = CodingRegion.Read(ref byteSpan, _proteinSeqs);

        Assert.Equal(expected, actual);
    }
}