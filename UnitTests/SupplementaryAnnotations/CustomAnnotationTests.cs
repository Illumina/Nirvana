using System.Collections.Generic;
using UnitTests.Utilities;
using Xunit;

namespace UnitTests.SupplementaryAnnotations
{
    public sealed class CustomAnnotationTests
    {
        // MISBEHAVE
        [Fact]
        public void OutputBasicCustomAnnotation()
        {
            JsonUtilities.CustomAlleleContains("chr1	949523	.	C	T	.	.	CLASS=DM",
                null,
                new List<string> { Resources.CustomAnnotations("chr1_949523_949524_hgmd.nsa") },
                "\"HGMD\":[{\"altAllele\":\"T\",\"isAlleleSpecific\":true,\"mutationCategory\":\"DM\",\"mutantAllele\":\"ALT\",\"gene\":\"ISG15\",\"strand\":\"+\",\"hgvsCoding\":\"NM_005101.3:c.163C>T\",\"hgvsProtein\":\"NP_005092.1:p.Q55*\",\"phenotype\":\"Idiopathic_basal_ganglia_calcification\",\"hgmdAccession\":\"CM1411641\"}]");
        }

        [Fact]
        public void RegularAndCustomAnnotation()
        {
            JsonUtilities.CustomAlleleContains(
                "chr1	985955	.	G	C	.	.	CLASS=DM;MUT=ALT;GENE=AGRN;STRAND=+;DNA=NM_198576.3:c.5125G>C;PROT=NP_940978.2:p.G1709R;DB=rs199476396;PHEN=Myasthenia;ACC=CM094737",
                Resources.MiniSuppAnnot("chr1_985955_985956.nsa"),
                new List<string> { Resources.CustomAnnotations("chr1_985955_985955_hgmd.nsa") },
                "{\"altAllele\":\"C\",\"refAllele\":\"G\",\"begin\":985955,\"chromosome\":\"chr1\",\"dbsnp\":[\"rs199476396\"],\"end\":985955,\"variantType\":\"SNV\",\"vid\":\"1:985955:C\",\"clinVar\":[{\"id\":\"RCV000019902.29\",\"reviewStatus\":\"no assertion criteria provided\",\"isAlleleSpecific\":true,\"alleleOrigins\":[\"germline\"],\"refAllele\":\"G\",\"altAllele\":\"C\",\"phenotypes\":[\"Myasthenic syndrome, congenital, 8\"],\"medGenIDs\":[\"C3808739\"],\"omimIDs\":[\"615120\"],\"orphanetIDs\":[\"590\"],\"significance\":\"pathogenic\",\"lastUpdatedDate\":\"2016-08-29\",\"pubMedIds\":[\"19631309\"]},{\"id\":\"RCV000235029.1\",\"reviewStatus\":\"no assertion criteria provided\",\"isAlleleSpecific\":true,\"alleleOrigins\":[\"germline\"],\"refAllele\":\"G\",\"altAllele\":\"C\",\"phenotypes\":[\"Congenital myasthenic syndrome\"],\"medGenIDs\":[\"C0751882\"],\"significance\":\"pathogenic\",\"lastUpdatedDate\":\"2016-08-29\",\"pubMedIds\":[\"19631309\"]}],\"HGMD\":[{\"altAllele\":\"C\",\"isAlleleSpecific\":true,\"mutationCategory\":\"DM\",\"mutantAllele\":\"ALT\",\"gene\":\"AGRN\",\"strand\":\"+\",\"hgvsCoding\":\"NM_198576.3:c.5125G>C\",\"hgvsProtein\":\"NP_940978.2:p.G1709R\",\"dbsnp137\":\"rs199476396\",\"phenotype\":\"Myasthenia\",\"hgmdAccession\":\"CM094737\"}]}");
        }

