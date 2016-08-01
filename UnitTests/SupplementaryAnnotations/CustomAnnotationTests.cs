using System.Collections.Generic;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.SupplementaryAnnotations
{
    [Collection("Chromosome 1 collection")]
    public sealed class CustomAnnotationTests
    {
        [Fact]
        public void OutputBasicCustomAnnotation()
        {
            var caReaders = new List<SupplementaryAnnotationReader>
            {
                ResourceUtilities.GetSupplementaryAnnotationReader("chr1_949523_949524_hgmd.nsa", true)
            };

            JsonUtilities.CustomAlleleContains("chr1	949523	.	C	T	.	.	CLASS=DM", caReaders,
                "\"HGMD\":[{\"altAllele\":\"T\",\"isAlleleSpecific\":true,\"mutationCategory\":\"DM\",\"mutantAllele\":\"ALT\",\"gene\":\"ISG15\",\"strand\":\"+\",\"hgvsCoding\":\"NM_005101.3:c.163C>T\",\"hgvsProtein\":\"NP_005092.1:p.Q55*\",\"phenotype\":\"Idiopathic_basal_ganglia_calcification\",\"hgmdAccession\":\"CM1411641\"}]");
        }

        [Fact]
        public void RegularAndCustomAnnotation()
        {
            var caReaders = new List<SupplementaryAnnotationReader>
            {
                ResourceUtilities.GetSupplementaryAnnotationReader("chr1_985955_985955_hgmd.nsa", true)
            };

            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_985955_985956.nsa",
                "chr1	985955	.	G	C	.	.	CLASS=DM;MUT=ALT;GENE=AGRN;STRAND=+;DNA=NM_198576.3:c.5125G>C;PROT=NP_940978.2:p.G1709R;DB=rs199476396;PHEN=Myasthenia;ACC=CM094737",
                caReaders);
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            Assert.NotNull(altAllele);

            Assert.Equal(
				"{\"altAllele\":\"C\",\"refAllele\":\"G\",\"begin\":985955,\"chromosome\":\"chr1\",\"dbsnp\":[\"rs199476396\"],\"end\":985955,\"variantType\":\"SNV\",\"vid\":\"1:985955:C\",\"clinVar\":[{\"id\":\"RCV000019902.29\",\"reviewStatus\":\"no criteria\",\"isAlleleSpecific\":true,\"alleleOrigin\":\"germline\",\"refAllele\":\"G\",\"altAllele\":\"C\",\"phenotype\":\"Myasthenic syndrome, congenital, with pre- and postsynaptic defects\",\"medGenId\":\"C3808739\",\"omimId\":\"615120\",\"significance\":\"pathogenic\",\"lastEvaluatedDate\":\"2009-08-01\",\"pubMedIds\":[\"19631309\"]}],\"HGMD\":[{\"altAllele\":\"C\",\"isAlleleSpecific\":true,\"mutationCategory\":\"DM\",\"mutantAllele\":\"ALT\",\"gene\":\"AGRN\",\"strand\":\"+\",\"hgvsCoding\":\"NM_198576.3:c.5125G>C\",\"hgvsProtein\":\"NP_940978.2:p.G1709R\",\"dbsnp137\":\"rs199476396\",\"phenotype\":\"Myasthenia\",\"hgmdAccession\":\"CM094737\"}]}",
                altAllele);
        }

        [Fact]
        public void CustomAnnotationForIndels()
        {
            var caReaders = new List<SupplementaryAnnotationReader>
            {
                ResourceUtilities.GetSupplementaryAnnotationReader("chr1_11893_13957_internalAF.nsa", true)
            };

            JsonUtilities.CustomAlleleContains("chr1	13289	.	CCT	C	.	.	.", caReaders,
                "InternalAF");
        }

        [Fact]
        public void CustomAnnotationForSnv()
        {
            var caReaders = new List<SupplementaryAnnotationReader>
            {
                ResourceUtilities.GetSupplementaryAnnotationReader("chr1_11893_13957_internalAF.nsa", true)
            };

            JsonUtilities.CustomAlleleContains("chr1	11893	.	T	G	.	.	.", caReaders,
                "InternalAF");
        }

        [Fact]
        public void CustomAnnotationForIndel2()
        {
            var caReaders = new List<SupplementaryAnnotationReader>
            {
                ResourceUtilities.GetSupplementaryAnnotationReader("chr3_1363515_1363516_hgmd.nsa", true)
            };

            JsonUtilities.CustomAlleleContains("chr3	1363515	.	TA	T	.	.	.", caReaders,
                "NM_014461.3:c.944delA");
        }

        [Fact]
        public void MultipleRealCustomAnnotations()
        {
            var caReaders = new List<SupplementaryAnnotationReader>
            {
                ResourceUtilities.GetSupplementaryAnnotationReader("chr1_957605_957605_hgmd.nsa", true),
                ResourceUtilities.GetSupplementaryAnnotationReader("chr1_957605_957605_internalAF.nsa", true)
            };

            var annotatedVariant = DataUtilities.GetVariant(null, null,
                "chr1	957605	.	G	A	.	.	CLASS=DM;MUT=ALT;GENE=AGRN;STRAND=+;DNA=NM_198576.3:c.5125G>C;PROT=NP_940978.2:p.G1709R;DB=rs199476396;PHEN=Myasthenia;ACC=CM094737",
                caReaders);
            Assert.NotNull(annotatedVariant);

            var altAllele = JsonUtilities.GetAllele(annotatedVariant);
            Assert.NotNull(altAllele);

            Assert.Contains("HGMD", altAllele);
            Assert.Contains("InternalAF", altAllele);
        }

	    [Fact]
	    public void CustomAnnotationForRefMinor()
	    {
			var caReaders = new List<SupplementaryAnnotationReader>
			{
				ResourceUtilities.GetSupplementaryAnnotationReader("chr1_1269554_1269554_hgmd.nsa", true),
				ResourceUtilities.GetSupplementaryAnnotationReader("chr1_1269554_1269554_internalAF.nsa", true)
			};

			var annotatedVariant = DataUtilities.GetVariant(null, "chr1_1269554_1269554.nsa",
				"chr1	1269554	.	T	.	.	.	.",
				caReaders);
			Assert.NotNull(annotatedVariant);

			var altAllele = JsonUtilities.GetAllele(annotatedVariant);
			Assert.NotNull(altAllele);

			Assert.Contains("HGMD", altAllele);
			Assert.DoesNotContain("InternalAF", altAllele);
		}

        [Fact]
		public void AlleleSpecificForInternalAF()
		{
			var caReaders = new List<SupplementaryAnnotationReader>
			{
				ResourceUtilities.GetSupplementaryAnnotationReader("chr1_1269554_1269554_hgmd.nsa", true),
				ResourceUtilities.GetSupplementaryAnnotationReader("chr1_1269554_1269554_internalAF.nsa", true)
			};

			var annotatedVariant = DataUtilities.GetVariant(null, "chr1_1269554_1269554.nsa",
				"chr1	1269554	.	T	C	.	.	.",
				caReaders);
			Assert.NotNull(annotatedVariant);

			var altAllele = JsonUtilities.GetAllele(annotatedVariant);
			Assert.NotNull(altAllele);

			Assert.Contains("HGMD", altAllele);
			Assert.Contains("InternalAF", altAllele);
		}
	}
}
