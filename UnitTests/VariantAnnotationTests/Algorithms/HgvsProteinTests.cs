using System.Text.RegularExpressions;
using UnitTests.Utilities;
using VariantAnnotation.Algorithms;
using Xunit;

namespace UnitTests.VariantAnnotationTests.Algorithms
{
    public sealed class HgvsProteinTests
    {
        #region members

        private readonly Regex _transcriptRegex;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public HgvsProteinTests()
        {
            _transcriptRegex = new Regex("^(.*?)_chr", RegexOptions.Compiled);
        }

        [Fact]
        public void UnidentifiedDuplication()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000374152_chr1_Ensembl84"),
                "chr1	27100181	COSM1341408	C	CGCA	.	.	.", "ENST00000374152", "GCA");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsp\":\"ENSP00000363267.2:p.(Gln951dup)\"", transcriptAllele.ToString());
        }

        [Fact]
        public void InitiatorCodonQuestion()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000487053_chr1_Ensembl84"),
                "chr1\t1558792\t.\tT\tC\t1060.00\tPASS\t.", "ENST00000487053", "C");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsp\":\"ENSP00000424615.1:p.(Met1?)\"", transcriptAllele.ToString());
        }

        [Fact]
        public void ProteinPositionCheck()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000288135_chr4_Ensembl84"),
                "chr4\t55592180\t.\tG\tGCTTCTG\t1000\tPASS\t.\tGT\t0/1", "ENST00000288135", "CTTCTG");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsp\":\"ENSP00000288135.5:p.(Ser501_Ala502dup)\"", transcriptAllele.ToString());
        }

        [Fact]
        public void TwoAminoAcidChange()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000359597_chr17_Ensembl84"),
                "chr17\t7577571\t.\tATG\tAAT\t1000\tPASS\t.", "ENST00000359597", "AT");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsp\":\"ENSP00000352610.4:p.(TyrMet236TerLeu)\"", transcriptAllele.ToString());
        }

        [Fact]
        public void StartIndexError()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000535881_chr1_Ensembl84"),
                "chr1	94476425	.	A	ATGGCAAACAGGTTCT	.	.	CLASS=DM;MUT=ALT;GENE=ABCA4;STRAND=-;DNA=NM_000350.2:c.5630_5644dupAGAACCTGTTTGCCA;PHEN=Stargardt_disease;ACC=CI092303", "ENST00000535881");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsp\":", transcriptAllele.ToString());
        }

        [Fact]
        public void MultiProteinInsertion()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000372470_chr1_Ensembl84"),
                "chr1	43815004	COSM133104	G	GCTGAGCTGCCTG	.	.	AA=p.L513_R514insLSCL;CDS=c.1539_1540ins12;CNT=1;GENE=MPL;SF=0;STRAND=+;CSQ=CTGAGCTGCCTG|inframe_insertion|MODERATE|MPL|ENSG00000117400|Transcript|ENST00000372470|protein_coding|10/12||ENST00000372470.3:c.1539_1540insCTGAGCTGCCTG|ENSP00000361548.3:p.Leu513_Arg514insLeuSerCysLeu|1581-1582|1539-1540|513-514|-/LSCL|-/CTGAGCTGCCTG|||1|HGNC|7217|YES|CCDS483.1|ENSP00000361548|||hmmpanther:PTHR23037&hmmpanther:PTHR23037:SF8|||||", "ENST00000372470");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsp\":", transcriptAllele.ToString());
        }

        [Fact]
        public void VepNoHgvsProteinNomenclature()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000324856_chr1_Ensembl84"),
                "chr1	27092856	COSM1731931	AG	A	.	.	.", "ENST00000324856", "");
            Assert.NotNull(transcriptAllele);
            Assert.DoesNotContain("\"hgvsp\"", transcriptAllele.ToString());
        }

        [Fact]
        public void NoHgvsProteinNomenclature()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000430291_chr1_Ensembl84"),
                "chr1	27092856	COSM1731931	AG	A	.	.	.", "ENST00000430291", "");
            Assert.NotNull(transcriptAllele);
            Assert.DoesNotContain("\"hgvsp\"", transcriptAllele.ToString());
        }

        [Fact]
        public void FrameshiftTerCodon()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000416839_chr1_Ensembl84"),
                "chr1\t220603308\t.\tTGTGTGA\tT,TGT\t40.00\tLowGQXHetDel\t.", "ENST00000416839", "", "GTGA");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsp\":\"ENSP00000404591.2:p.(Leu42HisfsTer?)\"", transcriptAllele.ToString());
        }

        [Theory]
        [InlineData("ENST00000368232_chr1_Ensembl84", "AC", "chr1\t156565049\t.\tA\tAAC\t3070.00\tPASS\t.", "ENSP00000357215.4:p.(Phe357ValfsTer91)")]
        [InlineData("ENST00000600779_chr1_Ensembl84", "", "chr1\t2258668\t.\tGACACAGAAAC\tG\t688.00\tPASS\t.", "ENSP00000469612.1:p.(Gln38GlufsTer?)")]
        [InlineData("ENST00000464439_chr1_Ensembl84", "", "chr1\t47280746\t.\tGAT\tG\t98.00\tPASS\t.", "ENSP00000433068.1:p.(Met271GlufsTer218)")]
        [InlineData("ENST00000391369_chr1_Ensembl84", "", "chr1\t32379996\t.\tCTATT\tC\t98.00\tPASS\t.", "ENSP00000375181.1:p.(Phe89ValfsTer37)")]
        public void Frameshifts(string cacheFileName, string altAllele, string vcfLine, string expectedHgvsProteinSequenceName)
        {
            CheckHgvsProteinSequenceName(cacheFileName, altAllele, vcfLine, expectedHgvsProteinSequenceName);
        }

        [Theory]
        [InlineData("ENST00000264126_chr1_Ensembl84", "", "chr1\t109465165\t.\tACTT\tA\t1275.00\tPASS\t.", "ENSP00000264126.3:p.(Ser525del)")]
        [InlineData("ENST00000444639_chr1_Ensembl84", "", "chr1\t175129924\t.\tCCTTCTTCTT\tC\t1087.00\tPASS\t.", "ENSP00000463734.1:p.(Lys73_Lys75del)")]
        [InlineData("ENST00000416272_chr1_Ensembl84", "", "chr1\t1850627\t.\tCAGCGGCAGG\tC\t86.00\tLowGQXHetDel\t.", "ENSP00000409669.1:p.(Leu24_Leu26del)")]
        [InlineData("ENST00000434088_chr1_Ensembl84", "", "chr1\t179504025\t.\tAAAG\tA\t276.00\tPASS\t.", "ENSP00000391716.1:p.(Glu851del)")]
        public void Deletion(string cacheFileName, string altAllele, string vcfLine, string expectedHgvsProteinSequenceName)
        {
            CheckHgvsProteinSequenceName(cacheFileName, altAllele, vcfLine, expectedHgvsProteinSequenceName);
        }

        [Theory]
        [InlineData("ENST00000255416_chr1_Ensembl84", "C", "chr1\t203137787\t.\tT\tC\t0.00\tLowGQX\t.", "ENSP00000255416.4:p.(Ter478TrpextTer7)")]
        [InlineData("ENST00000374163_chr1_Ensembl84", "C", "chr1\t26879920\t.\tT\tC\t1047.00\tPASS\t.", "ENSP00000363278.1:p.(Ter205ArgextTer21)")]
        [InlineData("NM_001080484_chr1_RefSeq84", "G", "chr1\t1887019\t.\tA\tG\t646.00\tPASS\t.", "NP_001073953.1:p.(Ter763GlnextTer6)")]
        [InlineData("ENST00000474953_chr1_Ensembl84", "A", "chr1\t235324545\t.\tT\tA\t1047.00\tPASS\t.", "ENSP00000420620.1:p.(Ter697LeuextTer7)")]
        [InlineData("ENST00000366540_chr1_Ensembl84", "G", "chr1\t243663046\tCOSM534001\tT\tG\t.\t.\t.", "ENSP00000355498.1:p.(Ter466SerextTer1)")]
        [InlineData("XM_005245367_chr1_RefSeq84", "ACA", "chr1\t151263677\t.\tG\tGACA\t730\tPASS\t.", "XP_005245424.1:p.(Asn1244dup)")]
        public void StopLost(string cacheFileName, string altAllele, string vcfLine, string expectedHgvsProteinSequenceName)
        {
            CheckHgvsProteinSequenceName(cacheFileName, altAllele, vcfLine, expectedHgvsProteinSequenceName);
        }

        /// <summary>
        /// compares the expected with the observed HGVS protein sequence name
        /// </summary>
        private void CheckHgvsProteinSequenceName(string cacheFileName, string altAllele, string vcfLine,
            // ReSharper disable once UnusedParameter.Local
            string expectedHgvsProteinSequenceName)
        {
            var transcriptMatch = _transcriptRegex.Match(cacheFileName);
            var transcriptId    = transcriptMatch.Groups[1].Value;

            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37(cacheFileName), vcfLine, transcriptId, altAllele);
            Assert.NotNull(transcriptAllele);
            Assert.Contains($"\"hgvsp\":\"{expectedHgvsProteinSequenceName}\"", transcriptAllele.ToString());
        }

        [Fact]
        public void InsertionAtTheEndOfTranscriptWithOutStopCodon()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000555254_chr14_Ensembl84"),
                "chr14	73640399	.	A	AATTTAT	.	.	.", "ENST00000555254");
            Assert.NotNull(transcriptAllele);
            Assert.DoesNotContain("hgvsp", transcriptAllele.ToString());
        }

        [Fact]
        public void InsertionWithStop()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000420607_chr1_Ensembl84"),
                 "chr1	76226858	.	G	GCTAGAATGAGTTA	.	.	.", "ENST00000420607");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsp\":\"ENSP00000409612.2:p.(Gln342Ter)\"", transcriptAllele.ToString());
        }

        [Fact]
        public void NotAduplication()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000269305_chr17_Ensembl84"),
                 "chr17	7573984	.	A	AAGGCCTT	.	.	.", "ENST00000269305");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsp\":\"ENSP00000269305.4:p.(Leu348Ter)\"", transcriptAllele.ToString());
        }

        [Theory]
        [InlineData("A", "AA", true)]
        [InlineData("", "AF", false)]
        [InlineData("TSP", "TSP", false)]
        [InlineData("TCP", "TCPTC", false)]
        [InlineData("TCP", "TCPTCPTCP", true)]
        [InlineData("TCP", "TCPTCPTCA", false)]
        public void IsDuplicatedAminoAcidsTests(string refAminoAcids, string altAminoAcids, bool result)
        {
            Assert.Equal(result, HgvsProteinNomenclature.IsDuplicatedAminoAcids(refAminoAcids, altAminoAcids));
        }

        [Fact]
        public void SilentMutationGetCorrectHgvsProteinNomenclature()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000359597_chr17_Ensembl84"),
                "chr17\t7577573\t.\tG\tA\t1000\tPASS\t.", "ENST00000359597", "A");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsp\":\"ENST00000359597.4:c.708C>T(p.(Tyr236=))\"", transcriptAllele.ToString());
        }


        

        [Fact]
        public void StopRetainedMutationGetCorrectHgvsProteinNomenclature()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000359597_chr17_Ensembl84"),
                "chr17\t7569525\t.\tT\tTC\t1000\tPASS\t.", "ENST00000359597");
            Assert.NotNull(transcriptAllele);
            Assert.Contains("\"hgvsp\":\"ENST00000359597.4:c.1030_1031insG(p.(Ter344=))\"", transcriptAllele.ToString());
        }
    }
}