        [Fact]
        public void CustomAnnotationForIndels()
        {
            JsonUtilities.CustomAlleleContains("chr1	13289	.	CCT	C	.	.	.", null,
                new List<string> { Resources.CustomAnnotations("chr1_11893_13957_internalAF.nsa") },
                "InternalAF");
        }

        [Fact]
        public void CustomAnnotationForSnv()
        {
            JsonUtilities.CustomAlleleContains("chr1	11893	.	T	G	.	.	.", null,
                new List<string> { Resources.CustomAnnotations("chr1_11893_13957_internalAF.nsa") },
                "InternalAF");
        }

        [Fact]
        public void CustomAnnotationForIndel2()
        {
            JsonUtilities.CustomAlleleContains("chr3	1363515	.	TA	T	.	.	.", null,
                new List<string> { Resources.CustomAnnotations("chr3_1363515_1363516_hgmd.nsa") },
                "NM_014461.3:c.944delA");
        }

        [Fact]
        public void MultipleRealCustomAnnotations()
        {
            JsonUtilities.CustomAlleleContains(
                "chr1	957605	.	G	A	.	.	CLASS=DM;MUT=ALT;GENE=AGRN;STRAND=+;DNA=NM_198576.3:c.5125G>C;PROT=NP_940978.2:p.G1709R;DB=rs199476396;PHEN=Myasthenia;ACC=CM094737",
                null,
                new List<string> { Resources.CustomAnnotations("chr1_957605_957605_hgmd.nsa"), Resources.CustomAnnotations("chr1_957605_957605_internalAF.nsa") },
                "HGMD");

            JsonUtilities.CustomAlleleContains(
                "chr1	957605	.	G	A	.	.	CLASS=DM;MUT=ALT;GENE=AGRN;STRAND=+;DNA=NM_198576.3:c.5125G>C;PROT=NP_940978.2:p.G1709R;DB=rs199476396;PHEN=Myasthenia;ACC=CM094737",
                null,
                new List<string> { Resources.CustomAnnotations("chr1_957605_957605_hgmd.nsa"), Resources.CustomAnnotations("chr1_957605_957605_internalAF.nsa") },
                "InternalAF");
        }

        [Fact]
        public void CustomAnnotationForRefMinor()
        {
            JsonUtilities.CustomAlleleContains(
                "chr1	1269554	.	T	.	.	.	.",
                Resources.MiniSuppAnnot("chr1_1269554_1269554.nsa"),
                new List<string> { Resources.CustomAnnotations("chr1_1269554_1269554_hgmd.nsa"), Resources.CustomAnnotations("chr1_1269554_1269554_internalAF.nsa") },
                "HGMD");

            JsonUtilities.CustomAlleleDoesNotContains(
                "chr1	1269554	.	T	.	.	.	.",
                Resources.MiniSuppAnnot("chr1_1269554_1269554.nsa"),
                new List<string> { Resources.CustomAnnotations("chr1_1269554_1269554_hgmd.nsa"), Resources.CustomAnnotations("chr1_1269554_1269554_internalAF.nsa") },
                "InternalAF");
        }

        [Fact]
        public void AlleleSpecificForInternalAF()
        {

            JsonUtilities.CustomAlleleContains(
                "chr1	1269554	.	T	C	.	.	.",
                Resources.MiniSuppAnnot("chr1_1269554_1269554.nsa"),
                new List<string> { Resources.CustomAnnotations("chr1_1269554_1269554_hgmd.nsa"), Resources.CustomAnnotations("chr1_1269554_1269554_internalAF.nsa") },
                "HGMD");

            JsonUtilities.CustomAlleleContains(
                "chr1	1269554	.	T	C	.	.	.",
                Resources.MiniSuppAnnot("chr1_1269554_1269554.nsa"),
                new List<string> { Resources.CustomAnnotations("chr1_1269554_1269554_hgmd.nsa"), Resources.CustomAnnotations("chr1_1269554_1269554_internalAF.nsa") },
                "InternalAF");
        }
    }
}
