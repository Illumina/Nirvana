using System.Collections.Generic;
using System.IO;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.EVS;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class EvsTests
    {
        private readonly IDictionary<string, IChromosome> _refChromDict;

        public EvsTests()
        {
            _refChromDict = new Dictionary<string, IChromosome>
            {
                {"1",new Chromosome("chr1", "1",0) },
                {"4",new Chromosome("chr4", "4", 3) }
            };
        }

        [Fact]
        public void OneAltAlleleTest()
        {
            const string vcfLine = "1	69428	rs140739101	T	G	.	PASS	BSNP=dbSNP_134;EA_AC=313,6535;AA_AC=14,3808;TAC=327,10343;MAF=4.5707,0.3663,3.0647;GTS=GG,GT,TT;EA_GTC=92,129,3203;AA_GTC=1,12,1898;GTC=93,141,5101;DP=110;GL=OR4F5;CP=1.0;CG=0.9;AA=T;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_001005484.1:missense;HGVS_CDNA_VAR=NM_001005484.1:c.338T>G;HGVS_PROTEIN_VAR=NM_001005484.1:p.(F113C);CDS_SIZES=NM_001005484.1:918;GS=205;PH=probably-damaging:0.999;EA_AGE=.;AA_AGE=.";

            var fileInfo = new StreamReader(new MemoryStream());
            var evsReader = new EvsReader(fileInfo, _refChromDict);

            var evs = evsReader.ExtractItems(vcfLine)[0];

            Assert.NotNull(evs);
            const string expectedRes = "\"sampleCount\":5335,\"coverage\":110,\"allAf\":0.030647,\"afrAf\":0.003663,\"eurAf\":0.045707";
            Assert.Equal(expectedRes, evs.GetJsonString());

        }

        [Fact]
        public void EvsDepthFieldTest()
        {
            const string vcfLine = "1	69428	rs140739101	T	G	.	PASS	BSNP=dbSNP_134;EA_AC=313,6535;AA_AC=14,3808;TAC=327,10343;MAF=4.5707,0.3663,3.0647;GTS=GG,GT,TT;EA_GTC=92,129,3203;AA_GTC=1,12,1898;GTC=93,141,5101;DP=110;GL=OR4F5;CP=1.0;CG=0.9;AA=T;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_001005484.1:missense;HGVS_CDNA_VAR=NM_001005484.1:c.338T>G;HGVS_PROTEIN_VAR=NM_001005484.1:p.(F113C);CDS_SIZES=NM_001005484.1:918;GS=205;PH=probably-damaging:0.999;EA_AGE=.;AA_AGE=.";

            var fileInfo = new StreamReader(new MemoryStream());
            var evsReader = new EvsReader(fileInfo, _refChromDict);

            var evs = evsReader.ExtractItems(vcfLine)[0];

            Assert.NotNull(evs);
            const string expectedRes = "\"sampleCount\":5335,\"coverage\":110,\"allAf\":0.030647,\"afrAf\":0.003663,\"eurAf\":0.045707";
            Assert.Equal(expectedRes, evs.GetJsonString());
        }

        [Fact]
        public void NumEvsSamplesTest()
        {
            const string vcfLine = "1	1564952	rs112177324	TG	TGG,T	.	PASS	BSNP=dbSNP_132;EA_AC=2,3039,4701;AA_AC=44,279,3231;TAC=46,3318,7932;MAF=39.2793,9.0884,29.7805;GTS=A1A1,A1A2,A1R,A2A2,A2R,RR;EA_GTC=0,1,1,707,1624,1538;AA_GTC=4,4,32,41,193,1503;GTC=4,5,33,748,1817,3041;DP=10;GL=MIB2;CP=0.8;CG=-0.0;AA=.;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_080875.2:intron,NM_080875.2:intron,NM_001170689.1:intron,NM_001170689.1:intron,NM_001170688.1:intron,NM_001170688.1:intron,NM_001170687.1:intron,NM_001170687.1:intron,NM_001170686.1:intron,NM_001170686.1:intron;HGVS_CDNA_VAR=NM_080875.2:c.2908+7del1,NM_080875.2:c.2908+6_2908+7insG,NM_001170689.1:c.2187-66del1,NM_001170689.1:c.2187-67_2187-66insG,NM_001170688.1:c.2713+7del1,NM_001170688.1:c.2713+6_2713+7insG,NM_001170687.1:c.2866+7del1,NM_001170687.1:c.2866+6_2866+7insG,NM_001170686.1:c.2896+7del1,NM_001170686.1:c.2896+6_28967insG;HGVS_PROTEIN_VAR=.,.,.,.,.,.,.,.,.,.;CDS_SIZES=NM_080875.2:3213,NM_080875.2:3213,NM_001170689.1:2262,NM_001170689.1:2262,NM_001170688.1:3018,NM_001170688.1:3018,NM_001170687.1:3171,NM_001170687.1:3171,NM_001170686.1:3201,NM_001170686.1:3201;GS=.,.,.,.,.,.,.,.,.,.;PH=.,.,.,.,.,.,.,.,.,.;EA_AGE=.;AA_AGE=.";

            var fileInfo = new StreamReader(new MemoryStream());
            var evsReader = new EvsReader(fileInfo, _refChromDict);

            var evs = evsReader.ExtractItems(vcfLine);

            Assert.NotNull(evs);
            Assert.Equal(2, evs.Count);
            const string expectedRes1 = "\"sampleCount\":5648,\"coverage\":10,\"allAf\":0.004072,\"afrAf\":0.012380,\"eurAf\":0.000258";
            Assert.Equal(expectedRes1, evs[0].GetJsonString());
            Assert.Equal("TGG", evs[0].AlternateAllele);

            const string expectedRes2 = "\"sampleCount\":5648,\"coverage\":10,\"allAf\":0.293732,\"afrAf\":0.078503,\"eurAf\":0.392534";
            Assert.Equal(expectedRes2, evs[1].GetJsonString());
            Assert.Equal("T", evs[1].AlternateAllele);
        }

        [Fact]
        public void EqualityAndHash()
        {
            var evsItem = new EvsItem(new Chromosome("chr1", "1", 0), 100, "rs101", "A", "C", "0.1", "0.1", "0.1", "0.1", "100");

            var evsHash = new HashSet<EvsItem> { evsItem };

            Assert.Single(evsHash);
            Assert.Contains(evsItem, evsHash);
        }
    }
}
