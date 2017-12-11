using SAUtils.InputFileParsers.MitoMap;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public class MitoMapVariantReaderTests
    {
        [Fact]
        public void GetAltAllelesTests()
        {
            string altAlleleString1 = "ACT";
            string altAlleleString2 = "ACT;AGT";
            string altAlleleString3 = "AKY";
            string altAlleleString4 = "ACT;AKY";
            string altAlleleString5 = "CNT;AKY";
            Assert.Equal(new[] { "ACT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString1));
            Assert.Equal(new[] { "ACT", "AGT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString2));
            Assert.Equal(new[] { "AGC", "AGT", "ATC", "ATT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString3));
            Assert.Equal(new[] { "ACT", "AGC", "AGT", "ATC", "ATT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString4));
            Assert.Equal(new[] { "CNT", "AGC", "AGT", "ATC", "ATT" }, MitoMapVariantReader.GetAltAlleles(altAlleleString5));
        }
    }
}