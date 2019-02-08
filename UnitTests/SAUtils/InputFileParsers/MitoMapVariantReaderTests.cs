using SAUtils.CreateMitoMapDb;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class MitoMapVariantReaderTests
    {
        [Fact]
        public void GetAltAllelesTests()
        {
            const string altAlleleString1 = "ACT";
            const string altAlleleString2 = "ACT;AGT";
            const string altAlleleString3 = "AKY";
            const string altAlleleString4 = "ACT;AKY";
            const string altAlleleString5 = "CNT;AKY";
            Assert.Equal(new[] { "ACT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString1));
            Assert.Equal(new[] { "ACT", "AGT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString2));
            Assert.Equal(new[] { "AGC", "AGT", "ATC", "ATT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString3));
            Assert.Equal(new[] { "ACT", "AGC", "AGT", "ATC", "ATT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString4));
            Assert.Equal(new[] { "CNT", "AGC", "AGT", "ATC", "ATT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString5));
        }
    }
}