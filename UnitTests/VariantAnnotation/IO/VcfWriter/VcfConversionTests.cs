using System.Collections.Generic;
using VariantAnnotation.AnnotatedPositions;
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
            var vcfFields = "chr1	101	sa123	A	.	.	.	.".Split("\t");
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
        public void OneKGAnnotation_is_handled()
        {
            
        }

        [Fact]
        public void Only_allele_specific_entry_for_annotation_not_matched_by_allele_is_output()
        {
            
        }

    }
}