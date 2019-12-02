using SAUtils.CreateMitoMapDb;
using SAUtils.DataStructures;
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

        [Theory]
        [InlineData("0 (0)", MitoMapDataTypes.MitoMapMutationsRNA, 0)]
        [InlineData("<a href='/cgi-bin/index_mitomap.cgi?title=RNA+Mutation+A+at+750&pos=750&ref=A&alt=A' target=_blank>858 (0)</a>\"", MitoMapDataTypes.MitoMapMutationsRNA, 858)]
        [InlineData("<a href='/cgi-bin/index_mitomap.cgi?title=Coding+Control+Mutation+T-C+at+16217&pos=16217&ref=T&alt=C' target=_blank>3657 (4688)</a>", MitoMapDataTypes.MitoMapMutationsCodingControl, 3657)]
        [InlineData("<a href='/cgi-bin/index_mitomap.cgi?title=Coding+Polymorphism+T-C+at+rCRS+position+650&pos=650&ref=T&alt=C&purge_type=' target='_blank'>36</a>", MitoMapDataTypes.MitoMapPolymorphismsCoding, 36)]
        [InlineData("0", MitoMapDataTypes.MitoMapPolymorphismsCoding, 0)]
        [InlineData("0", MitoMapDataTypes.MitoMapPolymorphismsControl, 0)]
        [InlineData("<a href='/cgi-bin/index_mitomap.cgi?title=Control+Polymorphism+T-C+at+rCRS+position+14&pos=14&ref=T&alt=C' target='_blank'>5 (3/2)</a>", MitoMapDataTypes.MitoMapPolymorphismsControl, 3)]
        [InlineData("<a href='/cgi-bin/index_mitomap.cgi?title=Control+Polymorphism+T-A+at+rCRS+position+14&pos=14&ref=T&alt=A' target='_blank'>38 (0/38)</a>", MitoMapDataTypes.MitoMapPolymorphismsControl, 0)]
        public void GetNumFullLengthSequences_AsExpected(string field, string dataType, int numFullLengthSequences)
        {
            Assert.Equal(numFullLengthSequences, MitoMapVariantReader.GetNumFullLengthSequences(field, dataType));
        }
    }
}