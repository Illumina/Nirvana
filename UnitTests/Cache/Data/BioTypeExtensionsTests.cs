using Cache.Data;
using Xunit;

namespace UnitTests.Cache.Data;

public class BioTypeExtensionsTests
{
    [Theory]
    [InlineData("exon",       BioType.exon)]
    [InlineData("CDS",        BioType.CDS)]
    [InlineData("mRNA",       BioType.mRNA)]
    [InlineData("gene",       BioType.gene)]
    [InlineData("cDNA_match", BioType.cDNA_match)]
    [InlineData("match",      BioType.match)]
    [InlineData("pseudogene", BioType.pseudogene)]
    [InlineData("transcript", BioType.transcript)]
    public void GetBioType_ExpectedResults(string s, BioType expected)
    {
        BioType actual = s.GetBioType();
        Assert.Equal(expected, actual);
    }
}