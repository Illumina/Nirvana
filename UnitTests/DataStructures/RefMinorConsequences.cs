using System.Linq;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class RefMinorConsequences : RandomFileBase
    {
        [Fact]
        public void UpstreamGeneVariant()
        {
            var sa = new SupplementaryAnnotation(46107)
            {
                IsRefMinorAllele = true,
                GlobalMajorAllele = "G",
                GlobalMajorAlleleFrequency = "0.9029",
                GlobalMinorAllele = "A",
                GlobalMinorAlleleFrequency = "0.0971"
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            var annotatedVariant = DataUtilities.GetVariant("ENST00000576171_chr17_Ensembl84.ndb", saFilename,
                "17	46107	.	A	.	153	LowGQX	SNVSB=-20.1;SNVHPOL=4;GMAF=G|0.9988;RefMinor;CSQT=A||ENST00000576171|upstream_gene_variant	GT:GQ:GQX:DP:DPF:AD	1/1:18:18:7:0:0,7");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("\"isReferenceMinorAllele\":true", annotatedVariant.ToString());

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault();
            Assert.NotNull(altAllele);

            const string expectedJsonLine = "{\"refAllele\":\"A\",\"begin\":46107,\"chromosome\":\"17\",\"end\":46107,\"globalMinorAllele\":\"A\",\"gmaf\":0.0971,\"isReferenceMinorAllele\":true,\"variantType\":\"SNV\",\"vid\":\"17:46107:A\",\"transcripts\":{\"ensembl\":[{\"transcript\":\"ENST00000576171\",\"geneId\":\"ENSG00000273172\",\"hgnc\":\"AC108004.2\",\"consequence\":[\"upstream_gene_variant\"],\"isCanonical\":true}]}}";
            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal(expectedJsonLine, altAllele.ToString());
        }

        [Fact]
        public void StartLost()
        {
            var sa = new SupplementaryAnnotation(1558792)
            {
                IsRefMinorAllele = true,
                GlobalMajorAllele = "C",
                GlobalMajorAlleleFrequency = "0.9239"
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            var annotatedVariant = DataUtilities.GetVariant("ENST00000487053_chr1_Ensembl84.ndb", saFilename,
                "chr1	1558792	.	T	.	1242.00	PASS	SNVSB=-87.0;SNVHPOL=3;CSQ=C|ENSG00000197530|ENST00000487053|Transcript|initiator_codon_variant&NMD_transcript_variant|353|2|1|M/T|aTg/aCg|||ENST00000487053.1:c.2T>C|ENSP00000424615.1:p.Met1?|deleterious(0)||benign(0.036)||ENSP00000424615||3/21||MIB2|||||");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("\"isReferenceMinorAllele\":true", annotatedVariant.ToString());

            var transcriptAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault()?.EnsemblTranscripts.FirstOrDefault();
            Assert.NotNull(transcriptAllele);

            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal("start_lost&NMD_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void IntergenicVariant()
        {
            var sa = new SupplementaryAnnotation(47960)
            {
                IsRefMinorAllele = true,
                GlobalMajorAllele = "A",
                GlobalMajorAlleleFrequency = "0.9239"
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            var annotatedVariant = DataUtilities.GetVariant("ENST00000518655_chr1_Ensembl84.ndb", saFilename,
                "chr1	47960	.	T	.	1242.00	PASS	SNVSB=-87.0;SNVHPOL=3");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("\"isReferenceMinorAllele\":true", annotatedVariant.ToString());

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault();
            Assert.NotNull(altAllele);

            // intergenic variant are not exposed in the JSON file
            AssertUtilities.CheckEnsemblTranscriptCount(0, altAllele);
            AssertUtilities.CheckRefSeqTranscriptCount(0, altAllele);
        }

        [Fact]
        public void IntronVariant()
        {
            var sa = new SupplementaryAnnotation(13302)
            {
                IsRefMinorAllele = true,
                GlobalMajorAllele = "T",
                GlobalMajorAlleleFrequency = "0.9239"
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            var annotatedVariant = DataUtilities.GetVariant("ENST00000518655_chr1_Ensembl84.ndb", saFilename,
                "chr1	13302	.	C	.	1242.00	PASS	SNVSB=-87.0;SNVHPOL=3");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("\"isReferenceMinorAllele\":true", annotatedVariant.ToString());

            var transcriptAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault()?.EnsemblTranscripts.FirstOrDefault();
            Assert.NotNull(transcriptAllele);

            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal("intron_variant&non_coding_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void NoConsequences()
        {
            var annotatedVariant = DataUtilities.GetVariant("chr1_115256529_G_TAA_RefSeq84_pos.ndb",
                "chr1	115256529	.	T	.	.	PASS	.	GT:GQX:DP:DPF	0/0:99:34:2");
            Assert.NotNull(annotatedVariant);
            Assert.DoesNotContain("\"isReferenceMinorAllele\":true", annotatedVariant.ToString());

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault();
            Assert.NotNull(altAllele);

            // intergenic variant are not exposed in the JSON file
            AssertUtilities.CheckEnsemblTranscriptCount(0, altAllele);
            AssertUtilities.CheckRefSeqTranscriptCount(0, altAllele);
        }

        [Fact]
        public void SpliceVariants()
        {
            var sa = new SupplementaryAnnotation(889158)
            {
                IsRefMinorAllele = true,
                GlobalMajorAllele = "G",
                GlobalMajorAlleleFrequency = "0.8239"
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            var annotatedVariant = DataUtilities.GetVariant("ENST00000327044_chr1_Ensembl84.ndb", saFilename,
                "chr1	889158	.	C	.	1243.00	PASS	SNVSB=-130.5;SNVHPOL=3;CSQ=C|ENSG00000188976|ENST00000327044|Transcript|splice_region_variant&intron_variant|||||||CCDS3.1|ENST00000327044.6:c.888+4C>G|||YES|||ENSP00000317992|||8/18|NOC2L||||| GT:GQ:GQX:DP:DPF:AD     1/1:313:26:105:9:0,105");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("\"isReferenceMinorAllele\":true", annotatedVariant.ToString());

            var transcriptAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault()?.EnsemblTranscripts.FirstOrDefault();
            Assert.NotNull(transcriptAllele);

            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal("splice_region_variant&intron_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void StopLost()
        {
            var sa = new SupplementaryAnnotation(26879920)
            {
                IsRefMinorAllele = true,
                GlobalMajorAllele = "T",
                GlobalMajorAlleleFrequency = "0.8239"
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            var annotatedVariant = DataUtilities.GetVariant("ENST00000374163_chr1_Ensembl84.ndb", saFilename,
                "chr1	26879920	.	C	.	265.00	PASS	SNVSB=-31.9;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	0/1:254:40:56:1:26,30");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("\"isReferenceMinorAllele\":true", annotatedVariant.ToString());

            var transcriptAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault()?.EnsemblTranscripts.FirstOrDefault();
            Assert.NotNull(transcriptAllele);

            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal("stop_lost&NMD_transcript_variant", string.Join("&", transcriptAllele.Consequence));
        }

        [Fact]
        public void SynonymousVariant()
        {
            var sa = new SupplementaryAnnotation(115258746)
            {
                IsRefMinorAllele = true,
                GlobalMajorAllele = "G",
                GlobalMajorAlleleFrequency = "0.8239"
            };

            var saFilename = GetRandomPath(true);
            SupplementaryAnnotationUtilities.Write(sa, "chr1", saFilename);

            var annotatedVariant = DataUtilities.GetVariant("ENST00000369535_chr1_Ensembl84.ndb", saFilename,
                "chr1	115258746	.	A	.	265.00	PASS	SNVSB=-31.9;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	0/1:254:40:56:1:26,30");
            Assert.NotNull(annotatedVariant);
            Assert.Contains("\"isReferenceMinorAllele\":true", annotatedVariant.ToString());

            var transcriptAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault()?.EnsemblTranscripts.FirstOrDefault();
            Assert.NotNull(transcriptAllele);

            // ReSharper disable once PossibleNullReferenceException
            Assert.Equal("synonymous_variant", string.Join("&", transcriptAllele.Consequence));
        }
    }
}