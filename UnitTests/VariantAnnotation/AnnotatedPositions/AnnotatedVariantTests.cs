using System.Collections.Generic;
using Moq;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedVariantTests
    {
        private const string OriginalChromosomeName = "BoB";
        
        [Fact]
        public void GetJsonString_RefMinor_WithTranscripts()
        {
            IVariant variant = GetRefMinorVariant();
            var annotatedVariant = new AnnotatedVariant(variant);

            AddRegulatoryRegion(annotatedVariant);
            AddTranscript(annotatedVariant);

            const string expectedResult = "{\"vid\":\"bob:100:G\",\"chromosome\":\"BoB\",\"begin\":100,\"end\":200,\"isReferenceMinorAllele\":true,\"refAllele\":\"A\",\"altAllele\":\"G\",\"variantType\":\"SNV\",\"linkedVids\":[\"bob:100:102:TAT\"],\"regulatoryRegions\":[{\"id\":\"7157\",\"type\":\"TF_binding_site\",\"consequence\":[\"regulatory_region_amplification\"]}],\"transcripts\":[]}";
            var observedResult = annotatedVariant.GetJsonString(OriginalChromosomeName);

            Assert.Equal(expectedResult, observedResult);
        }

        [Fact]
        public void GetJsonString_RecomposedSnvAfterTrimming_IsRecomposedTrue()
        {
            IVariant variant = new Variant(ChromosomeUtilities.Bob, 100, 200, "A", "G", VariantType.SNV, "bob-100-A-G", false, false, true,
                new[] { "bob-100-A-G" }, AnnotationBehavior.SmallVariants, false);
            var annotatedVariant = new AnnotatedVariant(variant);

            const string expectedResult = "{\"vid\":\"bob-100-A-G\",\"chromosome\":\"BoB\",\"begin\":100,\"end\":200,\"refAllele\":\"A\",\"altAllele\":\"G\",\"variantType\":\"SNV\",\"isRecomposedVariant\":true,\"linkedVids\":[\"bob-100-A-G\"]}";
            string observedResult = annotatedVariant.GetJsonString(OriginalChromosomeName);

            Assert.Equal(expectedResult, observedResult);
        }

        private void AddRegulatoryRegion(IAnnotatedVariant annotatedVariant)
        {
            var regulatoryRegion = new RegulatoryRegion(ChromosomeUtilities.Bob, 103, 104, CompactId.Convert("7157"),
                RegulatoryRegionType.TF_binding_site);
            var consequences = new List<ConsequenceTag> { ConsequenceTag.regulatory_region_amplification };

            annotatedVariant.RegulatoryRegions.Add(new AnnotatedRegulatoryRegion(regulatoryRegion, consequences));
        }

        private static void AddTranscript(IAnnotatedVariant annotatedVariant)
        {
            var annotatedTranscript = new Mock<IAnnotatedTranscript>();
            annotatedTranscript.SetupGet(x => x.Transcript.Id).Returns(CompactId.Convert("ENST00000540021"));
            annotatedTranscript.SetupGet(x => x.Transcript.Start).Returns(966300);
            annotatedTranscript.SetupGet(x => x.Transcript.End).Returns(966405);
            annotatedTranscript.SetupGet(x => x.AlternateCodons).Returns("cAt/cGt");

            annotatedVariant.Transcripts.Add(annotatedTranscript.Object);
        }

        private IVariant GetRefMinorVariant()
        {
            return new Variant(ChromosomeUtilities.Bob, 100, 200, "A", "G", VariantType.SNV, "bob:100:G", true, false, false,
                new[] { "bob:100:102:TAT" }, AnnotationBehavior.SmallVariants, false);
        }
    }
}
