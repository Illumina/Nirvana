using Moq;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.TranscriptAnnotation;
using Xunit;

namespace UnitTests.VariantAnnotation.TranscriptAnnotation
{
    public sealed class FullTranscriptAnnotatorTests
    {
        [Theory]
        [InlineData("S", "S", 60, 60, "S", "S", 60, 60)]
        [InlineData("S", "T", 60, 60, "S", "T", 60, 60)]
        [InlineData("ELC", "DVR", 632, 634, "ELC", "DVR", 632, 634)]
        [InlineData("LL", "LI", 213, 214, "L", "I", 214, 214)]
        [InlineData("K", "KLX", 523, 523, "K", "KLX", 523, 523 )]
        [InlineData("C", "CC", 46, 46, "C", "CC", 46, 46)]
        [InlineData("R", "KR", 22955, 22955, "R", "KR", 22955, 22955)]
        [InlineData("PPPPPQQQQ", "", 65, 73, "PPPPPQQQQ", "", 65, 73)]
        [InlineData("DMEIHA", "D", 370, 375, "MEIHA", "", 371, 375)]
        [InlineData("VV", "V", 690, 691, "V", "", 691, 691)]
        [InlineData("NARCN", "N", 243, 247, "ARCN", "", 244, 247)]
        [InlineData("QQQQP", "P", 52, 56, "QQQQ", "", 52, 55)]
        [InlineData("RV", "X", 1172, 1173, "RV", "X", 1172, 1173)]
        [InlineData("GA", "GX", 112, 113, "A", "X", 113, 113)]
        [InlineData("SPDGHE", "R", 566, 571, "SPDGHE", "R", 566, 571)]
        [InlineData("Q", "*VRX", 96, 96, "Q", "*VRX", 96, 96)]
        public void TryTrimAminoAcidsAndUpdateProteinPositions_AsExpected(string reference, string alt, int start, int end, string newReference, string newAlt, int newStart, int newEnd)
        {

            var mappedPositionMock = new Mock<IMappedPosition>();
            mappedPositionMock.SetupProperty(x => x.ProteinStart);
            mappedPositionMock.SetupProperty(x => x.ProteinEnd);
            var mappedPosition = mappedPositionMock.Object;
            mappedPosition.ProteinStart = start;
            mappedPosition.ProteinEnd = end;

            var trimmedAa = FullTranscriptAnnotator.TryTrimAminoAcidsAndUpdateProteinPositions((reference, alt), mappedPosition);

            Assert.Equal(newReference, trimmedAa.Reference);
            Assert.Equal(newAlt, trimmedAa.Alternate);
            Assert.Equal(newStart, mappedPosition.ProteinStart);
            Assert.Equal(newEnd, mappedPosition.ProteinEnd);
        }
    }
}