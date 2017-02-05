using UnitTests.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    public class GeneFusionTests
    {
        [Fact]
        public void DeletionTest()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000354725_ENST00000486037_ENST00000565468_Ensembl84_multi"),
                "chr7	127717248	.	T	<DEL>	.	.	END=140789466;SVTYPE=DEL;SVLEN=13072218;INV5;EVENT=MantaINV:267944:0:1:1:0:0;SOMATIC;SOMATICSCORE=283;JUNCTION_SOMATICSCORE=139", "ENST00000565468");

            Assert.NotNull(transcriptAllele);
            Assert.Contains("TMEM178B{ENST00000565468.1}:c.382+14975_885_SND1{ENST00000486037.1}:c.1_427-4164", transcriptAllele.ToString());
        }

        [Fact]
        public void DuplicationTest()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000354725_ENST00000486037_ENST00000565468_Ensembl84_multi"),
                "chr7	127717248	.	T	<DUP>	.	.	END=140789466;SVTYPE=DUP;SVLEN=13072218;INV5;EVENT=MantaINV:267944:0:1:1:0:0;SOMATIC;SOMATICSCORE=283;JUNCTION_SOMATICSCORE=139", "ENST00000565468");

            Assert.NotNull(transcriptAllele);
            Assert.Contains("TMEM178B{ENST00000565468.1}:c.1_382+14974_SND1{ENST00000354725.3}:c.1968+2506_2733", transcriptAllele.ToString());
            Assert.Contains("TMEM178B{ENST00000565468.1}:c.1_382+14974_SND1{ENST00000486037.1}:c.427-4164_695", transcriptAllele.ToString());
        }

        [Fact]
        public void InversionTest()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000354725_ENST00000486037_ENST00000565468_Ensembl84_multi"),
                "chr7	127717438	MantaINV:267944:0:1:1:0:0	T	<INV>	.	PASS	END=140789639;SVTYPE=INV;SVLEN=13072201;INV3;EVENT=MantaINV:267944:0:1:1:0:0;SOMATIC;SOMATICSCORE=283;JUNCTION_SOMATICSCORE=173", "ENST00000565468");

            Assert.NotNull(transcriptAllele);
            Assert.Contains("TMEM178B{ENST00000565468.1}:c.1_382+15147_oSND1{ENST00000354725.3}:c.1_1968+2696", transcriptAllele.ToString());
            Assert.Contains("TMEM178B{ENST00000565468.1}:c.1_382+15147_oSND1{ENST00000486037.1}:c.1_427-3974", transcriptAllele.ToString());
        }

        [Fact]
        public void TandemDuplicationTest()
        {
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000354725_ENST00000486037_ENST00000565468_Ensembl84_multi"),
                "chr7	127717248	.	T	<DUP:TANDEM>	.	.	END=140789466;SVTYPE=DUP;SVLEN=13072218;INV5;EVENT=MantaINV:267944:0:1:1:0:0;SOMATIC;SOMATICSCORE=283;JUNCTION_SOMATICSCORE=139", "ENST00000565468");

            Assert.NotNull(transcriptAllele);
            Assert.Contains("TMEM178B{ENST00000565468.1}:c.1_382+14974_SND1{ENST00000354725.3}:c.1968+2506_2733", transcriptAllele.ToString());
            Assert.Contains("TMEM178B{ENST00000565468.1}:c.1_382+14974_SND1{ENST00000486037.1}:c.427-4164_695", transcriptAllele.ToString());
        }

        [Fact]
        public void TransgenicTranslocationGeneFusion()
        {
            var transcriptAllele =
                DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000396373_ENST00000416754_ENST00000437180_Ensembl84_multi"),
                    "chr12	12026305	MantaBND:111133:1:2:0:1:0:1	A	A]chr21:36420571]	.	PASS	SVTYPE=BND;MATEID=MantaBND:111133:1:2:0:1:0:0;EVENT=MantaBND:111133:1:2:0:1:0:0;SOMATIC;SOMATICSCORE=271;JUNCTION_SOMATICSCORE=170;BND_DEPTH=63;MATE_BND_DEPTH=54	PR:SR	94,0:63,0	44,20:37,14",
                    "ENST00000396373");

            Assert.NotNull(transcriptAllele);
            Assert.Contains("ETV6{ENST00000396373.4}:c.1_1009+3402_RUNX1{ENST00000416754.1}:c.58+568_97", transcriptAllele.ToString());
            Assert.Contains("ETV6{ENST00000396373.4}:c.1_1009+3402_RUNX1{ENST00000437180.1}:c.58+568_1443", transcriptAllele.ToString());
        }

        //[Fact]
        //public void TranscriptFromDifferentSourcesTest()
        //{
        //    var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("Both84_chr11_308345_314262"),
        //        "chr11	308345	.	C	<DEL>	0	LowQ	SVTYPE=DEL;END=314262;ALTDEDUP=0;ALTDUP=0;REFDEDUP=0;REFDUP=0;INTERGENIC=False	.	.", "NM_006435.2");

        //    Assert.NotNull(transcriptAllele);
        //    Assert.Contains("IFITM2{NM_006435.2}:c.1_153_IFITM1{NM_003641.3}:c.93_378", transcriptAllele.ToString());
        //    Assert.DoesNotContain("IFITM2{NM_006435.2}:c.1_153_IFITM1{ENST00000528780}:c.93_378", transcriptAllele.ToString());
        //}
    }
}
