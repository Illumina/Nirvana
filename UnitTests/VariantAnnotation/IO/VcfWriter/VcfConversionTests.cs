using System.Collections.Generic;
using Moq;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.IO.VcfWriter;
using VariantAnnotation.SA;
using VariantAnnotation.Sequence;
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
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields, new []{false}, false);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);
            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.IdIndex];

            Assert.Equal(".", observedVcf);
        }

        [Fact]
        public void Original_dbsnp_rsid_get_removed()
        {
            var vcfFields = "chr1	101	rs123	A	T	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields, new[]{ false }, false);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);
            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.IdIndex];

            Assert.Equal(".", observedVcf);
        }

        [Fact]
        public void Original_dbsnp_nonrsid_get_kept()
        {
            var vcfFields = "chr1	101	sa123	A	T	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields, new[] { false }, false);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);
            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.IdIndex];

            Assert.Equal("sa123", observedVcf);
        }

        [Fact]
        public void Original_dbsnp_nonrsid_merged_with_rsid_from_dbsnp()
        {
            var vcfFields = "chr1	101	sa123	A	T	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields, new[] { false }, false);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);

            annotatedVariant.SupplementaryAnnotations.Add(new AnnotatedSaDataSource(new SaDataSource("dbsnp", "", "T", true, false, "rs456", null), "T"));
            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.IdIndex];

            Assert.Equal("sa123;rs456", observedVcf);
        }


        [Fact]
        public void Gmaf_outputed()
        {
            var vcfFields = "chr1	101	sa123	A	T	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields, new[] { false }, false);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);

            annotatedVariant.SupplementaryAnnotations.Add(new AnnotatedSaDataSource(new SaDataSource("globalAllele", "GMAF", "N", false, false, "G|0.002", null), "N"));
            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("GMAF=G|0.002", observedVcf);
        }



        [Fact]
        public void RefMinor_tag_is_added_to_info_field()
        {
            var vcfFields = "chr1	101	sa123	A	.	.	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "." }, 100, null, null, null, inforData, vcfFields, new[] { false }, false);
            var variant = new Variant(chrom, 101, 101, "A", ".", VariantType.reference, null, true, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);

            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("RefMinor", observedVcf);
        }

        [Fact]
        public void original_updated_info_is_added_to_info_field()
        {
            var vcfFields = "chr1	101	sa123	A	.	.	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "Test=abc", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "." }, 100, null, null, null, inforData, vcfFields, new[] { false }, false);
            var variant = new Variant(chrom, 101, 101, "A", ".", VariantType.reference, null, true, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);

            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("Test=abc;RefMinor", observedVcf);
        }

        [Fact]
        public void OneKGAnnotation_is_handled()
        {
            var vcfFields = "chr1	101	sa123	A	T	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields, new[] { false }, false);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);

            annotatedVariant.SupplementaryAnnotations.Add(new AnnotatedSaDataSource(new SaDataSource("oneKg", "AF1000G", "T", true, false, "0.000599;t", null), "T"));
            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("AA=t;AF1000G=0.000599", observedVcf);
        }

        [Fact]
        public void Only_allele_specific_entry_for_annotation_not_matched_by_allele_is_output()
        {
            var vcfFields = "chr1	101	sa123	A	T	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields, new[] { false }, false);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);

            annotatedVariant.SupplementaryAnnotations.Add(new AnnotatedSaDataSource(new SaDataSource("testSource", "Test", "T", false, true, "pathogenic", null), "T"));
            annotatedVariant.SupplementaryAnnotations.Add(new AnnotatedSaDataSource(new SaDataSource("testSource", "Test", "G", false, true, "benign", null), "T"));
            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("Test=1|pathogenic", observedVcf);
        }

        [Fact]
        public void Only_canonical_transcripts_are_reported_in_vcf()
        {
            var mockedTranscript1 = new Mock<IAnnotatedTranscript>();
            mockedTranscript1.Setup(x => x.Transcript.IsCanonical).Returns(true);
            mockedTranscript1.Setup(x => x.Transcript.Id).Returns(CompactId.Convert("ENST12345", 1));
            mockedTranscript1.Setup(x => x.Transcript.Gene.Symbol).Returns("testGene1");
            mockedTranscript1.SetupGet(x => x.Consequences)
                .Returns(new List<ConsequenceTag> { ConsequenceTag.five_prime_UTR_variant });

            var mockedTranscript2 = new Mock<IAnnotatedTranscript>();
            mockedTranscript2.Setup(x => x.Transcript.IsCanonical).Returns(false);
            mockedTranscript2.Setup(x => x.Transcript.Id).Returns(CompactId.Convert("ENST23456", 2));
            mockedTranscript2.Setup(x => x.Transcript.Gene.Symbol).Returns("testGene2");
            mockedTranscript2.SetupGet(x => x.Consequences)
                .Returns(new List<ConsequenceTag> { ConsequenceTag.missense_variant });

            var mockedTranscript3 = new Mock<IAnnotatedTranscript>();
            mockedTranscript3.Setup(x => x.Transcript.IsCanonical).Returns(true);
            mockedTranscript3.Setup(x => x.Transcript.Id).Returns(CompactId.Convert("NM_1234", 3));
            mockedTranscript3.Setup(x => x.Transcript.Gene.Symbol).Returns("testGene3");
            mockedTranscript3.SetupGet(x => x.Consequences)
                .Returns(new List<ConsequenceTag> { ConsequenceTag.missense_variant, ConsequenceTag.splice_region_variant });


            var vcfFields = "chr1	101	sa123	A	T	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields, new[] { false }, false);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);
            annotatedVariant.EnsemblTranscripts.Add(mockedTranscript1.Object);
            annotatedVariant.EnsemblTranscripts.Add(mockedTranscript2.Object);
            annotatedVariant.RefSeqTranscripts.Add(mockedTranscript3.Object);

            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("CSQT=1|testGene1|ENST12345.1|5_prime_UTR_variant,1|testGene3|NM_1234.3|missense_variant&splice_region_variant", observedVcf);
        }

        [Fact]
        public void regulatory_region_is_added()
        {
            var mockedRegulatory = new Mock<IAnnotatedRegulatoryRegion>();
            mockedRegulatory.SetupGet(x => x.Consequences).Returns(new List<ConsequenceTag> { ConsequenceTag.regulatory_region_variant });
            mockedRegulatory.SetupGet(x => x.RegulatoryRegion.Id).Returns(CompactId.Convert("ENSR12345"));

            var vcfFields = "chr1	101	sa123	A	.	.	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "Test=abc", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "." }, 100, null, null, null, inforData, vcfFields, new[] { false }, false);
            var variant = new Variant(chrom, 101, 101, "A", ".", VariantType.reference, null, false, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);
            annotatedVariant.RegulatoryRegions.Add(mockedRegulatory.Object);

            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("Test=abc;CSQR=1|ENSR12345|regulatory_region_variant", observedVcf);
        }

        [Fact]
        public void GenotypeIndex_is_correct_w_nonInformative_altAlleles_filtered()
        {
            var vcfFields1 = "chr1	101	sa123	A	<*>,T	.	.	.".Split("\t");
            var vcfFields2 = "chr1	101	sa123	A	<M>,T	.	.	.".Split("\t");
            var vcfFields3 = "chr1	101	sa123	A	*,T	.	.	.".Split("\t");
            var vcfFields4 = "chr1	101	sa123	A	<NON_REF>,T	.	.	.".Split("\t");
            var vcfFields5 = "chr1	101	sa123	A	T,<*>	.	.	.".Split("\t");
            var vcfFields6 = "chr1	101	sa123	A	T,<M>	.	.	.".Split("\t");
            var vcfFields7 = "chr1	101	sa123	A	T,*	.	.	.".Split("\t");
            var vcfFields8 = "chr1	101	sa123	A	T,<NON_REF>	.	.	.".Split("\t");

            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position1 = new Position(chrom, 101, 101, "A", new[] { "<*>", "T" }, 100, null, null, null, inforData, vcfFields1, new[] { false }, false);
            var position2 = new Position(chrom, 101, 101, "A", new[] { "<M>", "T" }, 100, null, null, null, inforData, vcfFields2, new[] { false }, false);
            var position3 = new Position(chrom, 101, 101, "A", new[] { "*", "T" }, 100, null, null, null, inforData, vcfFields3, new[] { false }, false);
            var position4 = new Position(chrom, 101, 101, "A", new[] { "<NON_REF>", "T" }, 100, null, null, null, inforData, vcfFields4, new[] { false }, false);
            var position5 = new Position(chrom, 101, 101, "A", new[] { "T", "<*>" }, 100, null, null, null, inforData, vcfFields5, new[] { false }, false);
            var position6 = new Position(chrom, 101, 101, "A", new[] { "T", "<M>" }, 100, null, null, null, inforData, vcfFields6, new[] { false }, false);
            var position7 = new Position(chrom, 101, 101, "A", new[] { "T", "*" }, 100, null, null, null, inforData, vcfFields7, new[] { false }, false);
            var position8 = new Position(chrom, 101, 101, "A", new[] { "T", "<NON_REF>" }, 100, null, null, null, inforData, vcfFields8, new[] { false }, false);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);

            annotatedVariant.SupplementaryAnnotations.Add(new AnnotatedSaDataSource(new SaDataSource("testSource", "Test", "T", false, true, "pathogenic", null), "T"));
            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition1 = new AnnotatedPosition(position1, annotatedVariants);
            var annotatedPosition2 = new AnnotatedPosition(position2, annotatedVariants);
            var annotatedPosition3 = new AnnotatedPosition(position3, annotatedVariants);
            var annotatedPosition4 = new AnnotatedPosition(position4, annotatedVariants);
            var annotatedPosition5 = new AnnotatedPosition(position5, annotatedVariants);
            var annotatedPosition6 = new AnnotatedPosition(position6, annotatedVariants);
            var annotatedPosition7 = new AnnotatedPosition(position7, annotatedVariants);
            var annotatedPosition8 = new AnnotatedPosition(position8, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf1 = converter.Convert(annotatedPosition1).Split("\t")[VcfCommon.InfoIndex];
            var observedVcf2 = converter.Convert(annotatedPosition2).Split("\t")[VcfCommon.InfoIndex];
            var observedVcf3 = converter.Convert(annotatedPosition3).Split("\t")[VcfCommon.InfoIndex];
            var observedVcf4 = converter.Convert(annotatedPosition4).Split("\t")[VcfCommon.InfoIndex];
            var observedVcf5 = converter.Convert(annotatedPosition5).Split("\t")[VcfCommon.InfoIndex];
            var observedVcf6 = converter.Convert(annotatedPosition6).Split("\t")[VcfCommon.InfoIndex];
            var observedVcf7 = converter.Convert(annotatedPosition7).Split("\t")[VcfCommon.InfoIndex];
            var observedVcf8 = converter.Convert(annotatedPosition8).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("Test=2|pathogenic", observedVcf1);
            Assert.Equal("Test=2|pathogenic", observedVcf2);
            Assert.Equal("Test=2|pathogenic", observedVcf3);
            Assert.Equal("Test=2|pathogenic", observedVcf4);
            Assert.Equal("Test=1|pathogenic", observedVcf5);
            Assert.Equal("Test=1|pathogenic", observedVcf6);
            Assert.Equal("Test=1|pathogenic", observedVcf7);
            Assert.Equal("Test=1|pathogenic", observedVcf8);
        }

        [Fact]
        public void GenotypeIndex_is_correct_w_refMinor_allele()
        {
            var vcfFields1 =
                "1	10628385	.	C	<NON_REF>	.	LowGQX;HighDPFRatio	END=10628385;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	0/0:24:9:18".Split('\t');
            var vcfFields2 =
                "1	10628385	.	C	<NON_REF>	.	LowGQX;HighDPFRatio	END=10628385;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	0/0:24:9:18".Split('\t');
            var chromosome = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position1 = new Position(chromosome, 10628385, 10628385, "C", new[] { "." }, 100, null, null, null, inforData, vcfFields1, new[] { false }, false);
            var position2 = new Position(chromosome, 10628385, 10628385, "C", new[] { "." }, 100, null, null, null, inforData, vcfFields2, new[] { false }, false);
            var variant = new Variant(chromosome, 10628385, 10628385, "C", "<NON_REF>", VariantType.reference, null, true, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);
            annotatedVariant.SupplementaryAnnotations.Add(new AnnotatedSaDataSource(new SaDataSource("testSource", "Test", "C", false, true, "pathogenic", null), "C"));

            var annotatedPosition1 = new AnnotatedPosition(position1, new IAnnotatedVariant[] { annotatedVariant });
            var annotatedPosition2 = new AnnotatedPosition(position2, new IAnnotatedVariant[] { annotatedVariant });
            var converter = new VcfConversion();
            var observedVcf1 = converter.Convert(annotatedPosition1).Split("\t")[VcfCommon.InfoIndex];
            var observedVcf2 = converter.Convert(annotatedPosition2).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("RefMinor;Test=1|pathogenic", observedVcf1);
            Assert.Equal("RefMinor;Test=1|pathogenic", observedVcf2);
        }
    }
}
