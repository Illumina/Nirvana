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
            var position = new Position(chrom,101,101,"A",new []{"T"},100,null,null,null, inforData, vcfFields);
            var variant = new Variant(chrom,101,101,"A","T",VariantType.SNV,null,false,false,null,null,new AnnotationBehavior(true,false,false,true,false,false));
            var annotatedVariant = new AnnotatedVariant(variant){};
            IAnnotatedVariant[] annotatedVariants = {annotatedVariant};
            var annotatedPosition = new AnnotatedPosition(position,annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.IdIndex];

            Assert.Equal(".",observedVcf);
        }

        [Fact]
        public void Original_dbsnp_rsid_get_removed()
        {
            var vcfFields = "chr1	101	rs123	A	T	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant) { };
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
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant) { };
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
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
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
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
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
            var position = new Position(chrom, 101, 101, "A", new[] { "." }, 100, null, null, null, inforData, vcfFields);
            var variant = new Variant(chrom, 101, 101, "A", ".", VariantType.reference, null, true, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
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
            var position = new Position(chrom, 101, 101, "A", new[] { "." }, 100, null, null, null, inforData, vcfFields);
            var variant = new Variant(chrom, 101, 101, "A", ".", VariantType.reference, null, true, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
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
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
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
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
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
            mockedTranscript1.Setup(x => x.Transcript.Id).Returns(CompactId.Convert("ENST12345"));
            mockedTranscript1.Setup(x => x.Transcript.Version).Returns(1);
            mockedTranscript1.Setup(x => x.Transcript.Gene.Symbol).Returns("testGene1");
            mockedTranscript1.SetupGet(x => x.Consequences)
                .Returns(new List<ConsequenceTag> {ConsequenceTag.five_prime_UTR_variant});

            var mockedTranscript2 = new Mock<IAnnotatedTranscript>();
            mockedTranscript2.Setup(x => x.Transcript.IsCanonical).Returns(false);
            mockedTranscript2.Setup(x => x.Transcript.Id).Returns(CompactId.Convert("ENST23456"));
            mockedTranscript2.Setup(x => x.Transcript.Version).Returns(2);
            mockedTranscript2.Setup(x => x.Transcript.Gene.Symbol).Returns("testGene2");
            mockedTranscript2.SetupGet(x => x.Consequences)
                .Returns(new List<ConsequenceTag> { ConsequenceTag.missense_variant });

            var mockedTranscript3 = new Mock<IAnnotatedTranscript>();
            mockedTranscript3.Setup(x => x.Transcript.IsCanonical).Returns(true);
            mockedTranscript3.Setup(x => x.Transcript.Id).Returns(CompactId.Convert("NM1234.3"));
            mockedTranscript3.Setup(x => x.Transcript.Version).Returns(3);
            mockedTranscript3.Setup(x => x.Transcript.Gene.Symbol).Returns("testGene3");
            mockedTranscript3.SetupGet(x => x.Consequences)
                .Returns(new List<ConsequenceTag> { ConsequenceTag.missense_variant,ConsequenceTag.splice_region_variant });


            var vcfFields = "chr1	101	sa123	A	T	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "T" }, 100, null, null, null, inforData, vcfFields);
            var variant = new Variant(chrom, 101, 101, "A", "T", VariantType.SNV, null, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);
            annotatedVariant.EnsemblTranscripts.Add(mockedTranscript1.Object);
            annotatedVariant.EnsemblTranscripts.Add(mockedTranscript2.Object);
            annotatedVariant.RefSeqTranscripts.Add(mockedTranscript3.Object);

            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("CSQT=1|testGene1|ENST12345.1|5_prime_UTR_variant,1|testGene3|.3|missense_variant&splice_region_variant", observedVcf);

        }

        [Fact]
        public void regulatory_region_is_added()
        {
            var mockedRegulatory = new Mock<IAnnotatedRegulatoryRegion>();
            mockedRegulatory.SetupGet(x => x.Consequences).Returns(new List<ConsequenceTag> { ConsequenceTag.regulatory_region_variant});
            mockedRegulatory.SetupGet(x => x.RegulatoryRegion.Id).Returns(CompactId.Convert("ENSR12345"));

            var vcfFields = "chr1	101	sa123	A	.	.	.	.	.".Split("\t");
            var chrom = new Chromosome("chr1", "1", 0);
            var inforData = new InfoData(null, null, VariantType.SNV, null, null, null, null, null, false, null, null,
                false, false, "Test=abc", null, null);
            var position = new Position(chrom, 101, 101, "A", new[] { "." }, 100, null, null, null, inforData, vcfFields);
            var variant = new Variant(chrom, 101, 101, "A", ".", VariantType.reference, null, false, false, null, null, new AnnotationBehavior(true, false, false, true, false, false));
            var annotatedVariant = new AnnotatedVariant(variant);
            annotatedVariant.RegulatoryRegions.Add(mockedRegulatory.Object);

            IAnnotatedVariant[] annotatedVariants = { annotatedVariant };
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            var converter = new VcfConversion();
            var observedVcf = converter.Convert(annotatedPosition).Split("\t")[VcfCommon.InfoIndex];

            Assert.Equal("Test=abc;CSQR=1|ENSR12345|regulatory_region_variant", observedVcf);
        }

    }
}