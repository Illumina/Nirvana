using System.Collections.Generic;
using Cache.Data;
using UnitTests.TestUtilities;
using UnitTests.VariantAnnotation.Utilities;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.AnnotatedPositions;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedVariantTests
    {
        [Fact]
        public void GetJsonString_RefMinor_WithTranscripts()
        {
            IVariant variant          = GetRefMinorVariant();
            var      annotatedVariant = new AnnotatedVariant(variant);

            const string originalChromosomeName = "BoB";

            AddRegulatoryRegion(annotatedVariant);
            AddTranscript(annotatedVariant);

            const string expectedResult =
                "{\"vid\":\"bob:100:G\",\"chromosome\":\"BoB\",\"begin\":100,\"end\":200,\"isReferenceMinorAllele\":true,\"refAllele\":\"A\",\"altAllele\":\"G\",\"variantType\":\"SNV\",\"regulatoryRegions\":[{\"id\":\"7157\",\"type\":\"protein_binding_site\",\"consequence\":[\"regulatory_region_amplification\"]}],\"transcripts\":[{\"transcript\":\"ENST00000540021.1\",\"source\":\"Ensembl\",\"bioType\":\"mRNA\",\"codons\":\"-/cAt/cGt\",\"hgnc\":\"ABC\"}]}";
            var observedResult = annotatedVariant.GetJsonString(originalChromosomeName);

            Assert.Equal(expectedResult, observedResult);
        }

        private static void AddRegulatoryRegion(AnnotatedVariant annotatedVariant)
        {
            var regulatoryRegion = new RegulatoryRegion(ChromosomeUtilities.Bob, 103, 104, "7157",
                BioType.protein_binding_site, null, null, null);
            var consequences = new List<ConsequenceTag> {ConsequenceTag.regulatory_region_amplification};

            annotatedVariant.RegulatoryRegions.Add(new AnnotatedRegulatoryRegion(regulatoryRegion, consequences));
        }

        private static void AddTranscript(AnnotatedVariant annotatedVariant)
        {
            var transcript = TranscriptMocker.GetTranscript(false, "ABC", null, null, ChromosomeUtilities.Chr1, 966300,
                966405, Source.Ensembl, "ENST00000540021.1");

            var annotatedTranscript = new AnnotatedTranscript(transcript, null, null,
                null, "cAt/cGt", null, null, null, null,
                null, false);

            annotatedVariant.Transcripts.Add(annotatedTranscript);
        }

        private static IVariant GetRefMinorVariant()
        {
            var behavior = new AnnotationBehavior(false, false, false, false, false);
            return new Variant(ChromosomeUtilities.Bob, 100, 200, "A", "G", VariantType.SNV, "bob:100:G", true, false,
                false,
                new[] {"bob:100:102:TAT"}, null, behavior);
        }
    }
}