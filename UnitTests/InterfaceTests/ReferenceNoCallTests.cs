using UnitTests.Utilities;
using VariantAnnotation.AnnotationSources;
using Xunit;

namespace UnitTests.InterfaceTests
{
    public class ReferenceNoCallTests
    {
        [Theory]
        [InlineData(false, false, 0)]
        [InlineData(true, false, 1)]
        [InlineData(true, true, 0)]
        public void ReferenceNoCallsNotOverlapTranscript(bool enableRefNoCall, bool limitToTranscript, int numberOfAnnotatedAlleles)
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000483270_chr1_Ensembl84"), null) as NirvanaAnnotationSource;

            if (enableRefNoCall)
                annotationSource?.EnableReferenceNoCalls(limitToTranscript);

            var annotatedVariant = DataUtilities.GetVariant(annotationSource,
                "1	10360	.	C	.	.	LowQ	END=10362	.	.");

            Assert.NotNull(annotatedVariant);
            AssertUtilities.CheckAlleleCount(numberOfAnnotatedAlleles, annotatedVariant);
        }

        [Theory]
        [InlineData(false, false, 0)]
        [InlineData(true, false, 1)]
        [InlineData(true, true, 1)]
        public void ReferenceNoCallsOverlapTranscript(bool enableRefNoCall, bool limitToTranscript, int numberOfAnnotatedAlleles)
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000483270_chr1_Ensembl84"), null) as NirvanaAnnotationSource;

            if (enableRefNoCall)
                annotationSource?.EnableReferenceNoCalls(limitToTranscript);

            var annotatedVariant = DataUtilities.GetVariant(annotationSource,
                "1	15886104	.	C	.	.	LowQ	END=15890000	.	.");

            Assert.NotNull(annotatedVariant);
            AssertUtilities.CheckAlleleCount(numberOfAnnotatedAlleles, annotatedVariant);
        }
    }
}