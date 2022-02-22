using System.Collections.Generic;
using Cache.Data;
using UnitTests.TestUtilities;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO.VcfWriter;
using Variants;
using Vcf;
using Vcf.Info;
using Xunit;

namespace UnitTests.VariantAnnotation.IO.VcfWriter
{
    public sealed class VcfConversionTests
    {
        [Fact]
        public void No_dbsnp_id()
        {
            var vcfFields = "chr1	101	.	A	T	.	.	.".Split("\t");
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(ChromosomeUtilities.Chr1, 101, 101, "A", new[] {"T"}, 100, null, null, null,
                inforData, vcfFields, new[] {false}, false);
            var variant = new Variant(ChromosomeUtilities.Chr1, 101, 101, "A", "T", VariantType.SNV, null, false, false,
                false, null, null, new AnnotationBehavior(true, false, false, true, false));
            var                annotatedVariant  = new AnnotatedVariant(variant);
            AnnotatedVariant[] annotatedVariants = {annotatedVariant};
            var                annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter   = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.IdIndex];

            Assert.Equal(".", observedVcf);
        }

        [Fact]
        public void Original_dbsnp_rsid_get_removed()
        {
            var vcfFields = "chr1	101	rs123	A	T	.	.	.".Split("\t");
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(ChromosomeUtilities.Chr1, 101, 101, "A", new[] {"T"}, 100, null, null, null,
                inforData, vcfFields, new[] {false}, false);
            var variant = new Variant(ChromosomeUtilities.Chr1, 101, 101, "A", "T", VariantType.SNV, null, false, false,
                false, null, null, new AnnotationBehavior(true, false, false, true, false));
            var                annotatedVariant  = new AnnotatedVariant(variant);
            AnnotatedVariant[] annotatedVariants = {annotatedVariant};
            var                annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter   = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.IdIndex];

