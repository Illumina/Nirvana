using UnitTests.Utilities;
using VariantAnnotation.AnnotationSources;
using Xunit;

namespace UnitTests.Loftee
{
    [Collection("Chromosome 1 collection")]
    public sealed class LofteeFilterTests
    {
        [Theory]
        [InlineData(true, true)]
        [InlineData(false, false)]
        public void LofteeEndTruncation(bool enableLoftee, bool containLoftee)
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000369356_chr1_Ensembl84"), null) as NirvanaAnnotationSource;
            if (enableLoftee) annotationSource?.AddPlugin(new VariantAnnotation.Loftee.Loftee());

            var annotatedVariant = DataUtilities.GetVariant(annotationSource, "chr1\t144852390\t.\tC\tT	.	LowQ	.	.	.");

            Assert.NotNull(annotatedVariant);

            Assert.Equal(containLoftee, annotatedVariant.ToString().Contains("loftee"));
            Assert.Equal(containLoftee, annotatedVariant.ToString().Contains("end_trunc"));
        }

        [Fact]
        public void NagnagSiteForwardStrand()
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000370165_chr1_Ensembl84"), null) as NirvanaAnnotationSource;
            annotationSource?.AddPlugin(new VariantAnnotation.Loftee.Loftee());
            var annotatedVariant = DataUtilities.GetVariant(annotationSource, "chr1\t100316589\t.\tA\tG\t.\tPASS\t.\t.\t.");
            Assert.NotNull(annotatedVariant);

            Assert.Contains("loftee", annotatedVariant.ToString());
            Assert.Contains("nagnag_site", annotatedVariant.ToString());
        }

        [Fact]
        public void SingleExon()
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000600779_chr1_Ensembl84"), null) as NirvanaAnnotationSource;
            annotationSource?.AddPlugin(new VariantAnnotation.Loftee.Loftee());
            var annotatedVariant = DataUtilities.GetVariant(annotationSource, "chr1\t2258668\t.\tGACACAGAAAC\tG\t.\tPASS\t.\t.\t.");
            Assert.NotNull(annotatedVariant);

            Assert.Contains("loftee", annotatedVariant.ToString());
            Assert.Contains("single_exon", annotatedVariant.ToString());
        }

        [Fact]
        public void NonCanonicalSplice()
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000319387_chr1_Ensembl84"), null) as NirvanaAnnotationSource;
            annotationSource?.AddPlugin(new VariantAnnotation.Loftee.Loftee());
            var annotatedVariant = DataUtilities.GetVariant(annotationSource, "chr1	178514165	.	C	T\t.\tPASS\t.\t.\t.");
            Assert.NotNull(annotatedVariant);

            Assert.Contains("loftee", annotatedVariant.ToString());
            Assert.Contains("non_can_splice", annotatedVariant.ToString());
        }

        [Fact]
        public void NonCanonicalSpliceReverseStrand()
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000378156_chr1_Ensembl84"), null) as NirvanaAnnotationSource;
            annotationSource?.AddPlugin(new VariantAnnotation.Loftee.Loftee());
            var annotatedVariant = DataUtilities.GetVariant(annotationSource, "chr1\t5935162\t.\tA\tT\t.\tPASS\t.\t.\t.");
            Assert.NotNull(annotatedVariant);

            Assert.Contains("loftee", annotatedVariant.ToString());
            Assert.Contains("non_can_splice", annotatedVariant.ToString());
        }

        [Fact]
        public void NonCanonicalSpliceSurr()
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000366872_chr1_Ensembl84"), null) as NirvanaAnnotationSource;
            annotationSource?.AddPlugin(new VariantAnnotation.Loftee.Loftee());
            var annotatedVariant = DataUtilities.GetVariant(annotationSource, "chr1	223722780	.	G	A\t.\tPASS\t.\t.\t.");
            Assert.NotNull(annotatedVariant);

            Assert.Contains("loftee", annotatedVariant.ToString());
            Assert.Contains("non_can_splice_surr", annotatedVariant.ToString());
        }

        [Fact]
        public void NotNonCanonicalSpliceSurr()
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000321556_chr1_Ensembl84"), null) as NirvanaAnnotationSource;
            annotationSource?.AddPlugin(new VariantAnnotation.Loftee.Loftee());
            var annotatedVariant = DataUtilities.GetVariant(annotationSource, "chr1	20977005	.	A	AGTT\t.\tPASS\t.\t.\t.");
            Assert.NotNull(annotatedVariant);

            Assert.DoesNotContain("loftee", annotatedVariant.ToString());
        }

        [Fact]
        public void NotNonCanonicalSpliceSurrReverse()
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(Resources.CacheGRCh37("ENST00000467459_chr1_Ensembl84"), null) as NirvanaAnnotationSource;
            annotationSource?.AddPlugin(new VariantAnnotation.Loftee.Loftee());
            var annotatedVariant = DataUtilities.GetVariant(annotationSource, "chr1	45797933	.	G	A\t.\tPASS\t.\t.\t.");
            Assert.NotNull(annotatedVariant);

            Assert.DoesNotContain("loftee", annotatedVariant.ToString());
        }
    }
}