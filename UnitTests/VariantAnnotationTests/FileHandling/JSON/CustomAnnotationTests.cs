using System.Collections.Generic;
using System.Linq;
using UnitTests.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotationTests.FileHandling.JSON
{
    public sealed class CustomAnnotationTests
    {
        // MISBEHAVE
        [Fact]
        public void OutputBasicCustomAnnotation()
        {
			JsonUtilities.AlleleContains("chr1	949523	.	C	T	.	.	CLASS=DM",
                Resources.CustomAnnotations("chr1_949523_949524_hgmd.nsa"),
                "\"HGMD\":[{\"altAllele\":\"T\",\"mutationCategory\":\"DM\",\"mutantAllele\":\"ALT\",\"gene\":\"ISG15\",\"strand\":\"+\",\"hgvsCoding\":\"NM_005101.3:c.163C>T\",\"hgvsProtein\":\"NP_005092.1:p.Q55*\",\"phenotype\":\"Idiopathic_basal_ganglia_calcification\",\"hgmdAccession\":\"CM1411641\",\"isAlleleSpecific\":true}]");
        }

        [Fact]
        public void RegularAndCustomAnnotation()
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, new List<string> { Resources.MiniSuppAnnot("chr1_985955_985956.nsa"), Resources.CustomAnnotations("chr1_985955_985955_hgmd.nsa") }, "chr1	985955	.	G	C	.	.	CLASS=DM;MUT=ALT;GENE=AGRN;STRAND=+;DNA=NM_198576.3:c.5125G>C;PROT=NP_940978.2:p.G1709R;DB=rs199476396;PHEN=Myasthenia;ACC=CM094737");

            Assert.NotNull(annotatedVariant);
            Assert.Equal(1, annotatedVariant.AnnotatedAlternateAlleles.Count);
            var alleleAnnotations = annotatedVariant.AnnotatedAlternateAlleles[0].SuppAnnotations.Select(x => x.KeyName).ToList();

            Assert.Contains("dbsnp",alleleAnnotations);
            Assert.Contains("clinvar", alleleAnnotations);
            Assert.Contains("HGMD", alleleAnnotations);

        }

        [Fact]
        public void CustomAnnotationForIndels()
        {
			JsonUtilities.AlleleContains("chr1	13289	.	CCT	C	.	.	.",
                Resources.CustomAnnotations("chr1_11893_13957_internalAF.nsa"),
                "internalAF");
        }

        [Fact]
        public void CustomAnnotationForSnv()
        {
		    JsonUtilities.AlleleContains("chr1	11893	.	T	G	.	.	.",
                Resources.CustomAnnotations("chr1_11893_13957_internalAF.nsa"),
                "internalAF");
        }

        [Fact]
        public void CustomAnnotationForIndel2()
        {
		    JsonUtilities.AlleleContains("chr3	1363515	.	TA	T	.	.	.",
                Resources.CustomAnnotations("chr3_1363515_1363516_hgmd.nsa"),
                "NM_014461.3:c.944delA");
        }

        [Fact]
        public void MultipleRealCustomAnnotations()
        {
			JsonUtilities.AlleleContains(
                "chr1	957605	.	G	A	.	.	CLASS=DM;MUT=ALT;GENE=AGRN;STRAND=+;DNA=NM_198576.3:c.5125G>C;PROT=NP_940978.2:p.G1709R;DB=rs199476396;PHEN=Myasthenia;ACC=CM094737",
                new List<string> { Resources.CustomAnnotations("chr1_957605_957605_hgmd.nsa"), Resources.CustomAnnotations("chr1_957605_957605_internalAF.nsa") },
                "HGMD");

			JsonUtilities.AlleleContains(
                "chr1	957605	.	G	A	.	.	CLASS=DM;MUT=ALT;GENE=AGRN;STRAND=+;DNA=NM_198576.3:c.5125G>C;PROT=NP_940978.2:p.G1709R;DB=rs199476396;PHEN=Myasthenia;ACC=CM094737",
                new List<string> { Resources.CustomAnnotations("chr1_957605_957605_hgmd.nsa"), Resources.CustomAnnotations("chr1_957605_957605_internalAF.nsa") },
                "internalAF");
        }

        [Fact]
        public void CustomAnnotationForRefMinor()
        {
            JsonUtilities.AlleleContains("chr1	1269554	.	T	.	.	.	.",
                new List<string>
                {
                    Resources.MiniSuppAnnot("chr1_1269554_1269554.nsa"),
                    Resources.CustomAnnotations("chr1_1269554_1269554_hgmd.nsa"),
                    Resources.CustomAnnotations("chr1_1269554_1269554_internalAF.nsa")
                }, "HGMD");

            JsonUtilities.AlleleContains("chr1	1269554	.	T	.	.	.	.",
                new List<string>
                {
                    Resources.MiniSuppAnnot("chr1_1269554_1269554.nsa"),
                    Resources.CustomAnnotations("chr1_1269554_1269554_hgmd.nsa"),
                    Resources.CustomAnnotations("chr1_1269554_1269554_internalAF.nsa")
                }, "internalAF");
        }

        [Fact]
        public void AlleleSpecificForInternalAF()
        {
            JsonUtilities.AlleleContains("chr1	1269554	.	T	C	.	.	.",
                new List<string>
                {
                    Resources.MiniSuppAnnot("chr1_1269554_1269554.nsa"),
                    Resources.CustomAnnotations("chr1_1269554_1269554_hgmd.nsa"),
                    Resources.CustomAnnotations("chr1_1269554_1269554_internalAF.nsa")
                }, "HGMD");

            JsonUtilities.AlleleDoesNotContain("chr1	1269554	.	T	G	.	.	.",
                new List<string>
                {
                    Resources.MiniSuppAnnot("chr1_1269554_1269554.nsa"),
                    Resources.CustomAnnotations("chr1_1269554_1269554_hgmd.nsa"),
                    Resources.CustomAnnotations("chr1_1269554_1269554_internalAF.nsa")
                }, "InternalAF");
        }
    }
}
