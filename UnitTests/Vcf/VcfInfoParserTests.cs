using Vcf.Info;
using Xunit;

namespace UnitTests.Vcf
{
    public sealed class VcfInfoParserTests
    {
        [Fact]
        public void Parse_Somatic_Manta()
        {
            var info = VcfInfoParser.Parse("END=1660503;SVTYPE=DEL;SVLEN=-65919;IMPRECISE;CIPOS=-285,285;CIEND=-205,205;SOMATIC;SOMATICSCORE=36;ColocalizedCanvas");
            Assert.Equal(65919, info.SvLength);
            Assert.Equal(1660503, info.End);
            Assert.Equal(36, info.JointSomaticNormalQuality);
            Assert.Equal(new[] {-285, 285}, info.CiPos);
            Assert.Equal(new[] {-205, 205}, info.CiEnd);
        }

        [Fact]
        public void Parse_Somatic_Strelka()
        {
            var info = VcfInfoParser.Parse("SOMATIC;QSS=2;TQSS=1;NT=het;QSS_NT=2;TQSS_NT=1;SGT=CG->CG;DP=183;MQ=46.57;MQ0=15;ALTPOS=35;ALTMAP=24;ReadPosRankSum=-1.23;SNVSB=0.00;PNOISE=0.00;PNOISE2=0.00;VQSR=1.23");
            Assert.Equal(1.23, info.RecalibratedQuality);
            Assert.Equal(2, info.JointSomaticNormalQuality);
        }

        [Fact]
        public void Parse_GATK()
        {
            var info = VcfInfoParser.Parse("AC=2;AF=0.250;AN=8;BaseQRankSum=1.719;DB;DP=106;Dels=0.00;FS=20.202;HaplotypeScore=0.0000;MLEAC=2;MLEAF=0.250;MQ=43.50;MQ0=52;MQRankSum=2.955;QD=4.73;ReadPosRankSum=1.024;SB=-1.368e+02;VQSLOD=-0.3503;culprit=MQ;PLF");

            Assert.Equal(-136.8, info.StrandBias);
        }

        [Fact]
        public void EmptyInfoField()
        {
            Assert.Null(VcfInfoParser.Parse(""));
        }
    }
}