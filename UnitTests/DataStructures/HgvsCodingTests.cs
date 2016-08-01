using UnitTests.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class HgvsCodingTests
    {
        [Fact]
        public void AlleleNormalizedDeletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENSESTT00000082768_UF_chr1_RefSeq84.ndb",
                "1\t59270922\t.\tCACACAA\tC,CAC\t71.00\tPASS\t.", "ENSESTT00000082768", "", "ACAA");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENSESTT00000082768.1:c.-7+19949_-7+19952delCAAA\"", transcriptAllele.ToString());
        }

        [Fact]
        public void AltAlleleWithN()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000275493_chr7_Ensembl84.ndb",
                "chr7\t55249018\t.\tC\tCNNN\t1000\tPASS\t.\tGT\t0/1", "ENST00000275493", "NNN");
            Assert.NotNull(transcriptAllele);
            Assert.DoesNotContain("\"hgvsc\"", transcriptAllele.ToString());
            Assert.DoesNotContain("\"hgvsp\"", transcriptAllele.ToString());
        }

        [Fact]
        public void ThreePrimeShiftingIntronic1()
        {
            var transcriptAllele = DataUtilities.GetTranscript("NR_047519.1_chr1_RefSeq84.ndb",
                "chr1	775789	.	TA	T	244.00	PASS	.", "NR_047519.1", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"NR_047519.1:n.288-7230delA\"", transcriptAllele.ToString());
        }

        [Fact]
        public void ThreePrimeShiftingIntronic2()
        {
            var transcriptAllele = DataUtilities.GetTranscript("XM_005244727.1_chr1_RefSeq84.ndb",
                "chr1	867993	.	GTTTC	G	1427.00	PASS	.", "XM_005244727.1", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"XM_005244727.1:c.305+1529_305+1532delTTTC\"", transcriptAllele.ToString());
        }

        [Fact]
        public void ReverseStrandMultiBaseShifting()
        {
            var transcriptAllele = DataUtilities.GetTranscript("XM_005244759.1_chr1_RefSeq84.ndb",
                "chr1	1296369	.	GAC	G	2694.00	PASS	.", "XM_005244759.1", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"XM_005244759.1:c.22+251_22+252delGT\"", transcriptAllele.ToString());
        }

        [Fact]
        public void DownstreamExonShifting()
        {
            var transcriptAllele = DataUtilities.GetTranscript("XM_005244748.1_chr1_RefSeq84.ndb",
                "chr1	966391	.	ATG	A	2694.00	PASS	.", "XM_005244748.1", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"XM_005244748.1:c.464-4257_464-4256delTG\"", transcriptAllele.ToString());
        }

        [Fact]
        public void ReverseStrandTranscript3PrimeShifting()
        {
            var transcriptAllele = DataUtilities.GetTranscript("XR_241051.1_chr1_RefSeq84.ndb",
                "chr1	858691	.	TG	T	1680.00	PASS	.", "XR_241051.1", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"XR_241051.1:n.349+85delC\"", transcriptAllele.ToString());
        }

        [Fact]
        public void MonobaseDeletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000324856_chr1_Ensembl84.ndb",
                "chr1	27087418	COSM3358384	AGG	A	.	.	.", "ENST00000324856", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENST00000324856.7:c.1994_1995delGG\"", transcriptAllele.ToString());
        }

        [Fact]
        public void BadDuplicationCall()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000256646_chr1_Ensembl84.ndb",
                "chr1	120612043	COSN4625707	G	GTCC	.	.	.", "ENST00000256646", "TCC");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENST00000256646.2:c.-24_-23insGGA\"", transcriptAllele.ToString());
        }

        [Fact]
        public void ExonThreePrimeShifting()
        {
            var transcriptAllele = DataUtilities.GetTranscript("NR_015368.2_chr1_RefSeq84.ndb",
                "chr1	790758	.	GTA	G	1680.00	PASS	.", "NR_015368.2", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"NR_015368.2:n.2416_2417delAT\"", transcriptAllele.ToString());
        }

        [Fact]
        public void Deletion()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000368138_chr1_Ensembl84.ndb",
                "chr1\t158945025\t.\tCTG\tC\t413.00\tPASS\t.", "ENST00000368138", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENST00000368138.3:c.*6-1462_*6-1461delGT\"", transcriptAllele.ToString());
        }

        [Fact]
        public void DuplicationExtraShift()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000457599_chr1_Ensembl84.ndb",
                "chr1	27057747	COSM51220	C	CCCTAC	.	.	.", "ENST00000457599", "CCTAC");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENST00000457599.2:c.1451_1455dupCCTAC\"", transcriptAllele.ToString());
        }

        [Fact]
        public void CosmicDuplication()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000515242_chr1_Ensembl84.ndb",
                "1	13417	COSN520661	C	CGAGA	.	.	.", "ENST00000515242", "GAGA");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENST00000515242.2:n.659_662dupGAGA\"", transcriptAllele.ToString());
        }

        [Fact]
        public void UnwantedShifting()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000324856_chr1_Ensembl84.ndb",
                "chr1	27023773	COSM133010	C	CC	.	.	.", "ENST00000324856", "C");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENST00000324856.7:c.879dupC\"", transcriptAllele.ToString());
        }

        [Fact]
        public void UnwantedShiftingDup()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000379370_chr1_Ensembl84.ndb",
                "1	987131	COSM1683283	G	GG	.	.	.", "ENST00000379370", "G");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENST00000379370.2:c.5587dupG\"", transcriptAllele.ToString());
        }

        [Fact]
        public void Duplication()
        {
            var transcriptAllele = DataUtilities.GetTranscript("NM_178221.2_chr1_RefSeq84.ndb",
                "chr1\t63318421\t.\tTT\tTTT,T\t31\tPASS\t.", "NM_178221.2", "T");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"NM_178221.2:c.1209+11215dupT\"", transcriptAllele.ToString());
        }

        [Fact]
        public void Duplication_InFrameInsertion()
        {
            var transcriptAllele = DataUtilities.GetTranscript("NM_024011.2_chr1_RefSeq84.ndb",
                "chr1\t1647893\t.\tT\tTTTTCTT\t31\tPASS\t.", "NM_024011.2", "TTTCTT");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"NM_024011.2:c.374_379dupAAGAAA\"", transcriptAllele.ToString());
        }

        [Fact]
        public void Duplication_InIntron()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENSESTT00000034529_UF_chr1_RefSeq84.ndb",
                "chr1\t811290\t.\tT\tTCCCACGCTCC\t31\tPASS\t.", "ENSESTT00000034529", "CCCACGCTCC");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENSESTT00000034529.1:c.24-765_24-756dupGGAGCGTGGG\"", transcriptAllele.ToString());
        }

        [Fact]
        public void IntronicAltAlleleN()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000269305_chr17_Ensembl84.ndb",
                "chr17\t7577156\t.\tC\tCNNN\t1000\tPASS\t.\tGT\t0/1", "ENST00000269305", "NNN");
            Assert.NotNull(transcriptAllele);
            Assert.DoesNotContain("\"hgvsc\"", transcriptAllele.ToString());
            Assert.DoesNotContain("\"hgvsp\"", transcriptAllele.ToString());
        }

        [Fact]
        public void Insertion()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000472832_chr10_Ensembl84.ndb",
                "chr10	89717674	COSM4774944	A	ACC	.	.	.", "ENST00000472832", "CC");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":", transcriptAllele.ToString());
        }

        [Fact]
        public void IntronOffset()
        {
            var transcriptAllele = DataUtilities.GetTranscript("NM_176877.2_chr1_RefSeq84.ndb",
                "chr1\t62620322\t.\tT\tTA\t31\tPASS\t.", "NM_176877.2", "A");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"NM_176877.2:c.5379-6245dupA\"", transcriptAllele.ToString());
        }

        [Fact]
        public void IntronOffset2()
        {
            var transcriptAllele = DataUtilities.GetTranscript("NR_034015.1_chr1_RefSeq84.ndb",
                "chr1\t59312754\t.\tT\tTAAAAAAAAAAA\t31\tPASS\t.", "NR_034015.1", "AAAAAAAAAAA");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"NR_034015.1:n.156-46462_156-46452dupAAAAAAAAAAA\"", transcriptAllele.ToString());
        }

        [Fact]
        public void IntronOffsetNoCdna()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000372299_chr1_Ensembl84.ndb",
                "chr1\t44596859\t.\tG\tA\t122.00\tPASS\t.", "ENST00000372299", "A");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENST00000372299.3:c.*431G>A\"", transcriptAllele.ToString());
        }

        [Fact]
        public void MissingEntry()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000366088_chr1_Ensembl84.ndb",
                "chr1\t230451517\t.\tG\tT\t413.00\tPASS\t.", "ENST00000366088", "T");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENST00000366088.1:n.450+1C>A\"", transcriptAllele.ToString());
        }

        [Fact]
        public void Multiple()
        {
            var transcriptAllele = DataUtilities.GetTranscript("CCDS58003.1_UF_chr1_RefSeq84.ndb",
                "1\t60064767\t.\tCGTTT\tCGTTTGTTTGTTT,C\t1111\tPASS\t.", "CCDS58003.1", "GTTTGTTT");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"CCDS58003.1:c.640-8691_640-8684dupGTTTGTTT\"", transcriptAllele.ToString());
        }

        [Fact]
        public void OneExonicOneIntronic()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000602074_chr1_Ensembl84.ndb",
                "1\t17658124\t.\tGTGGCC\tG\t93.00\tPASS\t.", "ENST00000602074", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENST00000602074.1:c.313_316+1delGGCCA\"", transcriptAllele.ToString());
        }

        [Fact]
        public void StartEqualToStopCodon()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000474756_chr1_Ensembl84.ndb",
                "chr1\t6292935\t.\tG\tT\t98.00\tPASS\t.", "ENST00000474756", "T");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":", transcriptAllele.ToString());
        }

        [Fact]
        public void UnwantedHgvsCodingNomenclature()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENSESTT00000011387_UF_chr1_RefSeq84.ndb",
                "chr1\t110908768\t.\tGCTCA\tG\t98.00\tPASS\t.", "ENSESTT00000011387", "");
            Assert.NotNull(transcriptAllele);
            Assert.DoesNotContain("\"hgvsc\"", transcriptAllele.ToString());
        }

        [Fact]
        public void VariantInExonBoundaryAndCanShiftToIntron()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000457599_chr1_Ensembl84.ndb",
                "chr1\t27092856\t.\tAG\tA\t.\t.\t.", "ENST00000457599", "");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":\"ENST00000457599.2:c.2878+1delG\"", transcriptAllele.ToString());
            Assert.DoesNotContain("\"hgvsp\"", transcriptAllele.ToString());
        }

        [Fact]
        public void InsertionDeletionVariants()
        {
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000540690_chr1_Ensembl84.ndb",
                "chr1\t27106158\t.\tCG\tCAA\t.\t.\t.", "ENST00000540690", "AA");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsc\":", transcriptAllele.ToString());
        }

	    [Fact]
	    public void DeletionBeyondTranscipt()
	    {
		    string vcfLine= "chr5	95966535	.	AGAGAAAGAAGGAAAGAAAGAAAGGAAGGAAGGAAGGAAGAAGAAAAGGAAAGGAAAAGAAAGAAAGGTTGTTTTGTTTTGGTCTACAATTGATTCTTGTATACTAAAAAAAATTTTTTTAAATAATCTCTTACAGAGTCTCCACCTGACTGCATTGCTTCATGGCACTGTGCTTTTGGACATTTGATATCCTTCAGCTAGAATATATTTCTTCATTGTCCATTCACAGGAGCCTTCCTTTCTTAAAGACTCAATGTAAGCATCTCCTCCTTTTTAAGTAGCATTTTTTAAATTGACATATACTTCACAGGACATAAAATCCACCATCTTAAAGCAAGTTCACTCTGTTGTACAACCATCACCACTATCTAACTCCAGAACATTTTCATCACCCCAAAAAGGACACTAGGACCTGTCAGCAATTAGTCCCAATTCACCTCTATTCCCATGCCCTGGCAACCACTAATCTACTTTCTATCTCTATAAATTTGGCTATTTTGGACATTTAATACAAATGTAATCATAAAATATGTGTCCTTTTTTGTCTGACTCCTTTCATTTAGTGTAATATTTTCAAGTTTTATTCATGTAGCATGTATCAGTACTTTACTCCTTTTTATGATTAAATAACATCCCATTGTATGGATTTCATTTTATTTATTCATTAGCTGATGGGCATTTAGGTTACTTTCACTTTTGGCTATTGTGAATAATGCTGCTGTGAACGTTTGTGTAAAAGTTTTTCTGTGAATACTTTTTTTTTTTTTTGAGACGGAGTCTTGCTCTGTCTCCCAGGCTGGAGTGCAGTGGTGAGATCTTGGCTCCCC	A	.	.	.";
			var transcriptAllele = DataUtilities.GetTranscript("XR_246551.1_chr5_RefSeq84.ndb", vcfLine,"XR_246551.1");
			Assert.NotNull(transcriptAllele);
			Assert.DoesNotContain("\"hgvsc\":", transcriptAllele.ToString());
		}

		[Fact]
		public void Inversion()
		{
			var transcriptAllele = DataUtilities.GetTranscript("XM_005244727.1_chr1_RefSeq84.ndb",
				"chr1	867993	.	TTTC	GAAA	1427.00	PASS	.", "XM_005244727.1");
			Assert.NotNull(transcriptAllele);
			Assert.Contains("\"hgvsc\":\"XM_005244727.1:c.305+1524_305+1527invTTTC\"", transcriptAllele.ToString());
		}
	}
}