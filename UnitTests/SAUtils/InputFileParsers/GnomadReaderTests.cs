using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers;
using VariantAnnotation.Interface.SA;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class GnomadReaderTests
    {
        private static readonly IChromosome Chrom3 = new Chromosome("chr3", "3", 3);
        private static readonly IChromosome Chrom22 = new Chromosome("chr22", "22", 22);

        private readonly Dictionary<string, IChromosome> _chromDict = new Dictionary<string, IChromosome>()
        {
            { "3", Chrom3},
            { "22", Chrom22}
        };

        private Stream GetGnomadStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##gnomAD");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("3\t60044\t.\tG\tA\t681.98\tRF\tAC=2;AF=3.72995e-04;AN=5362;BaseQRankSum=7.20000e-01;ClippingRankSum=-2.59000e-01;DP=140804;FS=3.05900e+00;InbreedingCoeff=-1.50000e-02;MQ=4.51200e+01;MQRankSum=3.58000e-01;QD=8.02000e+00;ReadPosRankSum=3.58000e-01;SOR=2.34300e+00;VQSLOD=-3.03600e+01;VQSR_culprit=MQ;GQ_HIST_ALT=0|1|0|0|2|0|0|1|1|2|3|0|0|0|1|1|0|0|0|2;DP_HIST_ALT=4|8|2|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0;AB_HIST_ALT=0|0|0|0|0|1|1|0|4|0|3|0|2|2|0|0|0|0|0|0;GQ_HIST_ALL=356|1720|1644|4096|3691|1281|1520|726|165|157|56|12|10|0|3|1|2|0|0|2;DP_HIST_ALL=3567|9159|2469|232|13|2|0|0|0|0|0|0|0|0|0|0|0|0|0|0;AB_HIST_ALL=0|0|0|0|0|1|1|0|4|0|3|0|2|2|0|0|0|0|0|0;AC_Male=1;AC_Female=1;AN_Male=3092;AN_Female=2270;AF_Male=3.23415e-04;AF_Female=4.40529e-04;GC_Male=1545,1,0;GC_Female=1134,1,0;GC_raw=15428,13,1;AC_raw=15;AN_raw=30884;GC=2679,2,0;AF_raw=4.85688e-04;Hom_AFR=0;Hom_AMR=0;Hom_ASJ=0;Hom_EAS=0;Hom_FIN=0;Hom_NFE=0;Hom_OTH=0;Hom=0;Hom_raw=1;AC_AFR=2;AC_AMR=0;AC_ASJ=0;AC_EAS=0;AC_FIN=0;AC_NFE=0;AC_OTH=0;AN_AFR=986;AN_AMR=166;AN_ASJ=56;AN_EAS=266;AN_FIN=664;AN_NFE=3064;AN_OTH=160;AF_AFR=2.02840e-03;AF_AMR=0.00000e+00;AF_ASJ=0.00000e+00;AF_EAS=0.00000e+00;AF_FIN=0.00000e+00;AF_NFE=0.00000e+00;AF_OTH=0.00000e+00;POPMAX=AFR;AC_POPMAX=2;AN_POPMAX=986;AF_POPMAX=2.02840e-03;DP_MEDIAN=5;DREF_MEDIAN=3.24648e-08;GQ_MEDIAN=49;AB_MEDIAN=5.00000e-01;AS_RF=4.70108e-02;AS_FilterStatus=RF;CSQ=A|intergenic_variant|MODIFIER||||||||||||||||1||||SNV|1||||||||||||||||||||||||||||||||||||||||||||;GC_AFR=491,2,0;GC_AMR=83,0,0;GC_ASJ=28,0,0;GC_EAS=133,0,0;GC_FIN=332,0,0;GC_NFE=1532,0,0;GC_OTH=80,0,0;Hom_Male=0;Hom_Female=0");
            writer.WriteLine("3\t60054\t.\tG\tC\t307.83\tRF\tAC=1;AF=1.20948e-04;AN=8268;BaseQRankSum=-3.65000e-01;ClippingRankSum=-7.79000e-01;DP=161136;FS=0.00000e+00;InbreedingCoeff=-9.00000e-03;MQ=4.59800e+01;MQRankSum=-7.79000e-01;QD=8.55000e+00;ReadPosRankSum=-3.65000e-01;SOR=8.18000e-01;VQSLOD=-1.37500e+01;VQSR_culprit=MQ;GQ_HIST_ALT=0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|1;DP_HIST_ALT=0|0|1|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0;AB_HIST_ALT=0|0|0|0|0|0|0|0|1|0|0|0|0|0|0|0|0|0|0|0;GQ_HIST_ALL=244|1135|1221|3500|3735|1551|2066|1155|322|325|135|33|31|7|7|0|1|0|1|1;DP_HIST_ALL=2416|8852|3644|508|47|2|1|0|0|0|0|0|0|0|0|0|0|0|0|0;AB_HIST_ALL=0|0|0|0|0|0|0|0|1|0|0|0|0|0|0|0|0|0|0|0;AC_Male=0;AC_Female=1;AN_Male=4754;AN_Female=3514;AF_Male=0.00000e+00;AF_Female=2.84576e-04;GC_Male=2377,0,0;GC_Female=1756,1,0;GC_raw=15469,1,0;AC_raw=1;AN_raw=30940;GC=4133,1,0;AF_raw=3.23206e-05;Hom_AFR=0;Hom_AMR=0;Hom_ASJ=0;Hom_EAS=0;Hom_FIN=0;Hom_NFE=0;Hom_OTH=0;Hom=0;Hom_raw=0;AC_AFR=0;AC_AMR=0;AC_ASJ=0;AC_EAS=0;AC_FIN=0;AC_NFE=1;AC_OTH=0;AN_AFR=1574;AN_AMR=262;AN_ASJ=94;AN_EAS=446;AN_FIN=1000;AN_NFE=4638;AN_OTH=254;AF_AFR=0.00000e+00;AF_AMR=0.00000e+00;AF_ASJ=0.00000e+00;AF_EAS=0.00000e+00;AF_FIN=0.00000e+00;AF_NFE=2.15610e-04;AF_OTH=0.00000e+00;POPMAX=NFE;AC_POPMAX=1;AN_POPMAX=4638;AF_POPMAX=2.15610e-04;DP_MEDIAN=10;DREF_MEDIAN=1.00000e-10;GQ_MEDIAN=99;AB_MEDIAN=4.00000e-01;AS_RF=2.65124e-01;AS_FilterStatus=RF;CSQ=C|intergenic_variant|MODIFIER||||||||||||||||1||||SNV|1||||||||||||||||||||||||||||||||||||||||||||;GC_AFR=787,0,0;GC_AMR=131,0,0;GC_ASJ=47,0,0;GC_EAS=223,0,0;GC_FIN=500,0,0;GC_NFE=2318,1,0;GC_OTH=127,0,0;Hom_Male=0;Hom_Female=0");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        
        [Fact]
        public void GetItems_test()
        {
            var gnomadReader = new GnomadReader(new StreamReader(GetGnomadStream()), _chromDict);

            var items = gnomadReader.GetItems().ToList();

            Assert.Equal(2, items.Count);
            Assert.Equal("\"coverage\":26,\"failedFilter\":true,\"allAf\":0.000373,\"allAn\":5362,\"allAc\":2,\"allHc\":0,\"afrAf\":0.002028,\"afrAn\":986,\"afrAc\":2,\"afrHc\":0,\"amrAf\":0,\"amrAn\":166,\"amrAc\":0,\"amrHc\":0,\"easAf\":0,\"easAn\":266,\"easAc\":0,\"easHc\":0,\"finAf\":0,\"finAn\":664,\"finAc\":0,\"finHc\":0,\"nfeAf\":0,\"nfeAn\":3064,\"nfeAc\":0,\"nfeHc\":0,\"asjAf\":0,\"asjAn\":56,\"asjAc\":0,\"asjHc\":0,\"othAf\":0,\"othAn\":160,\"othAc\":0,\"othHc\":0", items[0].GetJsonString());
        }
        private Stream GetConflictingItemsStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("##gnomAD");
            writer.WriteLine("#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO");
            writer.WriteLine("22\t16558315\trs369787349\tT\tC,G,T,ACTGGCTGCCTGGCTTG\t818363\tAC0;LCR;RF;SEGDUP\tAC=87,7,0,2;AF=5.30488e-01,4.26829e-02,0.00000e+00,1.21951e-02;AN=164;AC_AFR=31,1,0,2;AC_AMR=3,0,0,0;AC_ASJ=0,0,0,0;AC_EAS=4,0,0,0;AC_FIN=33,5,0,0;AC_NFE=13,1,0,0;AC_OTH=3,0,0,0;AC_Male=40,1,0,0;AC_Female=47,6,0,2;AN_AFR=56;AN_AMR=4;AN_ASJ=0;AN_EAS=6;AN_FIN=64;AN_NFE=28;AN_OTH=6;AN_Male=78;AN_Female=86;AF_AFR=5.53571e-01,1.78571e-02,0.00000e+00,3.57143e-02;AF_AMR=7.50000e-01,0.00000e+00,0.00000e+00,0.00000e+00;AF_ASJ=.,.,.,.;AF_EAS=6.66667e-01,0.00000e+00,0.00000e+00,0.00000e+00;AF_FIN=5.15625e-01,7.81250e-02,0.00000e+00,0.00000e+00;AF_NFE=4.64286e-01,3.57143e-02,0.00000e+00,0.00000e+00;AF_OTH=5.00000e-01,0.00000e+00,0.00000e+00,0.00000e+00;AF_Male=5.12821e-01,1.28205e-02,0.00000e+00,0.00000e+00;AF_Female=5.46512e-01,6.97674e-02,0.00000e+00,2.32558e-02;GC_AFR=3,16,7,0,1,0,0,0,0,0,0,0,0,0,1;GC_AMR=0,1,1,0,0,0,0,0,0,0,0,0,0,0,0;GC_ASJ=0,0,0,0,0,0,0,0,0,0,0,0,0,0,0;GC_EAS=0,2,1,0,0,0,0,0,0,0,0,0,0,0,0;GC_FIN=2,18,6,0,3,1,0,0,0,0,0,0,0,0,0;GC_NFE=3,8,2,0,1,0,0,0,0,0,0,0,0,0,0;GC_OTH=1,1,1,0,0,0,0,0,0,0,0,0,0,0,0;GC_Male=6,23,8,0,1,0,0,0,0,0,0,0,0,0,0;GC_Female=3,23,10,0,4,1,0,0,0,0,0,0,0,0,1;AC_raw=7179,402,23,4;AN_raw=13956;AF_raw=5.14402e-01,2.88048e-02,1.64804e-03,2.86615e-04;GC_raw=2158,1885,2598,68,90,122,3,8,0,6,0,0,0,0,2;GC=9,46,18,0,5,1,0,0,0,0,0,0,0,0,1;AC_POPMAX=3,5,.,2;AN_POPMAX=4,64,.,56;AF_POPMAX=7.50000e-01,7.81250e-02,.,3.57143e-02");
            writer.WriteLine("22\t16558315\trs376808508\tTAAGCCAGCCAGCCAGCCAAGCTGGCCAAGCCAGACAGGCAGCCAAGCCAACCAAGACACCCAGGCAGCCAAGCCAGC\tCAAGCCAGCCAGCCAGCCAAGCTGGCCAAGCCAGACAGGCAGCCAAGCCAACCAAGACACCCAGGCAGCCAAGCCAGC,T\t3.62825e+06\tLCR;RF;SEGDUP\tAC=155,1;AF=9.63451e-03,6.21581e-05;AN=16088;AC_AFR=46,1;AC_AMR=6,0;AC_ASJ=1,0;AC_EAS=3,0;AC_FIN=27,0;AC_NFE=67,0;AC_OTH=5,0;AC_Male=83,1;AC_Female=72,0;AN_AFR=3744;AN_AMR=534;AN_ASJ=186;AN_EAS=986;AN_FIN=1770;AN_NFE=8370;AN_OTH=498;AN_Male=8994;AN_Female=7094;AF_AFR=1.22863e-02,2.67094e-04;AF_AMR=1.12360e-02,0.00000e+00;AF_ASJ=5.37634e-03,0.00000e+00;AF_EAS=3.04260e-03,0.00000e+00;AF_FIN=1.52542e-02,0.00000e+00;AF_NFE=8.00478e-03,0.00000e+00;AF_OTH=1.00402e-02,0.00000e+00;AF_Male=9.22837e-03,1.11185e-04;AF_Female=1.01494e-02,0.00000e+00;GC_AFR=602,46,0,1,0,0;GC_AMR=64,6,0,0,0,0;GC_ASJ=20,1,0,0,0,0;GC_EAS=204,3,0,0,0,0;GC_FIN=255,23,2,0,0,0;GC_NFE=1083,51,8,0,0,0;GC_OTH=59,5,0,0,0,0;GC_Male=1304,71,6,1,0,0;GC_Female=983,64,4,0,0,0;AC_raw=413,1;AN_raw=28686;AF_raw=1.43973e-02,3.48602e-05;GC_raw=7802,349,30,1,0,0;GC=2287,135,10,1,0,0;AC_POPMAX=27,1;AN_POPMAX=1770,3744;AF_POPMAX=1.52542e-02,2.67094e-04");

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void IdentifyConflictingItems()
        {
            var gnomadReader = new GnomadReader(new StreamReader(GetConflictingItemsStream()), _chromDict);

            var items = new List<ISupplementaryDataItem>();
            foreach (GnomadItem item in gnomadReader.GetItems())
            {
                item.Trim();
                if (item.Position== 16558315)
                    items.Add(item);
            }

            items = SuppDataUtilities.RemoveConflictingAlleles(items);

            //two if the items were removed as conflicting items
            Assert.Equal(3,items.Count);
        }
    }
}