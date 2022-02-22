using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cache.Data;
using IO;
using Xunit;

namespace UnitTests.Cache.Data;

public sealed class GeneTests
{
    private static readonly Dictionary<int, string> HgncIdToSymbol = new();
    
    [Fact]
    public void Write_EndToEnd_ExpectedResults()
    {
        Gene expected = new("64109", "ENSG00000205755", true, 14281) {Symbol = "CRLF2"};

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            expected.Write(writer);
        }

        byte[]             bytes    = ms.ToArray();
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
        Gene               actual   = Gene.Read(ref byteSpan, HgncIdToSymbol);
        
        Assert.Equal(expected,        actual);
        Assert.Equal(expected.Symbol, actual.Symbol);
    }
    
    [Fact]
    public void Write_EndToEnd_NoHgncId_ExpectedResults()
    {
        Gene expected = new("64109", "ENSG00000205755", true, null) {Symbol = "CRLF2"};

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            expected.Write(writer);
        }

        byte[]             bytes    = ms.ToArray();
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
        Gene               actual   = Gene.Read(ref byteSpan, HgncIdToSymbol);

        Assert.Equal(expected,        actual);
        Assert.Equal(expected.Symbol, actual.Symbol);
    }
    
    [Fact]
    public void Write_EndToEnd_GeneSymbolOverride_ExpectedResults()
    {
        var hgncIdToSymbol = new Dictionary<int, string> {[123] = "CRLF2+"};

        Gene gene     = new("64109", "ENSG00000205755", true, 123) {Symbol = "CRLF2"};
        Gene expected = gene with {Symbol = "CRLF2+"};

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            expected.Write(writer);
        }

        byte[]             bytes    = ms.ToArray();
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
        Gene               actual   = Gene.Read(ref byteSpan, hgncIdToSymbol);

        Assert.Equal(expected,        actual);
        Assert.Equal(expected.Symbol, actual.Symbol);
    }
}