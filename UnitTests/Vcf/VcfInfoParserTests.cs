using Vcf.Info;
using Xunit;

namespace UnitTests.Vcf
{
    public sealed class VcfInfoParserTests
    {
        [Fact]
        public void Info_tags_are_capturedCorrect1()
        {
            var info = VcfInfoParser.Parse(
                "SOMATIC;QSI=2;TQSI=1;NT=het;QSI_NT=2;TQSI_NT=1;VQSR=5.9;CN=6");
            Assert.Equal(2, info.JointSomaticNormalQuality);
            Assert.Equal(6, info.CopyNumber);
            Assert.Equal(5.9, info.RecalibratedQuality);
        }

        [Fact]
        public void Info_tags_are_capturedCorrect2()
        {
            var info = VcfInfoParser.Parse(
                "END=1660503;SVTYPE=DEL;SVLEN=-65919;IMPRECISE;CIPOS=-285,285;CIEND=-205,205;SOMATIC;SOMATICSCORE=36;ColocalizedCanvas");
            Assert.Equal(65919, info.SvLength);
            Assert.Equal(1660503, info.End);
            Assert.True(info.ColocalizedWithCnv);
            Assert.Equal(36, info.JointSomaticNormalQuality);
            Assert.Equal(new[] {-285, 285}, info.CiPos);
            Assert.Equal(new[] {-205, 205}, info.CiEnd);
        }

        [Fact]
        public void Info_tags_are_capturedCorrect3()
        {
            var info = VcfInfoParser.Parse(
                "AC=2;AF=0.250;AN=8;BaseQRankSum=1.719;DB;DP=106;Dels=0.00;FS=20.202;HaplotypeScore=0.0000;MLEAC=2;MLEAF=0.250;MQ=43.50;MQ0=52;MQRankSum=2.955;QD=4.73;ReadPosRankSum=1.024;SB=-1.368e+02;VQSLOD=-0.3503;culprit=MQ;PLF");

            Assert.Equal(106, info.Depth);
            Assert.Equal(-136.8, info.StrandBias);
        }

        [Fact]
        public void Info_tags_are_correctly_removed()
        {
            var info = VcfInfoParser.Parse(
                "END=964423;SVTYPE=DEL;SVLEN=-422;IMPRECISE;CIPOS=-170,170;CIEND=-175,175;CSQT=1|AGRN|ENST00000379370|");
            Assert.Equal("END=964423;SVTYPE=DEL;SVLEN=-422;IMPRECISE;CIPOS=-170,170;CIEND=-175,175",
                info.UpdatedInfoField);
        }

        [Fact]
        public void Blank_info_field_when_remove_all_info_tags()
        {
            var info = VcfInfoParser.Parse(
                "AA=G;CSQT=1|AGRN|ENST00000379370|");
            Assert.Equal("", info.UpdatedInfoField);
        }

        [Fact]
        public void EmptyInfoField()
        {
            Assert.Null(VcfInfoParser.Parse(""));
        }
    }
}