            Assert.Equal(".", observedVcf);
        }

        [Fact]
        public void Original_dbsnp_nonrsid_get_kept()
        {
            var vcfFields = "chr1	101	sa123	A	T	.	.	.".Split("\t");
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(ChromosomeUtilities.Chr1, 101, 101, "A", new[] {"T"}, 100, null, null, null,
                inforData, vcfFields, new[] {false}, false);
            var variant = new Variant(ChromosomeUtilities.Chr1, 101, 101, "A", "T", VariantType.SNV, null, false, false,
                false, null, null, new AnnotationBehavior(true, false, false, true, false));
            var                annotatedVariant  = new AnnotatedVariant(variant);
            AnnotatedVariant[] annotatedVariants = {annotatedVariant};
            var                annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter   = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.IdIndex];

            Assert.Equal("sa123", observedVcf);
        }

        [Fact]
        public void Phylop_positional()
        {
            var vcfFields = "chr1	101	sa123	A	T	.	.	.".Split("\t");
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(ChromosomeUtilities.Chr1, 101, 101, "A", new[] {"T"}, 100, null, null, null,
                inforData, vcfFields, new[] {false}, false);
            var variant = new Variant(ChromosomeUtilities.Chr1, 101, 101, "A", "T", VariantType.SNV, null, false, false,
                false, null, null, new AnnotationBehavior(true, false, false, true, false));
            var annotatedVariant = new AnnotatedVariant(variant) {PhylopScore = -0.567};

            AnnotatedVariant[] annotatedVariants = {annotatedVariant};
            var                annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter   = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("phyloP=-0.567", observedVcf);
        }


        [Fact]
        public void RefMinor_tag_is_added_to_info_field()
        {
            var vcfFields = "chr1	101	sa123	A	.	.	.	.	.".Split("\t");
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(ChromosomeUtilities.Chr1, 101, 101, "A", new[] {"."}, 100, null, null, null,
                inforData, vcfFields, new[] {false}, false);
            var variant = new Variant(ChromosomeUtilities.Chr1, 101, 101, "A", ".", VariantType.reference, null, true,
                false, false, null, null, new AnnotationBehavior(true, false, false, true, false));
            var annotatedVariant = new AnnotatedVariant(variant);

            AnnotatedVariant[] annotatedVariants = {annotatedVariant};
            var                annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter   = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("RefMinor", observedVcf);
        }

        [Fact]
        public void original_updated_info_is_added_to_info_field()
        {
            var vcfFields = "chr1	101	sa123	A	.	.	.	.	.".Split("\t");
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "Test=abc", null, null);
            var position = new Position(ChromosomeUtilities.Chr1, 101, 101, "A", new[] {"."}, 100, null, null, null,
                inforData, vcfFields, new[] {false}, false);
            var variant = new Variant(ChromosomeUtilities.Chr1, 101, 101, "A", ".", VariantType.reference, null, true,
                false, false, null, null, new AnnotationBehavior(true, false, false, true, false));
            var annotatedVariant = new AnnotatedVariant(variant);

            AnnotatedVariant[] annotatedVariants = {annotatedVariant};
            var                annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter   = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("Test=abc;RefMinor", observedVcf);
        }

        [Fact]
        public void Only_canonical_transcripts_are_reported_in_vcf()
        {
            var gene1 = new Gene("1", "ENSG1", false, null) {Symbol = "testGene1"};
            var gene2 = new Gene("2", "ENSG2", false, null) {Symbol = "testGene2"};
            var gene3 = new Gene("3", "ENSG3", false, null) {Symbol = "testGene3"};

            var transcript1 = new Transcript(ChromosomeUtilities.Chr1, 1, 2, "ENST12345.1", BioType.mRNA, true,
                Source.Ensembl, gene1, null, null, null);
            var transcript2 = new Transcript(ChromosomeUtilities.Chr1, 1, 2, "ENST23456.2", BioType.mRNA, false,
                Source.Ensembl, gene2, null, null, null);
            var transcript3 = new Transcript(ChromosomeUtilities.Chr1, 1, 2, "NM_1234.3", BioType.mRNA, true,
                Source.RefSeq, gene3, null, null, null);

            var annotatedTranscript1 = new AnnotatedTranscript(transcript1, null, null, null, null, null, null, null,
                new List<ConsequenceTag> {ConsequenceTag.five_prime_UTR_variant}, null, false);
            var annotatedTranscript2 = new AnnotatedTranscript(transcript2, null, null, null, null, null, null, null,
                new List<ConsequenceTag> {ConsequenceTag.missense_variant}, null, false);
            var annotatedTranscript3 = new AnnotatedTranscript(transcript3, null, null, null, null, null, null, null,
                new List<ConsequenceTag> {ConsequenceTag.missense_variant, ConsequenceTag.splice_region_variant}, null,
                false);

            var vcfFields = "chr1	101	sa123	A	T	.	.	.".Split("\t");
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(ChromosomeUtilities.Chr1, 101, 101, "A", new[] {"T"}, 100, null, null, null,
                inforData, vcfFields, new[] {false}, false);
            var variant = new Variant(ChromosomeUtilities.Chr1, 101, 101, "A", "T", VariantType.SNV, null, false, false,
                false, null, null, new AnnotationBehavior(true, false, false, true, false));
            var annotatedVariant = new AnnotatedVariant(variant);
            annotatedVariant.Transcripts.Add(annotatedTranscript1);
            annotatedVariant.Transcripts.Add(annotatedTranscript2);
            annotatedVariant.Transcripts.Add(annotatedTranscript3);

            AnnotatedVariant[] annotatedVariants = {annotatedVariant};
            var                annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter   = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal(
                "CSQT=1|testGene1|ENST12345.1|5_prime_UTR_variant,1|testGene3|NM_1234.3|missense_variant&splice_region_variant",
                observedVcf);
        }

        [Fact]
        public void regulatory_region_is_added()
        {
            var regulatoryRegion = new RegulatoryRegion(ChromosomeUtilities.Chr1, 100, 200, "ENSR12345",
                BioType.enhancer, null, null, null);

            AnnotatedRegulatoryRegion annotatedRegulatoryRegion =
                new AnnotatedRegulatoryRegion(regulatoryRegion,
                    new List<ConsequenceTag> {ConsequenceTag.regulatory_region_variant});


            var vcfFields = "chr1	101	sa123	A	.	.	.	.	.".Split("\t");
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "Test=abc", null, null);
            var position = new Position(ChromosomeUtilities.Chr1, 101, 101, "A", new[] {"."}, 100, null, null, null,
                inforData, vcfFields, new[] {false}, false);
            var variant = new Variant(ChromosomeUtilities.Chr1, 101, 101, "A", ".", VariantType.reference, null, false,
                false, false, null, null, new AnnotationBehavior(true, false, false, true, false));
            var annotatedVariant = new AnnotatedVariant(variant);
            annotatedVariant.RegulatoryRegions.Add(annotatedRegulatoryRegion);

            AnnotatedVariant[] annotatedVariants = {annotatedVariant};
            var                annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter   = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("Test=abc;CSQR=1|ENSR12345|regulatory_region_variant", observedVcf);
        }
    }
}