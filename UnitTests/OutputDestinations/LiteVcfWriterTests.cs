using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures.VCF;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.OutputDestinations
{
    public sealed class LiteVcfWriterTests
    {
        [Fact]
        public void BlankInfoField()
        {
            VcfUtilities.FieldEquals(null,
                "chr1	24538137	.	C	.	.	PASS	.	GT:GQX:DP:DPF	0/0:99:34:2", ".", VcfCommon.InfoIndex);
        }

        [Fact]
        public void PotentialRefMinor()
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnot("chr17_77263_77265.nsa"));
            VcfUtilities.FieldEquals(saReader,
                "17	77264	.	G	.	428	PASS	END=77264;CIGAR=1M1D;RU=G;REFREP=4;IDREP=3	GT:GQ:GQX:DPI:AD	1/1:33:30:12:0,11",
                "END=77264;CIGAR=1M1D;RU=G;REFREP=4;IDREP=3", VcfCommon.InfoIndex);
        }

        [Fact]
        public void NotPotentialRefMinor()
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnot("chr17_77263_77265.nsa"));
            VcfUtilities.FieldEquals(saReader,
                "17	77264	.	G	.	428	PASS	END=77265;CIGAR=1M1D;RU=G;REFREP=4;IDREP=3	GT:GQ:GQX:DPI:AD	1/1:33:30:12:0,11",
                "END=77265;CIGAR=1M1D;RU=G;REFREP=4;IDREP=3", VcfCommon.InfoIndex);
        }

        [Fact]
        public void FirstAlleleMissingPhylop()
        {
            const string vcfLine = "1	103188976	rs35710136	CTCTA	ATATA,CTCTC	41	PASS	SNVSB=0.0;SNVHPOL=3;AA=.,a;GMAF=A|0.09465,A|0.4898;AF1000G=.,0.510184;phyloP=-0.094	GT:GQ:GQX:DP:DPF:AD	1/2:63:16:12:1:0,7,5";
            var vcfVariant = VcfUtilities.GetVcfVariant(vcfLine);

            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, vcfLine);
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(2, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            DataUtilities.SetConservationScore(altAllele, null);

            var altAllele2 = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(1);
            DataUtilities.SetConservationScore(altAllele2, "-0.094");

            var vcf = new VcfConversion();
            var observedVcfLine = vcf.Convert(vcfVariant, annotatedVariant).Split('\t')[VcfCommon.InfoIndex];
            Assert.Contains("phyloP=.,-0.094", observedVcfLine);
        }

        [Fact]
        public void NoPhylopScores()
        {
            const string vcfLine = "1	103188976	rs35710136	CTCTA	ATATA,CTCTC	41	PASS	SNVSB=0.0;SNVHPOL=3;AA=.,a;GMAF=A|0.09465,A|0.4898;AF1000G=.,0.510184;phyloP=-0.094	GT:GQ:GQX:DP:DPF:AD	1/2:63:16:12:1:0,7,5";
            var vcfVariant = VcfUtilities.GetVcfVariant(vcfLine);

            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, vcfLine);
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckAlleleCount(2, annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            DataUtilities.SetConservationScore(altAllele, null);

            var altAllele2 = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(1);
            DataUtilities.SetConservationScore(altAllele2, null);

            var vcf = new VcfConversion();
            var observedVcfLine = vcf.Convert(vcfVariant, annotatedVariant).Split('\t')[VcfCommon.InfoIndex];
            Assert.DoesNotContain("phyloP", observedVcfLine);
        }

        [Fact]
        public void DbSnpOutputTest()
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnot("chr1_115256529_115256530.nsa"));
            VcfUtilities.FieldContains(saReader,
                "chr1	115256529	.	T	C	1000	PASS	.	GT	0/1", "rs11554290", VcfCommon.IdIndex);
        }

        [Fact]
        public void Missing1KgValues()
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnot("chr17_505249_505250.nsa"));
            VcfUtilities.FieldEquals(saReader,
                "17	505249	.	T	C	35	PASS	SNVSB=0.7;SNVHPOL=5	GT:GQ:GQX:DP:DPF:AD	0/1:34:31:5:1:1,4",
                "SNVSB=0.7;SNVHPOL=5;GMAF=C|0.13;AF1000G=0.129992;cosmic=1|COSN16302644,1|COSN6658016",
                VcfCommon.InfoIndex);
        }

        [Fact]
        public void DuplicateOneKgFreq()
        {
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnot("chr5_29786207_29786208.nsa"));
            VcfUtilities.FieldEquals(saReader,
                "5	29786207	rs150619197	C	.	.	SiteConflict;LowGQX	END=29786207;BLOCKAVG_min30p3a;AF1000G=.,0.994409;GMAF=A|0.9944;RefMinor	GT:GQX:DP:DPF	0:24:9:0",
                "END=29786207;BLOCKAVG_min30p3a;RefMinor;GMAF=C|0.005591", VcfCommon.InfoIndex);
        }

	    [Fact]
	    public void VcfHeaderCheck()
	    {
			var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		    var outputVcfPath = randomPath + ".vcf.gz";
			var dataSourceVersions = new List<DataSourceVersion>
			{
				new DataSourceVersion("VEP","84",0, "Ensembl"),
				new DataSourceVersion("dbSNP","147",Convert.ToDateTime("06/01/2016").Ticks),
				new DataSourceVersion("COSMIC","78",Convert.ToDateTime("09/05/2016").Ticks)
				
			};
		    using (new LiteVcfWriter(outputVcfPath, new List<string> { "##source=SpliceGirl 1.0.0.28", "##reference=file:/illumina/scratch/Zodiac/Software/Jenkins/R2/Genomes/Homo_sapiens/UCSC/hg19/Sequence/WholeGenomeFasta" }, "84.22.34",dataSourceVersions))
		    {
			    
		    }

			var observedHeader = new List<string>();
			using (var reader = GZipUtilities.GetAppropriateStreamReader(outputVcfPath))
		    {			
			    string line;
			    while ((line = reader.ReadLine()) != null)
			    {
				    observedHeader.Add(line);
			    }
		    }
			var expectedHeader= new List<string>
			{
				"##source=SpliceGirl 1.0.0.28",
				"##reference=file:/illumina/scratch/Zodiac/Software/Jenkins/R2/Genomes/Homo_sapiens/UCSC/hg19/Sequence/WholeGenomeFasta",
				"##annotator=Illumina Annotation Engine",
				"##annotatorDataVersion=84.22.34",
				"##annotatorTranscriptSource=Ensembl",
				"##dataSource=dbSNP,version:147,release date:2016-06-01",
				"##dataSource=COSMIC,version:78,release date:2016-09-05"

			};

			Assert.Equal(expectedHeader.Count +10 ,observedHeader.Count); //for info tags added by default
		    for (var i = 0; i < expectedHeader.Count; i++)
		    {
			    if (expectedHeader[i].StartsWith("##annotator=Illumina"))
			    {
				    Assert.Contains(expectedHeader[i], observedHeader[i]);
					continue;
			    }
			    Assert.Equal(expectedHeader[i],observedHeader[i]);
		    }
		}

		[Fact]
		[Trait("jira","NIR-2027")]
		public void ClinvarSignificanceWithComma()
		{
            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader(Resources.MiniSuppAnnot("chrX_76937963_76937963.nsa"));
		    VcfUtilities.FieldContains(saReader, "X	76937963	.	G	C	.	.	.",
		        "conflicting_interpretations_of_pathogenicity\\x2c_not_provided", VcfCommon.InfoIndex);
		}
	}
}