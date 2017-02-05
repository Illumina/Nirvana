using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.InputFileParsers.ClinVar;
using SAUtils.InputFileParsers.DbSnp;
using SAUtils.InputFileParsers.EVS;
using SAUtils.InputFileParsers.ExAc;
using SAUtils.InputFileParsers.OneKGen;
using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures.CompressedSequence;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.JSON;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("ChromosomeRenamer")]
    public sealed class MergeSaTests
    {
        private readonly OneKGenReader _oneKGenReader;
        private readonly ChromosomeRenamer _renamer;
        private readonly ICompressedSequence _sequence;
        private readonly CompressedSequenceReader _reader;

        /// <summary>
        /// constructor
        /// </summary>
        public MergeSaTests(ChromosomeRenamerFixture fixture)
        {
            _renamer       = fixture.Renamer;
            _sequence      = fixture.Sequence;
            _reader        = fixture.Reader;
            _oneKGenReader = new OneKGenReader(_renamer);
        }

        [Fact]
        public void MergeDbSnpItems()
        {
            const string vcfLine1 = "1	10228	rs143255646	TA	T	.	.	RS=143255646;RSPOS=10229;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000020005000002000200;WGT=1;VC=DIV;R5;ASP";
            const string vcfLine2 = "1	10228	rs200462216	TAACCCCTAACCCTAACCCTAAACCCTA	T	.	.	RS=200462216;RSPOS=10229;dbSNPBuildID=137;SSR=0;SAO=0;VP=0x050000020005000002000200;WGT=1;VC=DIV;R5;ASP";

            var sa = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(10229));
            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItem1 = dbsnpReader.ExtractItem(vcfLine1)[0];
            var dbSnpItem2 = dbsnpReader.ExtractItem(vcfLine2)[0];

            var additionalItems = new List<SupplementaryDataItem>
            {
                dbSnpItem1.SetSupplementaryAnnotations(sa),
                dbSnpItem2.SetSupplementaryAnnotations(sa)
            };

            //sa.Clear();
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa);
            }

            var dbSnp1 =
                sa.SaPosition.AlleleSpecificAnnotations["1"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as
                    DbSnpAnnotation;
            Assert.NotNull(dbSnp1);

            var dbSnp27 =
                sa.SaPosition.AlleleSpecificAnnotations["27"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as
                    DbSnpAnnotation;
            Assert.NotNull(dbSnp27);

            Assert.Equal(dbSnp1.DbSnp, new List<long> { 143255646 });
            Assert.Equal(dbSnp27.DbSnp, new List<long> { 200462216 });
        }

        [Fact]
        public void MergeMultipleDbSnpItems()
        {
            const string vcfLine1 =
                "1	1469597	rs3118506	GCG	GC,GG	.	.	RS=3118506;RSPOS=1469598;RV;dbSNPBuildID=103;SSR=0;SAO=0;VP=0x050000800005000002000110;WGT=1;VC=SNV;U3;ASP;NOC";
            const string vcfLine2 =
                "1	1469598	rs368645009	CG	C	.	.	RS=368645009;RSPOS=1469599;RV;dbSNPBuildID=138;SSR=0;SAO=0;VP=0x050000800005000002000200;WGT=1;VC=DIV;U3;ASP";

            var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(1469599));
            var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(1469599));

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItems = dbsnpReader.ExtractItem(vcfLine1);
            var dbSnpItem2 = dbsnpReader.ExtractItem(vcfLine2)[0];

            var additionalItems = dbSnpItems.Select(dbSnpItem => dbSnpItem.SetSupplementaryAnnotations(sa1)).ToList();

            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa1);
            }

            additionalItems.Clear();
            additionalItems.Add(dbSnpItem2.SetSupplementaryAnnotations(sa2));

            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa2);
            }

            sa1.MergeSaCreator(sa2);

            var expectedDbSnp = new List<long> { 3118506, 368645009 };

            var dbSnp =
                sa1.SaPosition.AlleleSpecificAnnotations["1"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as
                    DbSnpAnnotation;
            Assert.NotNull(dbSnp);

            Assert.Equal(expectedDbSnp, dbSnp.DbSnp);
        }

        [Fact]
        public void MultipleDbsnpMerge()
        {
            // NIR-778, 805. The second dbSNP id is missing from the SA database.
            const string vcfLine1 =
                "17	3616153	rs34081014	C	G	.	.	RS=34081014;RSPOS=3616153;dbSNPBuildID=126;SSR=0;SAO=0;VP=0x050000000005140136000100;WGT=1;VC=SNV;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=0.9297,0.07029;COMMON=1";

            const string vcfLine2 =
                "17	3616152	rs71362546	GCTG	GCTT,GGTG	.	.	RS=71362546;RSPOS=3616153;dbSNPBuildID=130;SSR=0;SAO=0;VP=0x050100000005000102000810;WGT=1;VC=MNV;SLO;ASP;GNO;NOC";

            var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(3616153));
            var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(3616153));
            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItem1 = dbsnpReader.ExtractItem(vcfLine1)[0];
            var dbSnpItems = dbsnpReader.ExtractItem(vcfLine2);

            dbSnpItem1.SetSupplementaryAnnotations(sa1);

            var additionalItems = dbSnpItems.Select(dbSnpItem => dbSnpItem.SetSupplementaryAnnotations(sa1)).ToList();

            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa2);
            }

            sa1.MergeSaCreator(sa2);

            var dbSnp =
                sa1.SaPosition.AlleleSpecificAnnotations["G"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as
                    DbSnpAnnotation;
            Assert.NotNull(dbSnp);
            Assert.Equal(dbSnp.DbSnp, new List<long> { 34081014, 71362546 });

        }

        [Fact]
        public void MergeSnvAndDeletion()
        {
            // NIR-906
            const string vcfLine1 =
                "1	862389	rs6693546	A	G	.	.	RS=6693546;RSPOS=862389;dbSNPBuildID=116;SSR=0;SAO=0;VP=0x05010008000515013e000100;WGT=1;VC=SNV;SLO;INT;ASP;VLD;G5;GNO;KGPhase1;KGPhase3;CAF=0.3171,0.6829;COMMON=1";
            const string vcfLine2 = "1	862388	rs534606253	GA	G	.	.	RS=534606253;RSPOS=862389;dbSNPBuildID=142;SSR=0;SAO=0;VP=0x050000080005040024000200;WGT=1;VC=DIV;INT;ASP;VLD;KGPhase3;CAF=0.996,0.003994;COMMON=1";

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpEntry1 = dbsnpReader.ExtractItem(vcfLine1)[0];
            var dbSnpEntry2 = dbsnpReader.ExtractItem(vcfLine2)[0];

            var sa = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(862389));
            dbSnpEntry1.SetSupplementaryAnnotations(sa);

            var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(862389));
            var additionalEntry = dbSnpEntry2.SetSupplementaryAnnotations(sa2);

            additionalEntry.SetSupplementaryAnnotations(sa2);

            sa.MergeSaCreator(sa2);
            sa.FinalizePositionalAnnotations();

            Assert.Equal("G", sa.SaPosition.GlobalMajorAllele);
            Assert.Equal("0.6829", sa.SaPosition.GlobalMajorAlleleFrequency);
            Assert.Equal("A", sa.SaPosition.GlobalMinorAllele);
            Assert.Equal("0.3171", sa.SaPosition.GlobalMinorAlleleFrequency);

        }

        [Fact]
        public void DeletionAndSnvMerge()
        {
            // NIR-906
            const string vcfLine1 =
                "2	193187631	rs774176075	TGTTG	T	.	.	RS=774176075;RSPOS=193187632;dbSNPBuildID=144;SSR=0;SAO=0;VP=0x050000000005000002000200;WGT=1;VC=DIV;ASP";
            const string vcfLine2 = "2	193187632	rs2592266	G	T	.	.	RS=2592266;RSPOS=193187632;dbSNPBuildID=100;SSR=0;SAO=0;VP=0x050000000005150026000100;WGT=1;VC=SNV;ASP;VLD;G5;KGPhase3;CAF=0.01937,0.9806;COMMON=1";

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpEntry1 = dbsnpReader.ExtractItem(vcfLine1)[0];
            var dbSnpEntry2 = dbsnpReader.ExtractItem(vcfLine2)[0];

            var sa = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(193187632));
            var additionalEntry = dbSnpEntry1.SetSupplementaryAnnotations(sa);

            additionalEntry.SetSupplementaryAnnotations(sa);


            var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(193187632));
            dbSnpEntry2.SetSupplementaryAnnotations(sa2);

            sa.MergeSaCreator(sa2);
            sa.FinalizePositionalAnnotations();

            Assert.Equal("T", sa.SaPosition.GlobalMajorAllele);
            Assert.Equal("0.9806", sa.SaPosition.GlobalMajorAlleleFrequency);
            Assert.Equal("G", sa.SaPosition.GlobalMinorAllele);
            Assert.Equal("0.01937", sa.SaPosition.GlobalMinorAlleleFrequency);

        }

        [Fact]
        public void MergeDbSnp1Kg()
        {
            //NIR-1262
            const string vcfLine =
                "1	825069	rs4475692	G	A,C	.	.	RS=4475692;RSPOS=825069;dbSNPBuildID=111;SSR=0;SAO=0;VP=0x050100000005170126000100;WGT=1;VC=SNV;SLO;ASP;VLD;G5A;G5;GNO;KGPhase3;CAF=0.3227,.,0.6773;COMMON=1";
            const string vcfLine1Kg =
                "1	825069	rs4475692	G	C	100	PASS	AC=3392;AF=0.677316;AN=5008;NS=2504;DP=22495;EAS_AF=0.754;AMR_AF=0.5692;AFR_AF=0.6127;EUR_AF=0.7286;SAS_AF=0.7096;AA=g|||;VT=SNP;EAS_AN=1008;EAS_AC=760;EUR_AN=1006;EUR_AC=733;AFR_AN=1322;AFR_AC=810;AMR_AN=694;AMR_AC=395;SAS_AN=978;SAS_AC=694\tGT";

            var dbsnpReader = new DbSnpReader(_renamer);
            var sa = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(825069));
            foreach (var dbSnpEntry in dbsnpReader.ExtractItem(vcfLine))
            {
                dbSnpEntry.SetSupplementaryAnnotations(sa);
            }

            var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(825069));
            var onekgReader = new OneKGenReader(_renamer);
            foreach (var onekgItem in onekgReader.ExtractItems(vcfLine1Kg))
            {
                onekgItem.SetSupplementaryAnnotations(sa1);
            }

            sa.MergeSaCreator(sa1);
            sa.FinalizePositionalAnnotations();

            Assert.Equal("C", sa.SaPosition.GlobalMajorAllele);
            Assert.Equal("G", sa.SaPosition.GlobalMinorAllele);

        }
        [Fact]
        public void DuplicateDbsnp()
        {
            // NIR-853: can't reproduce the problem at dbSnp parsing and merging.
            const string vcfLine1 =
                "1	8121167	rs34500567	C	CAAT,CAATAATAAAATAATAATAATAAT,CAATAAT,CAAT	.	.	RS=34500567;RSPOS=8121167;dbSNPBuildID=126;SSR=0;SAO=0;VP=0x050000000005000002000200;WGT=1;VC=DIV;ASP;CAF=0.9726,.,.,.,.,0.9726;COMMON=1";
            const string vcfLine2 =
                "1	8121167	rs566669620	C	CAATAATAAAAT	.	.	RS=566669620;RSPOS=8121175;dbSNPBuildID=142;SSR=0;SAO=0;VP=0x050000000005040024000200;WGT=1;VC=DIV;ASP;VLD;KGPhase3;CAF=0.9726,0.0007987;COMMON=1";
            const string vcfLine3 =
                "1	8121167	rs59792241	C	CAAT,CAATAATAAAATAATAATAATAAT,CAATAAT,CAAT	.	.	RS=59792241;RSPOS=8121205;dbSNPBuildID=137;SSR=0;SAO=0;VP=0x050000000005000002000200;WGT=1;VC=DIV;ASP;CAF=0.9726,.,.,.,.,0.9726;COMMON=1";

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItems1 = dbsnpReader.ExtractItem(vcfLine1);
            var dbSnpItems2 = dbsnpReader.ExtractItem(vcfLine2);
            var dbSnpItems3 = dbsnpReader.ExtractItem(vcfLine3);


            var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(8121168));
            var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(8121168));
            var sa3 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(8121168));

            var additionalItems = dbSnpItems1.Select(dbSnpItem => dbSnpItem.SetSupplementaryAnnotations(sa1)).ToList();

            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa1);
            }

            additionalItems.Clear();
            additionalItems.AddRange(dbSnpItems2.Select(dbSnpItem => dbSnpItem.SetSupplementaryAnnotations(sa2)));

            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa2);
            }

            additionalItems.Clear();
            foreach (var dbSnpItem in dbSnpItems3)
            {
                additionalItems.Add(dbSnpItem.SetSupplementaryAnnotations(sa3));
            }

            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa3);
            }
            sa1.MergeSaCreator(sa2);
            sa1.MergeSaCreator(sa3);

            var dbSnp =
                sa1.SaPosition.AlleleSpecificAnnotations["iAAT"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as
                    DbSnpAnnotation;
            Assert.NotNull(dbSnp);
            Assert.Equal(2, dbSnp.DbSnp.Count);
            Assert.Equal(34500567, dbSnp.DbSnp[0]);
            Assert.Equal(59792241, dbSnp.DbSnp[1]);
        }


        [Fact]
        public void MergeConflictingOneKitems()
        {
            const string vcfLine1 =
                "1	11408760	rs112877363	CTATG	C	100	PASS	AC=6;AF=0.00119808;AN=5008;NS=2504;DP=23213;EAS_AF=0;AMR_AF=0;AFR_AF=0.0045;EUR_AF=0;SAS_AF=0";
            const string vcfLine2 =
                "1	11408760	rs59160279	CTATG	CTATGTATG,C	100	PASS	AC=174,763;AF=0.0347444,0.152356;AN=5008;NS=2504;DP=23213;EAS_AF=0.0069,0.0615;AMR_AF=0.0259,0.062;AFR_AF=0.0749,0.4213;EUR_AF=0.0378,0.0239;SAS_AF=0.0123,0.0787";

            var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(11408761));
            var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(11408761));
            var oneKitem1 = _oneKGenReader.ExtractItems(vcfLine1)[0];

            var additionalItems = new List<SupplementaryDataItem>
            {
                oneKitem1.SetSupplementaryAnnotations(sa1)
            };
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa1);
            }

            additionalItems.Clear();
            additionalItems.AddRange(_oneKGenReader.ExtractItems(vcfLine2).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa2)));

            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa2);
            }


            sa1.MergeSaCreator(sa2);

            var oneKg =
                sa1.SaPosition.AlleleSpecificAnnotations["4"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] as
                    OneKGenAnnotation;
            Assert.NotNull(oneKg);
            // For conflicting entries, we clear all fields
            Assert.True(oneKg.HasConflicts);

        }

        [Fact]
        public void MergeConflictingOneKitemsSnv()
        {
            const string vcfLine1 =
                "X	129354240	rs1160681	C	A	100	PASS	AC=1996;AF=0.528742;AN=3775;NS=2504;DP=10421;AMR_AF=0.353;AFR_AF=0.5953;EUR_AF=0.3052;SAS_AF=0.3896;EAS_AF=0.2738;AA=C|||;VT=SNP";

            const string vcfLine2 =
                "X	129354240	.	C	A,G	100	PASS	AC=1981,15;AF=0.524768,0.00397351;AN=3775;NS=2504;DP=10421;AMR_AF=0.353,0;AFR_AF=0.584,0.0113;EUR_AF=0.3052,0;SAS_AF=0.3896,0;EAS_AF=0.2738,0;AA=C|||;VT=SNP;MULTI_ALLELIC";

            var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(129354240));
            var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(129354240));
            var oneKitem1 = _oneKGenReader.ExtractItems(vcfLine1)[0];

            oneKitem1.SetSupplementaryAnnotations(sa1);

            foreach (var oneKitem in _oneKGenReader.ExtractItems(vcfLine2))
            {
                oneKitem.SetSupplementaryAnnotations(sa2);
            }

            sa1.MergeSaCreator(sa2);

            var oneKg =
                sa1.SaPosition.AlleleSpecificAnnotations["A"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] as
                    OneKGenAnnotation;
            Assert.NotNull(oneKg);
            Assert.True(oneKg.HasConflicts);

        }

        [Fact]
        public void MergeConflictingExacItems()
        {
            const string vcfLine1 = "1	13528	.	C	G,T	1771.54	VQSRTrancheSNP99.60to99.80	AC=21,11;AC_AFR=12,0;AC_AMR=1,0;AC_Adj=13,9;AC_EAS=0,0;AC_FIN=0,0;AC_Het=13,9,0;AC_Hom=0,0;AC_NFE=0,2;AC_OTH=0,0;AC_SAS=0,7;AF=6.036e-04,3.162e-04;AN=34792;AN_AFR=390;AN_AMR=116;AN_Adj=10426;AN_EAS=150;AN_FIN=8;AN_NFE=2614;AN_OTH=116;AN_SAS=7032;BaseQRankSum=1.23;ClippingRankSum=0.056;DP=144988;FS=0.000;GQ_MEAN=14.54;GQ_STDDEV=16.53;Het_AFR=12,0,0;Het_AMR=1,0,0;Het_EAS=0,0,0;Het_FIN=0,0,0;Het_NFE=0,2,0;Het_OTH=0,0,0;Het_SAS=0,7,0;Hom_AFR=0,0;Hom_AMR=0,0;Hom_EAS=0,0;Hom_FIN=0,0;Hom_NFE=0,0;Hom_OTH=0,0;Hom_SAS=0,0;InbreedingCoeff=0.0557;MQ=31.08;MQ0=0;MQRankSum=-5.410e-01;NCC=67387;QD=1.91;ReadPosRankSum=0.206;VQSLOD=-2.705e+00;culprit=MQ;DP_HIST=10573|1503|705|1265|2477|613|167|52|18|11|8|3|0|0|1|0|0|0|0|0,2|6|2|1|4|0|3|1|0|0|2|0|0|0|0|0|0|0|0|0,1|0|0|0|1|1|3|0|1|1|1|0|0|0|1|0|0|0|0|0;GQ_HIST=342|11195|83|56|3154|517|367|60|12|4|5|7|1373|180|15|16|1|0|1|8,0|0|1|0|1|0|3|1|0|1|2|0|1|2|0|1|1|0|1|6,0|1|0|0|1|1|0|0|1|0|0|1|1|1|1|0|0|0|0|2";

            const string vcfLine2 =
                "1	13528	.	C	T	334.33	VQSRTrancheSNP99.60to99.80	AC=2;AC_AFR=0;AC_AMR=0;AC_Adj=2;AC_EAS=0;AC_FIN=0;AC_Het=2;AC_Hom=0;AC_NFE=0;AC_OTH=0;AC_SAS=2;AF=5.957e-05;AN=33576;AN_AFR=392;AN_AMR=114;AN_Adj=10200;AN_EAS=146;AN_FIN=6;AN_NFE=2556;AN_OTH=110;AN_SAS=6876;BaseQRankSum=-1.988e+00;ClippingRankSum=0.525;DP=142450;FS=2.634;GQ_MEAN=14.30;GQ_STDDEV=15.90;Het_AFR=0;Het_AMR=0;Het_EAS=0;Het_FIN=0;Het_NFE=0;Het_OTH=0;Het_SAS=2;Hom_AFR=0;Hom_AMR=0;Hom_EAS=0;Hom_FIN=0;Hom_NFE=0;Hom_OTH=0;Hom_SAS=0;InbreedingCoeff=-0.0753;MQ=31.78;MQ0=0;MQRankSum=0.578;NCC=68350;QD=5.31;ReadPosRankSum=-5.730e-01;VQSLOD=-3.582e+00;culprit=MQ;DP_HIST=10108|1417|742|1238|2324|682|184|56|20|11|4|2|0|0|0|0|0|0|0|0,0|0|0|1|0|0|0|0|0|1|0|0|0|0|0|0|0|0|0|0;GQ_HIST=335|10726|91|50|3215|542|410|67|10|3|1|6|1138|163|14|15|0|0|0|2,0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|2;DOUBLETON_DIST=0.028857408061;AC_MALE=1;AC_FEMALE=1;AN_MALE=7294;AN_FEMALE=2906;AC_CONSANGUINEOUS=0;AN_CONSANGUINEOUS=1360;Hom_CONSANGUINEOUS=0;";

            var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(13528));
            var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(13528));

            var exacReader = new ExacReader(_renamer);
            var exacItems = exacReader.ExtractItems(vcfLine1);

            foreach (var item in exacItems)
            {
                item.SetSupplementaryAnnotations(sa1);
            }

            exacItems.Clear();
            exacItems.AddRange(exacReader.ExtractItems(vcfLine2));

            foreach (var item in exacItems)
            {
                item?.SetSupplementaryAnnotations(sa2);
            }

            sa1.MergeSaCreator(sa2);

            var exac =
                sa1.SaPosition.AlleleSpecificAnnotations["T"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Exac)] as
                    ExacAnnotation;
            Assert.NotNull(exac);
            Assert.True(exac.HasConflicts);
        }

        [Fact]
        public void MergeConflictingEvsItems()
        {
            const string vcfLine1 = "1	1564952	rs112177324	T	G,A	.	PASS	BSNP=dbSNP_132;EA_AC=2,3039,4701;AA_AC=44,279,3231;TAC=46,3318,7932;MAF=39.2793,9.0884,29.7805;GTS=A1A1,A1A2,A1R,A2A2,A2R,RR;EA_GTC=0,1,1,707,1624,1538;AA_GTC=4,4,32,41,193,1503;GTC=4,5,33,748,1817,3041;DP=10;GL=MIB2;CP=0.8;CG=-0.0;AA=.;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_080875.2:intron,NM_080875.2:intron,NM_001170689.1:intron,NM_001170689.1:intron,NM_001170688.1:intron,NM_001170688.1:intron,NM_001170687.1:intron,NM_001170687.1:intron,NM_001170686.1:intron,NM_001170686.1:intron;HGVS_CDNA_VAR=NM_080875.2:c.2908+7del1,NM_080875.2:c.2908+6_2908+7insG,NM_001170689.1:c.2187-66del1,NM_001170689.1:c.2187-67_2187-66insG,NM_001170688.1:c.2713+7del1,NM_001170688.1:c.2713+6_2713+7insG,NM_001170687.1:c.2866+7del1,NM_001170687.1:c.2866+6_2866+7insG,NM_001170686.1:c.2896+7del1,NM_001170686.1:c.2896+6_28967insG;HGVS_PROTEIN_VAR=.,.,.,.,.,.,.,.,.,.;CDS_SIZES=NM_080875.2:3213,NM_080875.2:3213,NM_001170689.1:2262,NM_001170689.1:2262,NM_001170688.1:3018,NM_001170688.1:3018,NM_001170687.1:3171,NM_001170687.1:3171,NM_001170686.1:3201,NM_001170686.1:3201;GS=.,.,.,.,.,.,.,.,.,.;PH=.,.,.,.,.,.,.,.,.,.;EA_AGE=.;AA_AGE=.";

            const string vcfLine2 = "1	1564952	rs140739101	T	A	.	PASS	BSNP=dbSNP_134;EA_AC=313,6535;AA_AC=14,3808;TAC=327,10343;MAF=4.5707,0.3663,3.0647;GTS=GG,GT,TT;EA_GTC=92,129,3203;AA_GTC=1,12,1898;GTC=93,141,5101;DP=110;GL=OR4F5;CP=1.0;CG=0.9;AA=T;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_001005484.1:missense;HGVS_CDNA_VAR=NM_001005484.1:c.338T>G;HGVS_PROTEIN_VAR=NM_001005484.1:p.(F113C);CDS_SIZES=NM_001005484.1:918;GS=205;PH=probably-damaging:0.999;EA_AGE=.;AA_AGE=.";

            var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(1564952));
            var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(1564952));

            var evsReader = new EvsReader(_renamer);
            var evsItems = evsReader.ExtractItems(vcfLine1);

            foreach (var item in evsItems)
            {
                item.SetSupplementaryAnnotations(sa1);
            }

            evsItems.Clear();
            evsItems.AddRange(evsReader.ExtractItems(vcfLine2));

            foreach (var item in evsItems)
            {
                item?.SetSupplementaryAnnotations(sa2);
            }

            sa1.MergeSaCreator(sa2);

            var evs =
                sa1.SaPosition.AlleleSpecificAnnotations["A"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)] as
                    EvsAnnotation;
            Assert.NotNull(evs);
            Assert.True(evs.HasConflicts);
        }

        [Fact]
        public void MergeConflictingOneKitems1()
        {
            const string vcfLine1 =
                "1	20505705	rs35377696	C	CTCTG,CTG,CTGTG	100	PASS	AC=46,1513,152;AF=0.0091853,0.302117,0.0303514;AN=5008;NS=2504;DP=23578;EAS_AF=0,0.2718,0.0268;AMR_AF=0.0086,0.2939,0.0072;AFR_AF=0.0303,0.2693,0.0756;EUR_AF=0,0.3032,0.001;SAS_AF=0,0.3824,0.0194";
            const string vcfLine2 =
                "1	20505705	.	C	CTG	100	PASS	AC=4;AF=0.000798722;AN=5008;NS=2504;DP=23578;EAS_AF=0.002;AMR_AF=0;AFR_AF=0.0008;EUR_AF=0.001;SAS_AF=0";

            //var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(20505706);
            //var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(20505706);
            var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(20505706));
            var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(20505706));

            var additionalItems = _oneKGenReader.ExtractItems(vcfLine1).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa1)).ToList();

            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa1);
            }

            additionalItems.Clear();
            additionalItems.AddRange(_oneKGenReader.ExtractItems(vcfLine2).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa2)));

            foreach (var item in additionalItems)
            {
                item?.SetSupplementaryAnnotations(sa2);
            }

            sa1.MergeSaCreator(sa2);

            Assert.True(sa1.SaPosition.AlleleSpecificAnnotations["iTG"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)].HasConflicts);
            // Assert.Null(sa1.AlleleSpecificAnnotations["iTG"].OneKgAll);

        }


        [Fact]
        public void DiscardConflictingOneKitems()
        {
            // NIR-1147
            const string vcfLine1 =
                "22	17996285	rs35048606	A	ATCTC	100	PASS	AC=12;AF=0.00239617;AN=5008;NS=2504;DP=19702;EAS_AF=0.0119;AMR_AF=0;AFR_AF=0;EUR_AF=0;SAS_AF=0;VT=INDEL";

            const string vcfLine2 =
                "22	17996285	rs35048606;rs5746424	A	ATCTC,C	100	PASS	AC=3444,1141;AF=0.6877,0.227835;AN=5008;NS=2504;DP=19702;EAS_AF=0.497,0.4544;AMR_AF=0.6354,0.2205;AFR_AF=0.798,0.1815;EUR_AF=0.7068,0.1233;SAS_AF=0.7526,0.1697;VT=SNP,INDEL;MULTI_ALLELIC";

            var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(17996286));
            var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(17996286));
            var oneKitem1 = _oneKGenReader.ExtractItems(vcfLine1)[0];

            var additionalItems = new List<SupplementaryDataItem>
            {
                oneKitem1.SetSupplementaryAnnotations(sa1)
            };
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa1);
            }

            additionalItems.Clear();
            additionalItems.AddRange(_oneKGenReader.ExtractItems(vcfLine2).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa2)));

            foreach (var item in additionalItems)
            {
                item?.SetSupplementaryAnnotations(sa2);
            }

            sa1.MergeSaCreator(sa2);

            var oneKg =
                sa1.SaPosition.AlleleSpecificAnnotations["iTCTC"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)];
            Assert.True(oneKg.HasConflicts);
        }

        [Fact]
        public void MergeDbSnpCosmic()
        {
            const string vcfLine1 = "1	10228	rs143255646	TA	T	.	.	RS=143255646;RSPOS=10229;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000020005000002000200;WGT=1;VC=DIV;R5;ASP";


            var sa = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(10229));
            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItem1 = dbsnpReader.ExtractItem(vcfLine1)[0];
            var additionalItems = new List<SupplementaryDataItem>
            {
                dbSnpItem1.SetSupplementaryAnnotations(sa)
            };

            var cosmicItem1 = new CosmicItem("1", 10229, "COSM1000", "TA", "T", "TP53",
                new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy(null, "carcinoma", "oesophagus") }, null);
            var cosmicItem2 = new CosmicItem("1", 10229, "COSM1000", "TA", "T", "TP53",
                new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy("01", "carcinoma", "large_intestine") }, null);

            additionalItems.Add(cosmicItem1.SetSupplementaryAnnotations(sa));
            additionalItems.Add(cosmicItem2.SetSupplementaryAnnotations(sa));

            //sa.Clear();
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa);
            }

            var dbSnpAnnotation =
                sa.SaPosition.AlleleSpecificAnnotations["1"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as
                    DbSnpAnnotation;
            Assert.NotNull(dbSnpAnnotation);
            Assert.Equal(dbSnpAnnotation.DbSnp, new List<long> { 143255646 });
            Assert.True(sa.SaPosition.ContainsCosmicId("COSM1000"));
        }

        [Fact]
        public void MergeDbSnpCosmic1Kg()
        {
            const string vcfLine1 = "1	10228	rs143255646	TA	T	.	.	RS=143255646;RSPOS=10229;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000020005000002000200;WGT=1;VC=DIV;R5;ASP";
            const string vcfLine2 = "1	10228	.	TA	T	100	PASS	AC=2130;AF=0.425319;AN=5008;NS=2504;DP=103152;EAS_AF=0.3363;AMR_AF=0.3602;AFR_AF=0.4909;EUR_AF=0.4056;SAS_AF=0.4949;AA=|||unknown(NO_COVERAGE)";

            var sa = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(10229));
            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItem1 = dbsnpReader.ExtractItem(vcfLine1)[0];
            var additionalItems = new List<SupplementaryDataItem>
            {
                dbSnpItem1.SetSupplementaryAnnotations(sa)
            };


            var cosmicItem1 = new CosmicItem("1", 10229, "COSM1000", "TA", "T", "TP53",
                new HashSet<CosmicItem.CosmicStudy> { new CosmicItem.CosmicStudy(null, "carcinoma", "oesophagus") }, null);
            additionalItems.Add(cosmicItem1.SetSupplementaryAnnotations(sa));

            var oneKGenItem = _oneKGenReader.ExtractItems(vcfLine2)[0];
            additionalItems.Add(oneKGenItem.SetSupplementaryAnnotations(sa));

            //sa.Clear();
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa);
            }

            var asa = sa.SaPosition.AlleleSpecificAnnotations["1"];
            var dbSnp = asa.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as DbSnpAnnotation;
            var oneKg = asa.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] as OneKGenAnnotation;

            Assert.NotNull(oneKg);

            var oneKgAc = oneKg.OneKgAllAc;
            var oneKgAn = oneKg.OneKgAllAn;

            Assert.NotNull(oneKgAc);
            Assert.NotNull(oneKgAn);

            Assert.NotNull(dbSnp);
            Assert.NotNull(oneKg);
            Assert.Equal(dbSnp.DbSnp, new List<long> { 143255646 });
            Assert.Equal("0.425319", (oneKgAc.Value / (double)oneKgAn.Value).ToString(JsonCommon.FrequencyRoundingFormat));

            Assert.True(sa.SaPosition.ContainsCosmicId("COSM1000"));
        }

        [Fact]
        public void MergeDbSnp1KpEvs()
        {
            const string vcfLine1 = "1	69428	rs140739101	T	G	.	.	RS=140739101;RSPOS=69428;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050200000a05140026000100;WGT=1;VC=SNV;S3D;NSM;REF;ASP;VLD;KGPhase3;CAF=0.981,0.01897;COMMON=1";
            const string vcfLine2 = "1	69428	rs140739101	T	G	100	PASS	AC=95;AF=0.0189696;AN=5008;NS=2504;DP=17611;EAS_AF=0.003;AMR_AF=0.036;AFR_AF=0.0015;EUR_AF=0.0497;SAS_AF=0.0153;AA=.|||;VT=SNP;EX_TARGET;EAS_AN=1008;EAS_AC=3;EUR_AN=1006;EUR_AC=50;AFR_AN=1322;AFR_AC=2;AMR_AN=694;AMR_AC=25;SAS_AN=978;SAS_AC=15";
            const string vcfLine3 = "1	69428	rs140739101	T	G	.	PASS	BSNP=dbSNP_134;EA_AC=313,6535;AA_AC=14,3808;TAC=327,10343;MAF=4.5707,0.3663,3.0647;GTS=GG,GT,TT;EA_GTC=92,129,3203;AA_GTC=1,12,1898;GTC=93,141,5101;DP=110;GL=OR4F5;CP=1.0;CG=0.9;AA=T;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_001005484.1:missense;HGVS_CDNA_VAR=NM_001005484.1:c.338T>G;HGVS_PROTEIN_VAR=NM_001005484.1:p.(F113C);CDS_SIZES=NM_001005484.1:918;GS=205;PH=probably-damaging:0.999;EA_AGE=.;AA_AGE=.";

            var sa = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(69428));

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItem = dbsnpReader.ExtractItem(vcfLine1)[0];
            dbSnpItem.SetSupplementaryAnnotations(sa);

            var oneKGenItem = _oneKGenReader.ExtractItems(vcfLine2)[0];
            oneKGenItem.SetSupplementaryAnnotations(sa);

            var evsReader = new EvsReader(_renamer);
            var evsItem = evsReader.ExtractItems(vcfLine3)[0];
            evsItem.SetSupplementaryAnnotations(sa);


            var asa = sa.SaPosition.AlleleSpecificAnnotations["G"];
            var dbSnp = asa.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as DbSnpAnnotation;
            var oneKg = asa.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] as OneKGenAnnotation;
            var evs = asa.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)] as EvsAnnotation;

            Assert.NotNull(dbSnp);
            Assert.NotNull(oneKg);
            Assert.NotNull(evs);

            var oneKgAc = oneKg.OneKgEurAc;
            var oneKgAn = oneKg.OneKgEurAn;

            Assert.NotNull(oneKgAc);
            Assert.NotNull(oneKgAn);

            Assert.Equal(new List<long> { 140739101 }, dbSnp.DbSnp);
            Assert.Equal("0.049702", (oneKgAc.Value / (double)oneKgAn.Value).ToString(JsonCommon.FrequencyRoundingFormat));
            Assert.Equal("0.045707", evs.EvsEur);
            Assert.False(sa.SaPosition.IsRefMinorAllele);
        }

        [Fact]
        public void MergeDbSnp1KpEvsRefMinor()
        {
            const string vcfLine1 = "1	69428	rs140739101	T	G	.	.	RS=140739101;RSPOS=69428;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050200000a05140026000100;WGT=1;VC=SNV;S3D;NSM;REF;ASP;VLD;KGPhase3;CAF=0.981,0.01897;COMMON=1";
            //vcf line is modified
            const string vcfLine2 = "1	69428	rs140739101	T	G	100	PASS	AC=4956;AF=0.989617;AN=5008;NS=2504;DP=17611;EAS_AF=0.003;AMR_AF=0.036;AFR_AF=0.0015;EUR_AF=0.0497;SAS_AF=0.0153;AA=.|||;VT=SNP;EX_TARGET;EAS_AN=1008;EAS_AC=3;EUR_AN=1006;EUR_AC=50;AFR_AN=1322;AFR_AC=2;AMR_AN=694;AMR_AC=25;SAS_AN=978;SAS_AC=15";
            const string vcfLine3 = "1	69428	rs140739101	T	G	.	PASS	BSNP=dbSNP_134;EA_AC=313,6535;AA_AC=14,3808;TAC=327,10343;MAF=4.5707,0.3663,3.0647;GTS=GG,GT,TT;EA_GTC=92,129,3203;AA_GTC=1,12,1898;GTC=93,141,5101;DP=110;GL=OR4F5;CP=1.0;CG=0.9;AA=T;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_001005484.1:missense;HGVS_CDNA_VAR=NM_001005484.1:c.338T>G;HGVS_PROTEIN_VAR=NM_001005484.1:p.(F113C);CDS_SIZES=NM_001005484.1:918;GS=205;PH=probably-damaging:0.999;EA_AGE=.;AA_AGE=.";

            var sa = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(69428));

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItem = dbsnpReader.ExtractItem(vcfLine1)[0];
            dbSnpItem.SetSupplementaryAnnotations(sa);

            var oneKGenItem = _oneKGenReader.ExtractItems(vcfLine2)[0];
            oneKGenItem.SetSupplementaryAnnotations(sa);

            var evsReader = new EvsReader(_renamer);
            var evsItem = evsReader.ExtractItems(vcfLine3)[0];
            evsItem.SetSupplementaryAnnotations(sa);
            sa.FinalizePositionalAnnotations();

            var dbSnp = sa.SaPosition.AlleleSpecificAnnotations["G"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as DbSnpAnnotation;

            Assert.NotNull(dbSnp);

            Assert.Equal(new List<long> { 140739101 }, dbSnp.DbSnp);
            Assert.Equal(true, sa.SaPosition.IsRefMinorAllele);
        }

        [Fact]
        public void Merge1KgEvsExac()
        {
            const string vcfLine1 =
                "1	13382	rs191719684	C	G	.	PASS	DBSNP=dbSNP_135;EA_AC=0,8600;AA_AC=17,4389;TAC=17,12989;MAF=0.0,0.3858,0.1307;GTS=GG,GC,CC;EA_GTC=0,0,4300;AA_GTC=0,17,2186;GTC=0,17,6486;DP=54;GL=SAMD11;CP=0.0;CG=1.5;AA=C;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_152486.2:intron;HGVS_CDNA_VAR=NM_152486.2:c.-30C>G;HGVS_PROTEIN_VAR=.;CDS_SIZES=NM_152486.2:2046;GS=.;PH=.;EA_AGE=.;AA_AGE=24.3+/-50.5";
            const string vcfLine2 =
                "1	13382	.	C	G	320.40	VQSRTrancheSNP99.60to99.80	AC=3;AC_AFR=0;AC_AMR=0;AC_Adj=1;AC_EAS=0;AC_FIN=0;AC_Het=1;AC_Hom=0;AC_NFE=0;AC_OTH=0;AC_SAS=1;AF=1.079e-04;AN=27810;AN_AFR=460;AN_AMR=82;AN_Adj=5728;AN_EAS=148;AN_FIN=4;AN_NFE=1400;AN_OTH=60;AN_SAS=3574;BaseQRankSum=-8.880e-01;ClippingRankSum=0.493;DP=86138;FS=0.000;GQ_MEAN=11.35;GQ_STDDEV=12.58;Het_AFR=0;Het_AMR=0;Het_EAS=0;Het_FIN=0;Het_NFE=0;Het_OTH=0;Het_SAS=1;Hom_AFR=0;Hom_AMR=0;Hom_EAS=0;Hom_FIN=0;Hom_NFE=0;Hom_OTH=0;Hom_SAS=0;InbreedingCoeff=-0.0832;MQ=34.49;MQ0=0;MQRankSum=-6.910e-01;NCC=72140;QD=20.03;ReadPosRankSum=-2.073e+00;VQSLOD=-4.106e+00;culprit=MQ;DP_HIST=9135|1821|1658|665|130|135|199|110|41|8|2|1|0|0|0|0|0|0|0|0,1|0|1|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0|0;GQ_HIST=1432|8682|140|118|2625|254|121|17|3|1|10|24|364|94|9|11|0|0|0|0,0|1|0|0|0|0|0|0|1|0|0|0|0|0|0|0|0|0|0|0;DOUBLETON_DIST=.;AC_MALE=1;AC_FEMALE=0;AN_MALE=3866;AN_FEMALE=1862;AC_CONSANGUINEOUS=0;AN_CONSANGUINEOUS=684;Hom_CONSANGUINEOUS=0";
            const string vcfLine3 =
                "1	13382	rs538606945	C	G	100	PASS	AC=1;AF=0.000199681;AN=5008;NS=2504;DP=28817;EAS_AF=0;AMR_AF=0;AFR_AF=0;EUR_AF=0;SAS_AF=0.001;AA=c|||;VT=SNP";

            var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(13382));
            var evsReader = new EvsReader(_renamer);
            var evsItem = evsReader.ExtractItems(vcfLine1)[0];
            evsItem.SetSupplementaryAnnotations(sa1);

            var sa2 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(13382));
            var exacReader = new ExacReader(_renamer);
            var exacItem = exacReader.ExtractItems(vcfLine2)[0];
            exacItem.SetSupplementaryAnnotations(sa2);

            var sa3 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(13382));
            var onekReader = new OneKGenReader(_renamer);
            var onekItem = onekReader.ExtractItems(vcfLine3)[0];
            onekItem.SetSupplementaryAnnotations(sa3);

            sa1.MergeSaCreator(sa2);
            sa1.MergeSaCreator(sa3);

            var asa = sa1.SaPosition.AlleleSpecificAnnotations["G"];
            var exac = asa.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Exac)] as ExacAnnotation;
            var oneKg = asa.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] as OneKGenAnnotation;
            var evs = asa.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)] as EvsAnnotation;

            Assert.NotNull(exac);
            Assert.NotNull(oneKg);
            Assert.NotNull(evs);

            var oneKgAc = oneKg.OneKgAllAc;
            var oneKgAn = oneKg.OneKgAllAn;
            var exacAllAc = exac.ExacAllAc;
            var exacAllAn = exac.ExacAllAn;

            Assert.NotNull(oneKgAc);
            Assert.NotNull(oneKgAn);
            Assert.NotNull(exacAllAc);
            Assert.NotNull(exacAllAn);

            Assert.Equal("0.0002", (oneKgAc.Value / (double)oneKgAn.Value).ToString(JsonCommon.FrequencyRoundingFormat));
            Assert.Equal("0.001307", evs.EvsAll);
            Assert.Equal("0.000175", (exacAllAc.Value / (double)exacAllAn.Value).ToString(JsonCommon.FrequencyRoundingFormat));
        }

        [Fact]
        public void MultiAlleleMergeDbSnp1KpEvs()
        {
            const string vcfLine1 = "1	1564952	rs112177324	TG	T	.	.	RS=112177324;RSPOS=1564953;dbSNPBuildID=132;SSR=0;SAO=0;VP=0x05010008000514013e000200;WGT=1;VC=DIV;SLO;INT;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=0.8468,0.1506;COMMON=1";
            const string vcfLine2 = "1	1564952	rs112177324	TG	TGG,T	100	PASS	AC=13,754;AF=0.00259585,0.150559;AN=5008;NS=2504;DP=8657;EAS_AF=0,0.0933;AMR_AF=0.0014,0.2046;AFR_AF=0.0091,0.0182;EUR_AF=0,0.3588;SAS_AF=0,0.136";
            const string vcfLine3 = "1	1564952	rs112177324	TG	TGG,T	.	PASS	BSNP=dbSNP_132;EA_AC=2,3039,4701;AA_AC=44,279,3231;TAC=46,3318,7932;MAF=39.2793,9.0884,29.7805;GTS=A1A1,A1A2,A1R,A2A2,A2R,RR;EA_GTC=0,1,1,707,1624,1538;AA_GTC=4,4,32,41,193,1503;GTC=4,5,33,748,1817,3041;DP=10;GL=MIB2;CP=0.8;CG=-0.0;AA=.;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_080875.2:intron,NM_080875.2:intron,NM_001170689.1:intron,NM_001170689.1:intron,NM_001170688.1:intron,NM_001170688.1:intron,NM_001170687.1:intron,NM_001170687.1:intron,NM_001170686.1:intron,NM_001170686.1:intron;HGVS_CDNA_VAR=NM_080875.2:c.2908+7del1,NM_080875.2:c.2908+6_2908+7insG,NM_001170689.1:c.2187-66del1,NM_001170689.1:c.2187-67_2187-66insG,NM_001170688.1:c.2713+7del1,NM_001170688.1:c.2713+6_2713+7insG,NM_001170687.1:c.2866+7del1,NM_001170687.1:c.2866+6_2866+7insG,NM_001170686.1:c.2896+7del1,NM_001170686.1:c.2896+6_28967insG;HGVS_PROTEIN_VAR=.,.,.,.,.,.,.,.,.,.;CDS_SIZES=NM_080875.2:3213,NM_080875.2:3213,NM_001170689.1:2262,NM_001170689.1:2262,NM_001170688.1:3018,NM_001170688.1:3018,NM_001170687.1:3171,NM_001170687.1:3171,NM_001170686.1:3201,NM_001170686.1:3201;GS=.,.,.,.,.,.,.,.,.,.;PH=.,.,.,.,.,.,.,.,.,.;EA_AGE=.;AA_AGE=.";

            var sa = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(1564952));

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItem = dbsnpReader.ExtractItem(vcfLine1)[0];
            var additionalItems = new List<SupplementaryDataItem>
            {
                dbSnpItem.SetSupplementaryAnnotations(sa)
            };

            foreach (var oneKitem in _oneKGenReader.ExtractItems(vcfLine2))
            {
                additionalItems.Add(oneKitem.SetSupplementaryAnnotations(sa));
            }

            var evsReader = new EvsReader(_renamer);
            var evsItemsList = evsReader.ExtractItems(vcfLine3);

            foreach (var evsItem in evsItemsList)
            {
                additionalItems.Add(evsItem.SetSupplementaryAnnotations(sa));
            }

            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa);
            }




            var asa1 = sa.SaPosition.AlleleSpecificAnnotations["1"];
            var dbSnp1 = asa1.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.DbSnp)] as DbSnpAnnotation;
            var oneKg1 = asa1.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] as OneKGenAnnotation;
            var evs1 = asa1.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)] as EvsAnnotation;

            Assert.NotNull(dbSnp1);
            Assert.NotNull(oneKg1);
            Assert.NotNull(evs1);

            var asaiG = sa.SaPosition.AlleleSpecificAnnotations["iG"];
            var oneKgiG = asaiG.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] as OneKGenAnnotation;
            var evsiG = asaiG.Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.Evs)] as EvsAnnotation;

            Assert.NotNull(oneKgiG);
            Assert.NotNull(evsiG);

            Assert.Equal(new List<long> { 112177324 }, dbSnp1.DbSnp);

            var oneKggAc = oneKgiG.OneKgAllAc;
            var oneKggAn = oneKgiG.OneKgAllAn;
            var oneKg1Ac = oneKg1.OneKgAllAc;
            var oneKg1An = oneKg1.OneKgAllAn;

            Assert.NotNull(oneKggAc);
            Assert.NotNull(oneKggAn);
            Assert.NotNull(oneKg1Ac);
            Assert.NotNull(oneKg1An);

            Assert.Equal("0.002596", (oneKggAc.Value / (double)oneKggAn.Value).ToString(JsonCommon.FrequencyRoundingFormat));
            Assert.Equal("0.150559", (oneKg1Ac.Value / (double)oneKg1An.Value).ToString(JsonCommon.FrequencyRoundingFormat));

            Assert.Equal("0.012380", evsiG.EvsAfr);
            Assert.Equal("0.000258", evsiG.EvsEur);
            Assert.Equal("0.004072", evsiG.EvsAll);

            Assert.Equal("0.078503", evs1.EvsAfr);
            Assert.Equal("0.392534", evs1.EvsEur);
            Assert.Equal("0.293732", evs1.EvsAll);
        }

        [Fact]
        public void MergeDbSnpClinVar()
        {
            const string vcfLine = "1	225592188	rs387906416	TAGAAGA	CTTCTAG	.	.	RS=387906416;RSPOS=225592188;RV;dbSNPBuildID=137;SSR=0;SAO=1;VP=0x050060000605000002110800;GENEINFO=LBR:3930;WGT=1;VC=MNV;PM;NSN;REF;ASP;LSD;OM";

            var dbsnpReader = new DbSnpReader(_renamer);
            var dbSnpItems = dbsnpReader.ExtractItem(vcfLine);

            var sa = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(225592188));

            foreach (var dbSnpItem in dbSnpItems)
            {
                dbSnpItem.SetSupplementaryAnnotations(sa);
            }

            var xmlReader = new ClinVarXmlReader(new FileInfo(Resources.TopPath("RCV000087262.xml")), _reader, _sequence);

            foreach (var clinVarItem in xmlReader)
            {
                var sa1 = new SupplementaryPositionCreator(new SupplementaryAnnotationPosition(225592188));
                clinVarItem.SetSupplementaryAnnotations(sa1);
                sa.MergeSaCreator(sa1);
            }

            Assert.Equal(1, sa.SaPosition.ClinVarItems.Count);

            foreach (var clinVarEntry in sa.SaPosition.ClinVarItems)
            {
                Assert.Equal(clinVarEntry.ID, "RCV000087262.3");
                Assert.Equal(clinVarEntry.MedGenIDs.First(), "C0030779");
                Assert.Equal(clinVarEntry.Phenotypes.First(), "Pelger-Huët anomaly");
            }
        }
    }
}
