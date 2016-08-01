using System.Collections.Generic;
using Illumina.VariantAnnotation.DataStructures.SupplementaryAnnotations;
using InputFileParsers.Evs;
using InputFileParsers.SupplementaryData;
using Xunit;

namespace NirvanaUnitTests.FileHandling
{
    public sealed class EvsTests
    {
        [Fact]
        public void OneAltAlleleTest()
        {
            const string vcfLine = "1\t69428\trs140739101\tT\tG\t.\tPASS\tBSNP=dbSNP_134;EA_AC=313,6535;AA_AC=14,3808;TAC=327,10343;MAF=4.5707,0.3663,3.0647;GTS=GG,GT,TT;EA_GTC=92,129,3203;AA_GTC=1,12,1898;GTC=93,141,5101;DP=110;GL=OR4F5;CP=1.0;CG=0.9;AA=T;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_001005484.1:missense;HGVS_CDNA_VAR=NM_001005484.1:c.338T>G;HGVS_PROTEIN_VAR=NM_001005484.1:p.(F113C);CDS_SIZES=NM_001005484.1:918;GS=205;PH=probably-damaging:0.999;EA_AGE=.;AA_AGE=.";

            var evsItem = EvsReader.ExtractEvsItem(vcfLine)[0];

            var sa = new SupplementaryAnnotation();

            evsItem.SetSupplementaryAnnotations(sa);

            // EA_AC=313,6535;
            // AA_AC=14,3808;
            // TAC=327,10343;
            Assert.Equal("0.045707", sa.AlleleSpecificAnnotations["G"].EvsEur);
            Assert.Equal("0.003663", sa.AlleleSpecificAnnotations["G"].EvsAfr);
            Assert.Equal("0.030647", sa.AlleleSpecificAnnotations["G"].EvsAll);
        }

        [Fact]
        public void EvsDepthFieldTest()
        {
            const string vcfLine = "1\t69428\trs140739101\tT\tG\t.\tPASS\tBSNP=dbSNP_134;EA_AC=313,6535;AA_AC=14,3808;TAC=327,10343;MAF=4.5707,0.3663,3.0647;GTS=GG,GT,TT;EA_GTC=92,129,3203;AA_GTC=1,12,1898;GTC=93,141,5101;DP=110;GL=OR4F5;CP=1.0;CG=0.9;AA=T;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_001005484.1:missense;HGVS_CDNA_VAR=NM_001005484.1:c.338T>G;HGVS_PROTEIN_VAR=NM_001005484.1:p.(F113C);CDS_SIZES=NM_001005484.1:918;GS=205;PH=probably-damaging:0.999;EA_AGE=.;AA_AGE=.";

            var evsItem = EvsReader.ExtractEvsItem(vcfLine)[0];

            var sa= new SupplementaryAnnotation();
            
            evsItem.SetSupplementaryAnnotations(sa);

            Assert.Equal("110", sa.AlleleSpecificAnnotations["G"].EvsCoverage);
        }

        [Fact]
        public void NumEvsSamplesTest()
        {
            const string vcfLine = "1\t1564952\trs112177324\tTG\tTGG,T\t.\tPASS\tBSNP=dbSNP_132;EA_AC=2,3039,4701;AA_AC=44,279,3231;TAC=46,3318,7932;MAF=39.2793,9.0884,29.7805;GTS=A1A1,A1A2,A1R,A2A2,A2R,RR;EA_GTC=0,1,1,707,1624,1538;AA_GTC=4,4,32,41,193,1503;GTC=4,5,33,748,1817,3041;DP=10;GL=MIB2;CP=0.8;CG=-0.0;AA=.;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_080875.2:intron,NM_080875.2:intron,NM_001170689.1:intron,NM_001170689.1:intron,NM_001170688.1:intron,NM_001170688.1:intron,NM_001170687.1:intron,NM_001170687.1:intron,NM_001170686.1:intron,NM_001170686.1:intron;HGVS_CDNA_VAR=NM_080875.2:c.2908+7del1,NM_080875.2:c.2908+6_2908+7insG,NM_001170689.1:c.2187-66del1,NM_001170689.1:c.2187-67_2187-66insG,NM_001170688.1:c.2713+7del1,NM_001170688.1:c.2713+6_2713+7insG,NM_001170687.1:c.2866+7del1,NM_001170687.1:c.2866+6_2866+7insG,NM_001170686.1:c.2896+7del1,NM_001170686.1:c.2896+6_28967insG;HGVS_PROTEIN_VAR=.,.,.,.,.,.,.,.,.,.;CDS_SIZES=NM_080875.2:3213,NM_080875.2:3213,NM_001170689.1:2262,NM_001170689.1:2262,NM_001170688.1:3018,NM_001170688.1:3018,NM_001170687.1:3171,NM_001170687.1:3171,NM_001170686.1:3201,NM_001170686.1:3201;GS=.,.,.,.,.,.,.,.,.,.;PH=.,.,.,.,.,.,.,.,.,.;EA_AGE=.;AA_AGE=.";

            var evsItemsList = EvsReader.ExtractEvsItem(vcfLine);

            var sa = new SupplementaryAnnotation();
	        var additionalItems = new List<ISupplementaryDataItem>();

	        foreach (var evsItem in evsItemsList)
	        {
		        additionalItems.Add(evsItem.SetSupplementaryAnnotations(sa));
	        }
            
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa);
            }

