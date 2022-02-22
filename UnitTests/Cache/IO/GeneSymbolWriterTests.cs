using System.IO;
using Cache.Data;
using Cache.IO;
using Xunit;

namespace UnitTests.Cache.IO;

public sealed class GeneSymbolWriterTests
{
    [Fact]
    public void GeneSymbolWriter_ExpectedResults()
    {
        HgncGeneSymbol[] expectedGeneSymbols =
        {
            new(123, "ABC"),
            new(456, "DEF")
        };

        using var ms = new MemoryStream();
        using (var writer = new GeneSymbolWriter(ms, true))
        {
            writer.Write(expectedGeneSymbols);
        }

        ms.Position = 0;

        HgncGeneSymbol[] actualGeneSymbols;
        using (var reader = new GeneSymbolReader(ms))
        {
            actualGeneSymbols = reader.GetHgncGeneSymbols();
        }

        Assert.Equal(expectedGeneSymbols, actualGeneSymbols);
    }
}