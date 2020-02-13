using Genome;
using Moq;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface;
using Vcf.VariantCreator;
using Xunit;

namespace UnitTests.Vcf.VariantCreator
{
    public sealed class VariantIdTests
    {
        private readonly ISequence _sequence;
        private readonly VariantId _vidCreator = new VariantId();

        public VariantIdTests()
        {
            var sequenceMock = new Mock<ISequence>();
            sequenceMock.Setup(x => x.Substring(999, 1)).Returns("N");
            sequenceMock.Setup(x => x.Substring(66520, 1)).Returns("T");
            sequenceMock.Setup(x => x.Substring(66571, 1)).Returns("G");
            sequenceMock.Setup(x => x.Substring(321681, 1)).Returns("G");
            sequenceMock.Setup(x => x.Substring(477967, 1)).Returns("A");
            sequenceMock.Setup(x => x.Substring(1350081, 1)).Returns("C");
            sequenceMock.Setup(x => x.Substring(1477853, 1)).Returns("A");
            sequenceMock.Setup(x => x.Substring(1477967, 1)).Returns("A");
            sequenceMock.Setup(x => x.Substring(1715897, 1)).Returns("A");
            sequenceMock.Setup(x => x.Substring(2633402, 1)).Returns("G");
            sequenceMock.Setup(x => x.Substring(2633403, 1)).Returns("G");
            sequenceMock.Setup(x => x.Substring(2650425, 1)).Returns("N");

            _sequence = sequenceMock.Object;
        }

        [Theory]
        [InlineData(66507, "T", ".", "1-66507-T-T")]
        [InlineData(66507, "T", "A", "1-66507-T-A")]
        [InlineData(66522, "", "ATATA", "1-66521-T-TATATA")]
        [InlineData(66573, "TA", "", "1-66572-GTA-G")]
        [InlineData(66573, "", "TACTATATATTA", "1-66572-G-GTACTATATATTA")]
        public void Create_SmallVariants_ReturnShortVid(int position, string refAllele, string altAllele, string expectedVid)
        {
            string observedVid = _vidCreator.Create(_sequence, VariantCategory.SmallVariant, null, ChromosomeUtilities.Chr1, position, position, refAllele, altAllele,
                null);
            Assert.Equal(expectedVid, observedVid);
        }

        [Fact]
        public void Create_TranslocationBreakend_ReturnShortVid()
        {
            string observedVid = _vidCreator.Create(_sequence, VariantCategory.SV, "BND", ChromosomeUtilities.Chr1, 2617277, 2617277, "A",
                "AAAAAAAAAAAAAAAAAATTAGTCAGGCAC[chr3:153444911[", null);
            Assert.Equal("1-2617277-A-AAAAAAAAAAAAAAAAAATTAGTCAGGCAC[chr3:153444911[", observedVid);
        }

        [Theory]
        [InlineData(1000, 3001000, "N", "<ROH>", "ROH", VariantCategory.ROH, "1-1000-3001000-N-<ROH>-ROH")]
        [InlineData(1350082, 1351320, "N", "<DEL>", "DEL", VariantCategory.SV, "1-1350082-1351320-C-<DEL>-DEL")]
        [InlineData(1477854, 1477984, "N", "<DUP:TANDEM>", "DUP", VariantCategory.SV, "1-1477854-1477984-A-<DUP:TANDEM>-DUP")]
        [InlineData(1477968, 1477968, "N", "<INS>", "INS", VariantCategory.SV, "1-1477968-1477968-A-<INS>-INS")]
        [InlineData(1715898, 1750149, "N", "<DUP>", "CNV", VariantCategory.CNV, "1-1715898-1750149-A-<DUP>-CNV")]
        [InlineData(2650426, 2653074, "N", "<DEL>", "CNV", VariantCategory.CNV, "1-2650426-2653074-N-<DEL>-CNV")]
        [InlineData(321682, 421681, "N", "<INV>", "INV", VariantCategory.SV, "1-321682-421681-G-<INV>-INV")]
        [InlineData(2633403, 2633421, "N", "<STR2>", "", VariantCategory.RepeatExpansion, "1-2633403-2633421-G-<STR2>-STR")]
        public void Create_StructuralVariants_RecoverRefAllele_ReturnLongVid(int position, int endPosition,
            string refAllele, string altAllele, string svType, VariantCategory category, string expectedVid)
        {
            string observedVid = _vidCreator.Create(_sequence, category, svType, ChromosomeUtilities.Chr1, position, endPosition, refAllele, altAllele, null);
            Assert.Equal(expectedVid, observedVid);
        }
    }
}