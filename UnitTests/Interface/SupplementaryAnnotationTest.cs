using System.IO;
using System.Linq;
using System.Reflection;
using UnitTests.Utilities;
using VariantAnnotation.AnnotationSources;
using VariantAnnotation.Interface;
using Xunit;

namespace UnitTests.Interface
{
    [Collection("Chromosome 1 collection")]

    public class SupplementaryAnnotationTest
    {
        [Fact]
        public void SuppIntervalCnv()
        {
            var annotatedVariant = DataUtilities.GetSuppIntervalVariant("chr1_713035_713060.nsa",
                "chr1	713045	.	T	<CN1>	.	LowGQX;HighDPFRatio	END=718045;SVTYPE=CNV;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	.:.:0:1");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckIntervalCount(28, annotatedVariant);
            AssertUtilities.CheckJsonContains("variantType\":\"copy_number_variation\"", annotatedVariant);
        }

        [Fact]
        public void RefMinorGrch38()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, Path.Combine("hg38", "chr1_13369329_13369329.nsa"),
                "chr1	13369329	.	C	.	.	LowQscore	SOMATIC;QSS=0;TQSS=1;NT=ref;QSS_NT=0;TQSS_NT=1;SGT=CC->CC;DP=122;MQ=54.94;MQ0=8;ALTPOS=0;ALTMAP=0;ReadPosRankSum=0.00;SNVSB=3.85;PNOISE=0.00;PNOISE2=0.00;VQSR=0.96");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            Assert.False(altAllele.IsReferenceMinor);
        }

        [Fact]
        public void ClinVarUnknownAllele()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr13_40298637_40298638.nsa",
                "chr1	40298637	.	TTA	T	222	PASS	CIGAR=1M2D");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            Assert.Equal("chr1", altAllele.ReferenceName);
            Assert.Equal(40298638, altAllele.ReferenceBegin);
            Assert.Equal("deletion", altAllele.VariantType);
            Assert.Equal("1:40298638:40298639", altAllele.VariantId);

            AssertUtilities.CheckAlleleCoverage(altAllele, 45, "ExacCoverage");
            AssertUtilities.CheckAlleleFrequencies(altAllele, 0.073456, "ExacAlleleFrequencyAll");
            AssertUtilities.CheckAlleleFrequencies(altAllele, 0.046417, "ExacAlleleFrequencyAfrican");
            AssertUtilities.CheckAlleleFrequencies(altAllele, 0.169241, "ExacAlleleFrequencyAmerican");
            AssertUtilities.CheckAlleleFrequencies(altAllele, 0.078326, "ExacAlleleFrequencyEastAsian");
            AssertUtilities.CheckAlleleFrequencies(altAllele, 0.05431, "ExacAlleleFrequencyFinish");
            AssertUtilities.CheckAlleleFrequencies(altAllele, 0.056521, "ExacAlleleFrequencyNonFinish");
            AssertUtilities.CheckAlleleFrequencies(altAllele, 0.088235, "ExacAlleleFrequencyOther");
            AssertUtilities.CheckAlleleFrequencies(altAllele, 0.109281, "ExacAlleleFrequencySouthAsian");

            // ReSharper disable once PossibleNullReferenceException
            foreach (var clinVarEntry in altAllele.ClinVarEntries)
            {
                Assert.Equal("RCV000082046.4", clinVarEntry.ID);
                Assert.Equal("germline", clinVarEntry.AlleleOrigin);
                Assert.Equal("not_specified", clinVarEntry.Phenotype);
                Assert.Equal("CN169374", clinVarEntry.MedGenID);
                Assert.Equal("Benign", clinVarEntry.Significance);
            }

            var cosmicEntry = altAllele.CosmicEntries.ElementAt(1);
            Assert.NotNull(cosmicEntry);

            Assert.Equal("COSM3730300", cosmicEntry.ID);
            Assert.Equal("true", cosmicEntry.IsAlleleSpecific);
            Assert.Equal("-", cosmicEntry.AltAllele);
            Assert.Equal("COG6", cosmicEntry.Gene);

            // ReSharper disable once PossibleNullReferenceException
            foreach (var cosmicStudy in cosmicEntry.Studies)
            {
                Assert.Equal("carcinoma", cosmicStudy.Histology);
                Assert.Equal("oesophagus", cosmicStudy.PrimarySite);
            }

            cosmicEntry = altAllele.CosmicEntries.ElementAt(2);
            Assert.NotNull(cosmicEntry);

            Assert.Equal("COSM3730301", cosmicEntry.ID);
            Assert.Equal("true", cosmicEntry.IsAlleleSpecific);
            Assert.Equal("-", cosmicEntry.AltAllele);
            Assert.Equal("COG6_ENST00000416691", cosmicEntry.Gene);

            // ReSharper disable once PossibleNullReferenceException
            foreach (var cosmicStudy in cosmicEntry.Studies)
            {
                Assert.Equal("carcinoma", cosmicStudy.Histology);
                Assert.Equal("oesophagus", cosmicStudy.PrimarySite);
            }
        }

        [Fact]
        public void RefNoCall()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_22426387_22426388.nsa",
                "1	22426387	.	A	.	.	LowGQX	END=22426406;BLOCKAVG_min30p3a	GT:GQX:DP:DPF	.:.:0:0", null, true);
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault();
            Assert.NotNull(altAllele);

            Assert.Equal("reference_no_call", altAllele.VariantType);
            Assert.Equal("1:22426387:22426406:NC", altAllele.VariantId);
        }

        [Fact]
        public void RefMinorCosmic()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chrX_144904882_144904882.nsa",
                "X	144904882	.	T	.	.	PASS	RefMinor;phyloP=-0.312	GT:GQX:DP:DPF:AD	0:509:35:2:35", null, true);
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            Assert.Equal("SNV", altAllele.VariantType);
            Assert.Equal("X:144904882:T", altAllele.VariantId);
            Assert.True(altAllele.IsReferenceMinor);

            var cosmicEntry = altAllele.CosmicEntries.FirstOrDefault();
            Assert.NotNull(cosmicEntry);

            Assert.Equal(cosmicEntry.ID, "COSM391442");
            Assert.Equal(cosmicEntry.AltAllele, "-");
            Assert.Equal(cosmicEntry.Gene, "SLITRK2");

            // ReSharper disable once PossibleNullReferenceException
            foreach (var study in cosmicEntry.Studies)
            {
                Assert.Equal(study.Histology, "carcinoma");
                Assert.Equal(study.PrimarySite, "lung");
            }
        }

        [Fact]
        public void MissingRefAllele()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_15274_15275.nsa",
                "chr1	15274	.	A	.	279.00	PASS	SNVSB=-39.1;SNVHPOL=2	GT:GQ:GQX:DP:DPF:AD	1/2:58:55:20:1:0,5,15");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            Assert.Equal("A", altAllele.RefAllele);
            Assert.Null(altAllele.AltAllele);
            Assert.True(altAllele.IsReferenceMinor);
        }

        [Fact]
        public void BadClinVarRef()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr11_109157259_109157260.nsa",
                "11	109157259	.	T	.	.	PASS	RefMinor;GMAF=T|0.01877 GT:GQX:DP:DPF:AD        0/0:69:24:3:24");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            // there should not be any clinvar entries
            Assert.True(altAllele.IsReferenceMinor);
            Assert.Equal(0, altAllele.ClinVarEntries.Count());
        }

        [Fact]
        public void DuplicateOneKgFreq()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr5_29786207_29786208.nsa",
                "5	29786207	rs150619197	C	.	.	.	END=29786207;BLOCKAVG_min30p3a;AF1000G=.,0.994409;GMAF=A|0.9944;RefMinor	GT:GQX:DP:DPF	0:24:9:0");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            // we should not have any 1kg frequencies for this position
            Assert.Null(typeof(IAnnotatedAlternateAllele).GetTypeInfo().GetProperty("AlleleFrequencyAll").GetValue(altAllele));
        }

        [Fact]
        public void AnnotationCarryover()
        {
            var annotationSource = ResourceUtilities.GetAnnotationSource(null) as NirvanaAnnotationSource;
            annotationSource?.EnableReferenceNoCalls(false);

            var saReader = ResourceUtilities.GetSupplementaryAnnotationReader("chr2_90472571_90472592.nsa");
            annotationSource?.SetSupplementaryAnnotationReader(saReader);

            var annotatedVariant = DataUtilities.GetVariant(annotationSource,
                "2	90472571	.	AAAAAAAAAAAAAAAAAAGTCC	AGTCT	177	PASS	CIGAR=1M21D4I;RU=.;REFREP=.;IDREP=.	GT:GQ:GQX:DPI:AD	0/1:220:177:46:40,7");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            Assert.False(altAllele.IsReferenceMinor);
            Assert.Equal("indel", altAllele.VariantType);
            Assert.Equal("2:90472572:90472592:GTCT", altAllele.VariantId);

            annotatedVariant = DataUtilities.GetVariant(annotationSource,
                "2	90472592	.	C	.	.	PASS	RefMinor	GT:GQX:DP:DPF:AD	0:96:33:15:33");
            Assert.NotNull(annotatedVariant);

            altAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault();
            Assert.NotNull(altAllele);

            Assert.True(altAllele.IsReferenceMinor);
            Assert.Equal("SNV", altAllele.VariantType);
        }

        [Fact]
        public void RefSiteRefMinor()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_789256_789257.nsa",
                "1	789256	rs3131939	T	.	.	LowGQX	END=789256	GT:GQX:DP:DPF:AD	0:.:0:0:0", null, true);
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            Assert.True(altAllele.IsReferenceMinor);
            Assert.Equal("SNV", altAllele.VariantType);
        }

        [Fact]
        public void RefMinor1000G()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_825069_825070.nsa",
                "chr1	825069	rs4475692	G	.	362.00	LowGQX;HighDPFRatio	SNVSB=-36.9;SNVHPOL=3	GT:GQ:GQX:DP:DPF:AD	1/2:4:0:52:38:8,11,33", null, true);
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            // this is a ref no-call. In the earlier verion of the the test, the ref minor flag was artificially set.
            // so, it should have no SA
            Assert.Equal("reference_no_call", altAllele.VariantType);
            Assert.Null(altAllele.GlobalMinorAlleleFrequency);
            Assert.Null(altAllele.GlobalMinorAllele);
        }

        [Fact]
        public void MissingRefMinorAnnotation()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr2_193187632_193187633.nsa",
                "2	193187632	.	G	.	.	LowGQX;HighDPFRatio	.	GT:GQX:DP:DPF	.:.:0:2");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            Assert.True(altAllele.IsReferenceMinor);
        }

        [Fact]
        public void UpstreamGeneVariant()
        {
            var annotatedVariant = DataUtilities.GetVariant("ENST00000576171_chr17_Ensembl84.ndb",
                "chr17_46107_46108.nsa",
                "17	46107	.	A	.	153	LowGQX	SNVSB=-20.1;SNVHPOL=4;GMAF=G|0.9988;RefMinor;CSQT=A||ENST00000576171|upstream_gene_variant	GT:GQ:GQX:DP:DPF:AD	1/1:18:18:7:0:0,7");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(1, altAllele);

            var transcript = altAllele.EnsemblTranscripts.FirstOrDefault();
            Assert.NotNull(transcript);

            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Equal("upstream_gene_variant", string.Join("&", transcript.Consequence));
        }


        [Fact]
        public void RefAlleleForRefMinor()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, "chr1_789256_789257.nsa",
                "1	789256	.	T	<NON_REF>	.	QUAL	END=789256	GT:DP:GQ:MIN_DP:PL	0/0:32:0:32:0,0,0");
            Assert.NotNull(annotatedVariant);

            // bool isGatkGenomeVcf = true;

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.First();
            Assert.NotNull(altAllele);

            Assert.Equal("T", altAllele.RefAllele);
        }

        [Fact]
        public void MantaInsertionRsId()
        {
            var annotatedVariant = DataUtilities.GetVariant(null, Path.Combine("hg38", "chr1_186774064_186774065.nsa"),
                "chr1	186774064	MantaINS:338:0:0:0:0:0	C	CTATATATATACTTTATATATACTGTATGTGTATATATAAAGTATATATATAGTG	101	MinGQ	END=186774064;SVTYPE=INS;SVLEN=54;CIGAR=1M54I;CIPOS=0,8;HOMLEN=8;HOMSEQ=TATATATA	GT:FT:GQ:PL:PR:SR	0/0:MinGQ:4:48,3,0:0,0:0,1	0/0:MinGQ:3:50,3,0:0,0:0,1	1/1:MinGQ:7:148,9,0:0,0:0,3");
            Assert.NotNull(annotatedVariant);

            AssertUtilities.CheckJsonContains("rs57376790", annotatedVariant);
        }
    }
}
