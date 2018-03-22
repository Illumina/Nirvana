using System.Collections.Generic;
using SAUtils.InputFileParsers.ExAc;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    [Collection("ChromosomeRenamer")]
    public sealed class ExacTests
    {
        private readonly IDictionary<string, IChromosome> _refChromDict;

        /// <summary>
        /// constructor
        /// </summary>
        public ExacTests()
        {
            _refChromDict = new Dictionary<string, IChromosome>
            {
                {"1",new Chromosome("chr1", "1",0) },
                {"4",new Chromosome("chr4", "4", 3) }
            };
        }

        [Fact]
        public void ExacExtraction()
        {
            const string vcfLine = "1	13528	.	C	G,T	1771.54	VQSRTrancheSNP99.60to99.80	AC=21,11;AC_AFR=12,0;AC_AMR=1,0;AC_Adj=13,9;AC_EAS=0,0;AC_FIN=0,0;AC_Het=13,9,0;AC_Hom=0,0;AC_NFE=0,2;AC_OTH=0,0;AC_SAS=0,7;AF=6.036e-04,3.162e-04;AN=34792;AN_AFR=390;AN_AMR=116;AN_Adj=10426;AN_EAS=150;AN_FIN=8;AN_NFE=2614;AN_OTH=116;AN_SAS=7032;BaseQRankSum=1.23;ClippingRankSum=0.056;DP=144988;FS=0.000;GQ_MEAN=14.54;GQ_STDDEV=16.53;Het_AFR=12,0,0;Het_AMR=1,0,0;Het_EAS=0,0,0;Het_FIN=0,0,0;Het_NFE=0,2,0;Het_OTH=0,0,0;Het_SAS=0,7,0;Hom_AFR=0,0;Hom_AMR=0,0;Hom_EAS=0,0;Hom_FIN=0,0;Hom_NFE=0,0;Hom_OTH=0,0;Hom_SAS=0,0;InbreedingCoeff=0.0557;MQ=31.08;MQ0=0;MQRankSum=-5.410e-01;NCC=67387;QD=1.91;ReadPosRankSum=0.206;VQSLOD=-2.705e+00;culprit=MQ;DP_HIST=10573|1503|705|1265|2477|613|167|52|18|11|8|3|0|0|1|0|0|0|0|0,2|6|2|1|4|0|3|1|0|0|2|0|0|0|0|0|0|0|0|0,1|0|0|0|1|1|3|0|1|1|1|0|0|0|1|0|0|0|0|0;GQ_HIST=342|11195|83|56|3154|517|367|60|12|4|5|7|1373|180|15|16|1|0|1|8,0|0|1|0|1|0|3|1|0|1|2|0|1|2|0|1|1|0|1|6,0|1|0|0|1|1|0|0|1|0|0|1|1|1|1|0|0|0|0|2";

            var exacReader = new ExacReader(null,_refChromDict);
            var exacItems = exacReader.ExtractItems(vcfLine);

            var allAlleleNumber  = exacItems[0].AllAlleleNumber;
            var allAlleleCount   = exacItems[0].AllAlleleCount;
            var allAlleleNumber2 = exacItems[1].AllAlleleNumber;
            var allAlleleCount2  = exacItems[1].AllAlleleCount;

            Assert.NotNull(allAlleleNumber);
            Assert.NotNull(allAlleleCount);
            Assert.NotNull(allAlleleNumber2);
            Assert.NotNull(allAlleleCount2);

            Assert.Equal(10426, allAlleleNumber.Value);
            Assert.Equal(28, exacItems[0].Coverage);
            Assert.Equal("0.001247", (allAlleleCount.Value / (double)allAlleleNumber.Value).ToString(JsonCommon.FrequencyRoundingFormat));

            Assert.Equal(10426, allAlleleNumber2.Value);
            Assert.Equal(28, exacItems[1].Coverage);
            Assert.Equal("0.000863", (allAlleleCount2.Value / (double)allAlleleNumber2.Value).ToString(JsonCommon.FrequencyRoundingFormat));
        }
    }
}
