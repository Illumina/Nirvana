using System.Collections.Generic;
using Genome;
using Moq;
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
        private readonly IChromosome _chromosome;

        public AnnotatedVariantTests()
        {
            _chromosome = new Chromosome("chrBob", "bob", 3);
        }

        [Fact]
        public void GetJsonString_RefMinor_WithTranscripts()
        {
            IVariant variant = GetRefMinorVariant();
            var annotatedVariant = new AnnotatedVariant(variant);

            const string originalChromosomeName = "BoB";

            AddRegulatoryRegion(annotatedVariant);
            AddTranscript(annotatedVariant);

            const string expectedResult = "{\"vid\":\"bob:100:G\",\"chromosome\":\"BoB\",\"begin\":100,\"end\":200,\"isReferenceMinorAllele\":true,\"refAllele\":\"A\",\"altAllele\":\"G\",\"variantType\":\"SNV\",\"linkedVids\":[\"bob:100:102:TAT\"],\"regulatoryRegions\":[{\"id\":\"7157\",\"type\":\"TF_binding_site\",\"consequence\":[\"regulatory_region_amplification\"]}],\"transcripts\":[]}";
            var observedResult = annotatedVariant.GetJsonString(originalChromosomeName);

            Assert.Equal(expectedResult, observedResult);
        }

        private void AddRegulatoryRegion(IAnnotatedVariant annotatedVariant)
        {
            var regulatoryRegion = new RegulatoryRegion(_chromosome, 103, 104, CompactId.Convert("7157"),
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

        //private static void AddSupplementaryAnnotation(IAnnotatedVariant annotatedVariant)
        //{
        //    var dataSource = new SaDataSource("clinVar", "clinVar", "C", true, false, null,
        //        new[] {"\"good\":\"result\""});
        //    var annotatedSaDataSource = new AnnotatedSaDataSource(dataSource, "C");

        //    var dataSource2 = new SaDataSource("exac", "exac", "G", true, true, null,
        //        new[] { "\"bad\":\"temper\"", "\"brutal\":\"kangaroo\"" });
        //    var annotatedSaDataSource2 = new AnnotatedSaDataSource(dataSource2, "G");

        //    annotatedVariant.SupplementaryAnnotations.Add(annotatedSaDataSource);
        //    annotatedVariant.SupplementaryAnnotations.Add(annotatedSaDataSource2);
        //}

        private IVariant GetRefMinorVariant()
        {
            var behavior = new AnnotationBehavior(false, false, false, false, false);
            return new Variant(_chromosome, 100, 200, "A", "G", VariantType.SNV, "bob:100:G", true, false, false,
                new[] { "bob:100:102:TAT" }, null, behavior);
        }
    }
}
