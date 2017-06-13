using System.Collections.Generic;
using SAUtils.DataStructures;
using UnitTests.Fixtures;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.SaUtilsTests.InputFileParsers
{
    [Collection("ChromosomeRenamer")]
    public sealed class EvsTests
    {
        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public EvsTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        [Fact(Skip = "new SA")]
        public void OneAltAlleleTest()
        {
            //const string vcfLine = "1	69428	rs140739101	T	G	.	PASS	BSNP=dbSNP_134;EA_AC=313,6535;AA_AC=14,3808;TAC=327,10343;MAF=4.5707,0.3663,3.0647;GTS=GG,GT,TT;EA_GTC=92,129,3203;AA_GTC=1,12,1898;GTC=93,141,5101;DP=110;GL=OR4F5;CP=1.0;CG=0.9;AA=T;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_001005484.1:missense;HGVS_CDNA_VAR=NM_001005484.1:c.338T>G;HGVS_PROTEIN_VAR=NM_001005484.1:p.(F113C);CDS_SIZES=NM_001005484.1:918;GS=205;PH=probably-damaging:0.999;EA_AGE=.;AA_AGE=.";

            //var evsReader = new EvsReader(_renamer);
            //var evsItem = evsReader.ExtractItems(vcfLine)[0];

            //var sa = new SupplementaryAnnotationPosition(69428);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //evsItem.SetSupplementaryAnnotations(saCreator);

            //// EA_AC=313,6535;
            //// AA_AC=14,3808;
            //// TAC=327,10343;

            //var evs =
            //    sa.AlleleSpecificAnnotations["G"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)] as EvsAnnotation;
            //Assert.NotNull(evs);
            //Assert.Equal("0.045707", evs.EvsEur);
            //Assert.Equal("0.003663", evs.EvsAfr);
            //Assert.Equal("0.030647", evs.EvsAll);
        }

        [Fact(Skip = "new SA")]
        public void EvsDepthFieldTest()
        {
            //const string vcfLine = "1	69428	rs140739101	T	G	.	PASS	BSNP=dbSNP_134;EA_AC=313,6535;AA_AC=14,3808;TAC=327,10343;MAF=4.5707,0.3663,3.0647;GTS=GG,GT,TT;EA_GTC=92,129,3203;AA_GTC=1,12,1898;GTC=93,141,5101;DP=110;GL=OR4F5;CP=1.0;CG=0.9;AA=T;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_001005484.1:missense;HGVS_CDNA_VAR=NM_001005484.1:c.338T>G;HGVS_PROTEIN_VAR=NM_001005484.1:p.(F113C);CDS_SIZES=NM_001005484.1:918;GS=205;PH=probably-damaging:0.999;EA_AGE=.;AA_AGE=.";

            //var evsReader = new EvsReader(_renamer);
            //var evsItem = evsReader.ExtractItems(vcfLine)[0];

            //var sa = new SupplementaryAnnotationPosition(69428);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //evsItem.SetSupplementaryAnnotations(saCreator);

            //var evs =
            //    sa.AlleleSpecificAnnotations["G"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)] as EvsAnnotation;
            //Assert.NotNull(evs);

            //Assert.Equal("110", evs.EvsCoverage);
        }

        [Fact(Skip = "new SA")]
        public void NumEvsSamplesTest()
        {
            //const string vcfLine = "1	1564952	rs112177324	TG	TGG,T	.	PASS	BSNP=dbSNP_132;EA_AC=2,3039,4701;AA_AC=44,279,3231;TAC=46,3318,7932;MAF=39.2793,9.0884,29.7805;GTS=A1A1,A1A2,A1R,A2A2,A2R,RR;EA_GTC=0,1,1,707,1624,1538;AA_GTC=4,4,32,41,193,1503;GTC=4,5,33,748,1817,3041;DP=10;GL=MIB2;CP=0.8;CG=-0.0;AA=.;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_080875.2:intron,NM_080875.2:intron,NM_001170689.1:intron,NM_001170689.1:intron,NM_001170688.1:intron,NM_001170688.1:intron,NM_001170687.1:intron,NM_001170687.1:intron,NM_001170686.1:intron,NM_001170686.1:intron;HGVS_CDNA_VAR=NM_080875.2:c.2908+7del1,NM_080875.2:c.2908+6_2908+7insG,NM_001170689.1:c.2187-66del1,NM_001170689.1:c.2187-67_2187-66insG,NM_001170688.1:c.2713+7del1,NM_001170688.1:c.2713+6_2713+7insG,NM_001170687.1:c.2866+7del1,NM_001170687.1:c.2866+6_2866+7insG,NM_001170686.1:c.2896+7del1,NM_001170686.1:c.2896+6_28967insG;HGVS_PROTEIN_VAR=.,.,.,.,.,.,.,.,.,.;CDS_SIZES=NM_080875.2:3213,NM_080875.2:3213,NM_001170689.1:2262,NM_001170689.1:2262,NM_001170688.1:3018,NM_001170688.1:3018,NM_001170687.1:3171,NM_001170687.1:3171,NM_001170686.1:3201,NM_001170686.1:3201;GS=.,.,.,.,.,.,.,.,.,.;PH=.,.,.,.,.,.,.,.,.,.;EA_AGE=.;AA_AGE=.";

            //var evsReader = new EvsReader(_renamer);
            //var evsItemsList = evsReader.ExtractItems(vcfLine);

            //var sa = new SupplementaryAnnotationPosition(1564953);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //var additionalItems = new List<SupplementaryDataItem>();

            //foreach (var evsItem in evsItemsList)
            //{
            //    additionalItems.Add(evsItem.SetSupplementaryAnnotations(saCreator));
            //}

            //foreach (var item in additionalItems)
            //{
            //    item.SetSupplementaryAnnotations(saCreator);
            //}

            //var evs =
            //    sa.AlleleSpecificAnnotations["iG"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)] as EvsAnnotation;
            //Assert.NotNull(evs);

            //Assert.Equal("5648", evs.NumEvsSamples);//GTC=4,5,33,748,1817,3041;
        }

        [Fact(Skip = "new SA")]
        public void MultiAltAlleleTest()
        {
            //const string vcfLine = "1	1564952	rs112177324	TG	TGG,T	.	PASS	BSNP=dbSNP_132;EA_AC=2,3039,4701;AA_AC=44,279,3231;TAC=46,3318,7932;MAF=39.2793,9.0884,29.7805;GTS=A1A1,A1A2,A1R,A2A2,A2R,RR;EA_GTC=0,1,1,707,1624,1538;AA_GTC=4,4,32,41,193,1503;GTC=4,5,33,748,1817,3041;DP=10;GL=MIB2;CP=0.8;CG=-0.0;AA=.;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_080875.2:intron,NM_080875.2:intron,NM_001170689.1:intron,NM_001170689.1:intron,NM_001170688.1:intron,NM_001170688.1:intron,NM_001170687.1:intron,NM_001170687.1:intron,NM_001170686.1:intron,NM_001170686.1:intron;HGVS_CDNA_VAR=NM_080875.2:c.2908+7del1,NM_080875.2:c.2908+6_2908+7insG,NM_001170689.1:c.2187-66del1,NM_001170689.1:c.2187-67_2187-66insG,NM_001170688.1:c.2713+7del1,NM_001170688.1:c.2713+6_2713+7insG,NM_001170687.1:c.2866+7del1,NM_001170687.1:c.2866+6_2866+7insG,NM_001170686.1:c.2896+7del1,NM_001170686.1:c.2896+6_28967insG;HGVS_PROTEIN_VAR=.,.,.,.,.,.,.,.,.,.;CDS_SIZES=NM_080875.2:3213,NM_080875.2:3213,NM_001170689.1:2262,NM_001170689.1:2262,NM_001170688.1:3018,NM_001170688.1:3018,NM_001170687.1:3171,NM_001170687.1:3171,NM_001170686.1:3201,NM_001170686.1:3201;GS=.,.,.,.,.,.,.,.,.,.;PH=.,.,.,.,.,.,.,.,.,.;EA_AGE=.;AA_AGE=.";

            //var evsReader = new EvsReader(_renamer);
            //var evsItemsList = evsReader.ExtractItems(vcfLine);

            //var sa = new SupplementaryAnnotationPosition(1564953);
            //var saCreator = new SupplementaryPositionCreator(sa);

            //var additionalItems = new List<SupplementaryDataItem>();

            //foreach (var evsItem in evsItemsList)
            //{
            //    additionalItems.Add(evsItem.SetSupplementaryAnnotations(saCreator));
            //}

            //foreach (var item in additionalItems)
            //{
            //    item.SetSupplementaryAnnotations(saCreator);
            //}

            //var evs1 =
            //    sa.AlleleSpecificAnnotations["1"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)] as EvsAnnotation;
            //Assert.NotNull(evs1);

            //var evsiG =
            //    sa.AlleleSpecificAnnotations["iG"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)] as EvsAnnotation;
            //Assert.NotNull(evsiG);

            //Assert.Equal("0.012380", evsiG.EvsAfr);
            //Assert.Equal("0.000258", evsiG.EvsEur);
            //Assert.Equal("0.004072", evsiG.EvsAll);

            //Assert.Equal("0.078503", evs1.EvsAfr);
            //Assert.Equal("0.392534", evs1.EvsEur);
            //Assert.Equal("0.293732", evs1.EvsAll);
        }

        [Fact]
        public void EqualityAndHash()
        {
            var evsItem = new EvsItem("chr1", 100, "rs101", "A", "C", "0.1", "0.1", "0.1", "0.1", "100");

            var evsHash = new HashSet<EvsItem> { evsItem };

            Assert.Equal(1, evsHash.Count);
            Assert.True(evsHash.Contains(evsItem));
        }
    }
}