            Assert.Equal("5648", sa.AlleleSpecificAnnotations["iG"].NumEvsSamples);//GTC=4,5,33,748,1817,3041;

        }

        [Fact]
        public void MultiAltAlleleTest()
        {
            const string vcfLine = "1\t1564952\trs112177324\tTG\tTGG,T\t.\tPASS\tBSNP=dbSNP_132;EA_AC=2,3039,4701;AA_AC=44,279,3231;TAC=46,3318,7932;MAF=39.2793,9.0884,29.7805;GTS=A1A1,A1A2,A1R,A2A2,A2R,RR;EA_GTC=0,1,1,707,1624,1538;AA_GTC=4,4,32,41,193,1503;GTC=4,5,33,748,1817,3041;DP=10;GL=MIB2;CP=0.8;CG=-0.0;AA=.;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_080875.2:intron,NM_080875.2:intron,NM_001170689.1:intron,NM_001170689.1:intron,NM_001170688.1:intron,NM_001170688.1:intron,NM_001170687.1:intron,NM_001170687.1:intron,NM_001170686.1:intron,NM_001170686.1:intron;HGVS_CDNA_VAR=NM_080875.2:c.2908+7del1,NM_080875.2:c.2908+6_2908+7insG,NM_001170689.1:c.2187-66del1,NM_001170689.1:c.2187-67_2187-66insG,NM_001170688.1:c.2713+7del1,NM_001170688.1:c.2713+6_2713+7insG,NM_001170687.1:c.2866+7del1,NM_001170687.1:c.2866+6_2866+7insG,NM_001170686.1:c.2896+7del1,NM_001170686.1:c.2896+6_28967insG;HGVS_PROTEIN_VAR=.,.,.,.,.,.,.,.,.,.;CDS_SIZES=NM_080875.2:3213,NM_080875.2:3213,NM_001170689.1:2262,NM_001170689.1:2262,NM_001170688.1:3018,NM_001170688.1:3018,NM_001170687.1:3171,NM_001170687.1:3171,NM_001170686.1:3201,NM_001170686.1:3201;GS=.,.,.,.,.,.,.,.,.,.;PH=.,.,.,.,.,.,.,.,.,.;EA_AGE=.;AA_AGE=.";

			var evsItemsList = EvsReader.ExtractEvsItem(vcfLine);

			var sa = new SupplementaryAnnotation();
			var additionalItems = new List<ISupplementaryDataItem>();

			foreach (var evsItem in evsItemsList)
			{
				additionalItems.Add(evsItem.SetSupplementaryAnnotations(sa));
			}

			foreach (var item in additionalItems)
			{
				item.SetSupplementaryAnnotations(sa);
			}

            // EA_AC =2,3039,4701;
            // AA_AC =44,279,3231;
            // TAC   =46,3318,7932;

            Assert.Equal( "0.012380",sa.AlleleSpecificAnnotations["iG"].EvsAfr);
            Assert.Equal("0.000258", sa.AlleleSpecificAnnotations["iG"].EvsEur);
            Assert.Equal("0.004072", sa.AlleleSpecificAnnotations["iG"].EvsAll );

            Assert.Equal("0.078503", sa.AlleleSpecificAnnotations["1"].EvsAfr);
            Assert.Equal("0.392534", sa.AlleleSpecificAnnotations["1"].EvsEur);
            Assert.Equal("0.293732", sa.AlleleSpecificAnnotations["1"].EvsAll);
            

        }
    }
}
