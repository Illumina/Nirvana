using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.OneKGen;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.SAUtils.InputFileParsers
{
    public sealed class OneKGenTests
    {
        private const string VcfLine1 = "1	10352	rs555500075	T	TA	100	PAS	AC=2191;AF=0.4375;AN=5008;NS=2504;DP=88915;EAS_AF=0.4306;AMR_AF=0.4107;AFR_AF=0.4788;EUR_AF=0.4264;SAS_AF=0.4192;AA=|||unknown(NO_COVERAGE); VT=INDEL;EAS_AN=1008;EAS_AC=434;EUR_AN=1006;EUR_AC=429;AFR_AN=1322;AFR_AC=633;AMR_AN=694;AMR_AC=285;SAS_AN=978;SAS_AC=410";

        private const string VcfLine2 = "1	15274	rs62636497	A	G,T	100	PASS	AC=1739,3210;AF=0.347244,0.640974;AN=5008;NS=2504;DP=23255;EAS_AF=0.4812,0.5188;AMR_AF=0.2752,0.7205;AFR_AF=0.323,0.6369;EUR_AF=0.2922,0.7078;SAS_AF=0.3497,0.6472;AA=g|||;VT=SNP;MULTI_ALLELIC;EAS_AN=1008;EAS_AC=485,523;EUR_AN=1006;EUR_AC=294,712;AFR_AN=1322;AFR_AC=427,842;AMR_AN=694;AMR_AC=191,500;SAS_AN=978;SAS_AC=342,633";

        //private const string VcfLine3 = "1	10616	rs376342519	CCGCCGTTGCAAAGGCGCGCCG	C	100	PASS	AC=4973;AF=0.993011;AN=5008;NS=2504;DP=2365;EAS_AF=0.9911; AMR_AF=0.9957;AFR_AF=0.9894;EUR_AF=0.994;SAS_AF=0.9969;VT=INDEL;EAS_AN=1008;EAS_AC=999;EUR_AN=1006;EUR_AC=1000;AFR_AN=1322;AFR_AC = 1308; AMR_AN=694;AMR_AC=691;SAS_AN=978;SAS_AC=975";

        //// have been modified to make the first alt allele very freq
        //private const string VcfLine4 = "1	806324	.	G	GATA,T	100	PASS	AC=4912,2;AF=0.9808,0.000399361;AN=5008";

        //// frequency have been modified
        //private const string VcfLine5 = "1	985465	rs200384052;rs566813074	G	A,GT	100	PASS	AC=200,11;AF=0.0399361,0.00219649;AN=5008;NS=2504;DP=7139;EAS_AF=0,0;AMR_AF=0,0;AFR_AF=0,0;EUR_AF=0.002,0.002;SAS_AF=0,0.0092;VT=SNP,INDEL;MULTI_ALLELIC;EX_TARGET;EAS_AN=1008;EAS_AC=0,0;EUR_AN=1006;EUR_AC=2,2;AFR_AN=1322;AFR_AC=0,0;AMR_AN=694;AMR_AC=0,0;SAS_AN=978;SAS_AC=0,9";

        //// VcfLine6 is an artificial entry for testing only
        //private const string VcfLine6 = "1	985465	.	G	C	100	PASS	AC=4688;AF=0.9361;AN=5008;NS=2504;DP=7139;EAS_AF=0;AMR_AF=0;AFR_AF=0;EUR_AF=0.002;SAS_AF=0";

        private readonly OneKGenReader _oneKGenReader;
        private readonly IDictionary<string, IChromosome> _refChromDict;

        /// <summary>
        /// constructor
        /// </summary>
        public OneKGenTests()
        {
            _refChromDict = new Dictionary<string, IChromosome>
            {
                {"1",new Chromosome("chr1", "1",0) },
                {"4",new Chromosome("chr4", "4", 3) },
                {"X",new Chromosome("chrX", "X", 22) }
            };
            _oneKGenReader = new OneKGenReader(_refChromDict);
        }

        private static string GetAlleleFrequency(string jsonString, string description)
        {
            var regexMatch = Regex.Match(jsonString, $"\"{description}\":([0|1]\\.?\\d+)?");
            return regexMatch.Success ? regexMatch.Groups[1].ToString() : null;
        }

        [Fact(Skip = "new SA")]
        public void MultiEntryMixedVariant()
        {
            //var sa1 = new SupplementaryPositionCreator();

            //var oneKItem1 = _oneKGenReader.ExtractItems(VcfLine5)[0];
            //var oneKItem2 = _oneKGenReader.ExtractItems(VcfLine6)[0];

            //oneKItem1.SetSupplementaryAnnotations(sa1);

            //// additional items are ignored since they cannot be SNVs
            //var sa2 = new SupplementaryPositionCreator();
            //oneKItem2.SetSupplementaryAnnotations(sa2);

            //sa1.MergeSaCreator(sa2);
            //sa1.FinalizePositionalAnnotations();

            //Assert.True(sa1.IsRefMinor());
        }

        [Fact(Skip = "new SA")]
        public void MixedVariantType()
        {
            //var oneKItem = _oneKGenReader.ExtractItems(VcfLine4)[1];

            //var sa = new SupplementaryPositionCreator();

            //oneKItem.SetSupplementaryAnnotations(sa);

            //Assert.False(sa.IsRefMinor());
        }

        [Fact(Skip = "new SA")]
        public void AncestralAlleleN()
        {
            //const string vcfLine = "1	30923	rs806731	G	T	100	PASS	AC=4369;AF=0.872404;AN=5008;NS=2504;DP=13565;EAS_AF=0.996;AMR_AF=0.9164;AFR_AF=0.6687;EUR_AF=0.9364;SAS_AF=0.9233;AA=N|||;VT=SNP";

            //var oneKItem = _oneKGenReader.ExtractItems(vcfLine)[0];

            //var sa = new SupplementaryPositionCreator();

            //oneKItem.SetSupplementaryAnnotations(sa);

            //var oneKg =
            //    sa.SaPosition.AlleleSpecificAnnotations["T"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] as
            //        OneKGenAnnotation;
            //Assert.NotNull(oneKg);

            //Assert.Equal("N", oneKg.AncestralAllele);
        }

        [Fact(Skip = "new SA")]
        public void DiscardConflictingOneKitems()
        {
            //// NIR-1147
            //const string vcfLine1 = "22	17996285	rs35048606	A	ATCTC	100	PASS	AC=12;AF=0.00239617;AN=5008;NS=2504;DP=19702;EAS_AF=0.0119;AMR_AF=0;AFR_AF=0;EUR_AF=0;SAS_AF=0;VT=INDEL";
            //const string vcfLine2 = "22	17996285	rs35048606;rs5746424	A	ATCTC,C	100	PASS	AC=3444,1141;AF=0.6877,0.227835;AN=5008;NS=2504;DP=19702;EAS_AF=0.497,0.4544;AMR_AF=0.6354,0.2205;AFR_AF=0.798,0.1815;EUR_AF=0.7068,0.1233;SAS_AF=0.7526,0.1697;VT=SNP,INDEL;MULTI_ALLELIC";

            //var sa1 = new SupplementaryPositionCreator();
            //var sa2 = new SupplementaryPositionCreator();
            //var oneKitem1 = _oneKGenReader.ExtractItems(vcfLine1)[0];

            //var additionalItems = new List<SupplementaryDataItem>
            //{
            //    oneKitem1.SetSupplementaryAnnotations(sa1)
            //};

            //foreach (var item in additionalItems)
            //{
            //    item.SetSupplementaryAnnotations(sa1);
            //}

            //additionalItems.Clear();
            //additionalItems.AddRange(_oneKGenReader.ExtractItems(vcfLine2).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa2)));

            //foreach (var item in additionalItems)
            //{
            //    item?.SetSupplementaryAnnotations(sa2);
            //}

            //sa1.MergeSaCreator(sa2);

            //var oneKg =
            //    sa1.SaPosition.AlleleSpecificAnnotations["iTCTC"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] as
            //        OneKGenAnnotation;
            //Assert.NotNull(oneKg);

            //// in some cases, the merge happens using setSupplementaryAnnotation(). this unit test checks if that path is ok
            //Assert.True(oneKg.HasConflicts);
        }

        [Fact(Skip = "new SA")]
        public void NonConflictingOneKitems1()
        {
            //// NIR-1147
            //const string vcfLine1 = "X	5331877	rs71800267	AAC	AACAC,A	100	PASS	AC=159,562;AF=0.0421192,0.148874;AN=3775;NS=2504;OLD_VARIANT=X:5331899:CAC/CACAC/C;DP=9474;AMR_AF=0.0014,0.0908;AFR_AF=0.025,0.2769;EUR_AF=0.0109,0.0835;SAS_AF=0.0481,0.0307;EAS_AF=0.0665,0.0188;VT=INDEL;MULTI_ALLELIC";
            //const string vcfLine2 = "X	5331877	.	AACACACACAC	A	100	PASS	AC= 101;AF=0.026755;AN=3775;NS=2504;DP=9474;AMR_AF=0.0086;AFR_AF=0.0711;EUR_AF=0.001;SAS_AF=0;EAS_AF=0;VT=INDEL";

            //var sa1 = new SupplementaryPositionCreator();
            //var sa2 = new SupplementaryPositionCreator();
            //var oneKitem1 = _oneKGenReader.ExtractItems(vcfLine1)[1];

            //var additionalItems = new List<SupplementaryDataItem>
            //{
            //    oneKitem1.SetSupplementaryAnnotations(sa1)
            //};

            //foreach (var item in additionalItems)
            //{
            //    item.SetSupplementaryAnnotations(sa1);
            //}

            //additionalItems.Clear();
            //additionalItems.AddRange(_oneKGenReader.ExtractItems(vcfLine2).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa2)));

            //foreach (var item in additionalItems)
            //{
            //    item?.SetSupplementaryAnnotations(sa2);
            //}

            //sa1.MergeSaCreator(sa2);
            //// in some cases, the merge happens using setSupplementaryAnnotation(). this unit test checks if that path is ok

            //var oneKg2 =
            //    sa1.SaPosition.AlleleSpecificAnnotations["2"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] as
            //        OneKGenAnnotation;
            //Assert.NotNull(oneKg2);

            //var oneKg10 =
            //    sa1.SaPosition.AlleleSpecificAnnotations["10"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)] as
            //        OneKGenAnnotation;
            //Assert.NotNull(oneKg10);

            //Assert.False(oneKg2.HasConflicts);
            //Assert.NotNull(oneKg2.OneKgAllAc);
            //Assert.False(oneKg10.HasConflicts);
        }

        [Fact(Skip = "new SA")]
        public void MissingRefMinor()
        {
            //const string vcfLine =
            //    "1	15274	rs62636497	A	G,T	100	PASS	AC=1739,3210;AF=0.347244,0.640974;AN=5008;NS=2504;DP=23255;EAS_AF=0.4812,0.5188;AMR_AF=0.2752,0.7205;AFR_AF=0.323,0.6369;EUR_AF=0.2922,0.7078;SAS_AF=0.3497,0.6472;AA=g|||;VT=SNP;MULTI_ALLELIC";

            //var sa = new SupplementaryPositionCreator();

            //foreach (var oneKitems in _oneKGenReader.ExtractItems(vcfLine))
            //{
            //    oneKitems.SetSupplementaryAnnotations(sa);
            //}
            //sa.FinalizePositionalAnnotations();
            //Assert.True(sa.IsRefMinor());
        }

        [Fact(Skip = "new SA")]
        public void AlleleFrequencyArbitration()
        {
            //const string vcfLine1 = "4	170887158	rs34734657	GCAAAA	G	100	PASS	AC=2222;AF=0.44369;AN=5008;NS=2504;DP=30687;EAS_AF=0.0675;AMR_AF=0.5447;AFR_AF=0.6377;EUR_AF=0.5547;SAS_AF=0.3834;VT=INDEL";
            //const string vcfLine2 = "4	170887158	rs556076439;rs34734657	GCAAAA	GCAAAACAAAA,G	100	PASS	AC=69,650;AF=0.013778,0.129792;AN=5008;NS=2504;DP=30687;EAS_AF=0.0109,0.2331;AMR_AF=0.0331,0.0303;AFR_AF=0.0045,0.1165;EUR_AF=0.0209,0.0099;SAS_AF=0.0082,0.2352;VT=INDEL;MULTI_ALLELIC";
            //const string vcfLine3 =
            //    "4	170887158	rs536596553	GCAAAACAAAACAAAA	G	100	PASS	AC=32;AF=0.00638978;AN=5008;NS=2504;DP=30687;EAS_AF=0;AMR_AF=0.0029;AFR_AF=0.0023;EUR_AF=0.0268;SAS_AF=0;VT=INDEL";

            //var sa1 = new SupplementaryPositionCreator();

            //var additionalItems = _oneKGenReader.ExtractItems(vcfLine1).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa1)).ToList();
            //additionalItems.AddRange(_oneKGenReader.ExtractItems(vcfLine2).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa1)));
            //additionalItems.AddRange(_oneKGenReader.ExtractItems(vcfLine3).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa1)));

            //foreach (var item in additionalItems)
            //{
            //    item.SetSupplementaryAnnotations(sa1);
            //}

            //var oneKg =
            //    sa1.SaPosition.AlleleSpecificAnnotations["5"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)
            //        ] as OneKGenAnnotation;

            //Assert.NotNull(oneKg);
            //Assert.True(oneKg.HasConflicts);
            //Assert.Null(oneKg.OneKgAllAc);
        }

        [Fact(Skip = "new SA")]
        public void SpuriousRefMinor()
        {
            //// NIR-903
            //const string vcfLine =
            //    "2	190634102	rs531674661;rs1225108	A	AC,C	100	PASS	AC=18,4905;AF=0.00359425,0.979433;AN=5008;NS=2504;DP=14024;EAS_AF=0.001,0.997;AMR_AF=0.0043,0.9899;AFR_AF=0,0.9402;EUR_AF=0.004,0.996;SAS_AF=0.0102,0.9898;VT=SNP,INDEL;MULTI_ALLELIC";

            //var sa = new SupplementaryPositionCreator();
            //var sa1 = new SupplementaryPositionCreator();

            //var oneKitems = _oneKGenReader.ExtractItems(vcfLine);

            //oneKitems[0].SetSupplementaryAnnotations(sa);
            //oneKitems[1].SetSupplementaryAnnotations(sa1);

            //sa.FinalizePositionalAnnotations();
            //sa1.FinalizePositionalAnnotations();

            //Assert.False(sa.IsRefMinor());
            //Assert.True(sa1.IsRefMinor());
        }

        [Fact(Skip = "new SA")]
        public void BadRefMinor()
        {
            //// NIR-903
            //const string vcfLine1 =
            //    "X	1619046	.	C	A	100	PASS	AC=2620;AF=0.523163;AN=5008;NS=2504;DP=15896;AMR_AF=0.6412;AFR_AF=0.1415;EUR_AF=0.6153;SAS_AF=0.5419;EAS_AF=0.8323;AA=c|||;VT=SNP";
            //const string vcfLine2 =
            //    "X	1619046	.	C	A,G	100	PASS	AC=2163,730;AF=0.431909,0.145767;AN=5008;NS=2504;DP=15896;AMR_AF=0.428,0.3372;AFR_AF=0.1422,0.0159;EUR_AF=0.4036,0.3419;SAS_AF=0.4622,0.1299;EAS_AF=0.8135,0.004;AA=c|||;VT=SNP;MULTI_ALLELIC";

            //var sa1 = new SupplementaryPositionCreator();
            //var sa2 = new SupplementaryPositionCreator();

            //foreach (var oneKitem in _oneKGenReader.ExtractItems(vcfLine1))
            //{
            //    oneKitem.SetSupplementaryAnnotations(sa1);
            //}

            //foreach (var oneKitem in _oneKGenReader.ExtractItems(vcfLine2))
            //{
            //    oneKitem.SetSupplementaryAnnotations(sa2);
            //}

            //sa1.MergeSaCreator(sa2);

            //Assert.False(sa1.IsRefMinor());

            //var oneKgG =
            //    sa1.SaPosition.AlleleSpecificAnnotations["G"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)]
            //        as OneKGenAnnotation;
            //var oneKgA =
            //    sa1.SaPosition.AlleleSpecificAnnotations["A"].Annotations[DataSourceCommon.GetIndex(DataSourceCommon.DataSource.OneKg)]
            //        as OneKGenAnnotation;

            //Assert.NotNull(oneKgG);
            //Assert.NotNull(oneKgA);

            //// all onek entries should also be cleared
            //Assert.True(oneKgA.HasConflicts);
            //Assert.False(oneKgG.HasConflicts);
            //Assert.Equal("0.145767", GetAlleleFrequency(oneKgG.OneKgAllAn, oneKgG.OneKgAllAc));
        }

        [Fact(Skip = "new SA")]
        public void RefMinorNonSnv()
        {
            //// NIR-903
            //const string vcfLine1 =
            //    "X	1619046	.	C	A	100	PASS	AC=2620;AF=0.53163;AN=5008;NS=2504;DP=15896;AMR_AF=0.6412;AFR_AF=0.1415;EUR_AF=0.6153;SAS_AF=0.5419;EAS_AF=0.8323;AA=c|||;VT=SNP";
            //const string vcfLine2 =
            //    "X	1619046	.	C	AG	100	PASS	AC=2163,730;AF=0.431909;AN=5008;NS=2504;DP=15896;AMR_AF=0.428;AFR_AF=0.1422;EUR_AF=0.4036;SAS_AF=0.4622;EAS_AF=0.8135;AA=c|||;VT=SNP;MULTI_ALLELIC";

            //var oneKItem1 = _oneKGenReader.ExtractItems(vcfLine1)[0];
            //var oneKItem2 = _oneKGenReader.ExtractItems(vcfLine2)[0];

            //var sa1 = new SupplementaryPositionCreator();
            //var sa2 = new SupplementaryPositionCreator();
            //oneKItem1.SetSupplementaryAnnotations(sa1);
            //oneKItem2.SetSupplementaryAnnotations(sa2);

            //sa1.MergeSaCreator(sa2);

            //Assert.False(sa1.IsRefMinor());
        }

        [Fact]
        public void AlleleFrequencyTest()
        {
            var oneKItem = _oneKGenReader.ExtractItems(VcfLine1)[0].GetJsonString();

            Assert.Equal("0.4375", GetAlleleFrequency(oneKItem, "allAf"));
            Assert.Equal("0.47882", GetAlleleFrequency(oneKItem, "afrAf"));
            Assert.Equal("0.410663", GetAlleleFrequency(oneKItem, "amrAf"));
            Assert.Equal("0.430556", GetAlleleFrequency(oneKItem, "easAf"));
            Assert.Equal("0.426441", GetAlleleFrequency(oneKItem, "eurAf"));
            Assert.Equal("0.419223", GetAlleleFrequency(oneKItem, "sasAf"));
            Assert.DoesNotContain("ancestralAllele", oneKItem);
        }

        [Fact]
        public void MultiAltAlleleTest()
        {
            var oneKGenItems = _oneKGenReader.ExtractItems(VcfLine2);

            Assert.Equal(2, oneKGenItems.Count);

            var json1 = oneKGenItems[0].GetJsonString();
            var json2 = oneKGenItems[1].GetJsonString();

            Assert.Equal("0.347244", GetAlleleFrequency(json1, "allAf"));
            Assert.Equal("0.322995", GetAlleleFrequency(json1, "afrAf"));
            Assert.Equal("0.275216", GetAlleleFrequency(json1, "amrAf"));
            Assert.Equal("0.481151", GetAlleleFrequency(json1, "easAf"));
            Assert.Equal("0.292247", GetAlleleFrequency(json1, "eurAf"));
            Assert.Equal("0.349693", GetAlleleFrequency(json1, "sasAf"));

            Assert.Equal("0.640974", GetAlleleFrequency(json2, "allAf"));
            Assert.Equal("0.636914", GetAlleleFrequency(json2, "afrAf"));
            Assert.Equal("0.720461", GetAlleleFrequency(json2, "amrAf"));
            Assert.Equal("0.518849", GetAlleleFrequency(json2, "easAf"));
            Assert.Equal("0.707753", GetAlleleFrequency(json2, "eurAf")); //double check this one: 0.7077535
            Assert.Equal("0.647239", GetAlleleFrequency(json2, "sasAf"));
        }

        [Fact]
        public void MultiAltAlleleAncesterTest()
        {
            var oneKGenItems = _oneKGenReader.ExtractItems(VcfLine2);

            Assert.Equal(2, oneKGenItems.Count);
            Assert.Contains("\"ancestralAllele\":\"g\"", oneKGenItems[0].GetJsonString());
            Assert.Contains("\"ancestralAllele\":\"g\"", oneKGenItems[1].GetJsonString());
        }

        [Fact(Skip = "new SA")]
        public void RefAlleleMinor()
        {
            //var sa = new SupplementaryPositionCreator();

            //foreach (var oneKitem in _oneKGenReader.ExtractItems(VcfLine2))
            //{
            //    oneKitem.SetSupplementaryAnnotations(sa);
            //}
            //sa.FinalizePositionalAnnotations();
            //Assert.Equal(sa.IsRefMinor(), true);
        }

        [Fact(Skip = "new SA")]
        public void RefAlleleMinorDeletion()
        {
            //var oneKItem = _oneKGenReader.ExtractItems(VcfLine3)[0];
            //var sa = new SupplementaryPositionCreator();

            //var additionalItems = new List<SupplementaryDataItem>
            //{
            //    oneKItem.SetSupplementaryAnnotations(sa)
            //};

            ////sa.Clear();
            //foreach (var item in additionalItems)
            //{
            //    item.SetSupplementaryAnnotations(sa);
            //}

            //// when only SNVs are considered this should be false
            //Assert.False(sa.IsRefMinor());
        }

        [Fact(Skip = "new SA")]
        public void RefAlleleMajor()
        {
            //var sa = new SupplementaryPositionCreator();

            //foreach (var oneKitem in _oneKGenReader.ExtractItems(VcfLine1))
            //{
            //    oneKitem.SetSupplementaryAnnotations(sa);
            //}

            //Assert.False(sa.IsRefMinor());
        }

        [Fact(Skip = "new SA")]
        public void BadRefMinor2()
        {
            //// NIR-1368
            //const string vcfLine1 =
            //    "X	1389061	.	A	C	100	PASS	AC=3235;AF=0.645966;AN=5008;NS=2504;DP=13425;AMR_AF=0.7262;AFR_AF=0.2504;EUR_AF=0.8827;SAS_AF=0.7955;EAS_AF=0.7282;AA=a|||;VT=SNP";
            //const string vcfLine2 =
            //    "X	1389061	.	A	C,T	100	PASS	AC=2120,1771;AF=0.423323,0.353634;AN=5008;NS=2504;DP=13425;AMR_AF=0.4625,0.2997;AFR_AF=0.087,0.5998;EUR_AF=0.6551,0.2306;SAS_AF=0.5859,0.2157;EAS_AF=0.4484,0.3244;AA=a|||;VT=SNP;MULTI_ALLELIC";

            //var sa1 = new SupplementaryPositionCreator();
            //var sa2 = new SupplementaryPositionCreator();

            //foreach (var oneKitem in _oneKGenReader.ExtractItems(vcfLine1))
            //{
            //    oneKitem.SetSupplementaryAnnotations(sa1);
            //}

            //foreach (var oneKitem in _oneKGenReader.ExtractItems(vcfLine2))
            //{
            //    oneKitem.SetSupplementaryAnnotations(sa2);
            //}

            //sa1.MergeSaCreator(sa2);

            //Assert.False(sa1.IsRefMinor());
        }

        [Fact]
        public void PrioretizingSymbolicAllele4Svs()
        {
            const string vcfLine =
                "X	101155257	rs373174489	GTGCAAAAGCTCTTTAGTTTAATTAGGTCTCAGCTATTTATCTTTGTTCTTAT	G	100	PASS	AN=3775;AC=1723;AF=0.456424;AA=;EAS_AN=764;EAS_AC=90;EAS_AF=0.1178;EUR_AN=766;EUR_AC=439;EUR_AF=0.5731;AFR_AN=1003;AFR_AC=839;AFR_AF=0.8365;AMR_AN=524;AMR_AC=180;AMR_AF=0.3435;SAS_AN=718;SAS_AC=175;SAS_AF=0.2437";

            var oneKItems = _oneKGenReader.ExtractItems(vcfLine);
            Assert.False(oneKItems[0].IsInterval);
            var json1 = oneKItems[0].GetJsonString();
            Assert.Equal("0.456424", GetAlleleFrequency(json1, "allAf"));
            Assert.Equal("0.836491", GetAlleleFrequency(json1, "afrAf"));
            Assert.Equal("0.343511", GetAlleleFrequency(json1, "amrAf"));
            Assert.Equal("0.117801", GetAlleleFrequency(json1, "easAf"));
            Assert.Equal("0.573107", GetAlleleFrequency(json1, "eurAf"));
            Assert.Equal("0.243733", GetAlleleFrequency(json1, "sasAf"));

        }

        [Fact]
        public void HashCode()
        {
            var chr1 = new Chromosome("chr1", "1", 0);
            var onekItem = new OneKGenItem(chr1, 100, "rs1001", "A", "C", "a", null, null, null, null, null, null, null, null, null, null, null, null, null, 0, null, null, null, null, null, 100, 0, 0);

            var onekHash = new HashSet<OneKGenItem> { onekItem };

            Assert.Single(onekHash);
            Assert.Contains(onekItem, onekHash);
        }

        [Fact]
        public void OnekGenSvReader()
        {
            var inputFileInfo = new FileInfo(Resources.InputFiles("1000G_SVs.tsv"));

            var svReader = new OneKGenSvReader(inputFileInfo, _refChromDict);

            var svItemList = svReader.GetOneKGenSvItems().ToList();

            var si = svItemList[0].GetSupplementaryInterval();

            Assert.Equal("esv3584976", si.StringValues["id"]);
            Assert.Equal(668631, si.Start);
            Assert.Equal(850204, si.End);
            Assert.Equal(VariantType.copy_number_gain, si.VariantType);
            Assert.Equal(0.02396, si.PopulationFrequencies["variantFreqAll"]);
            Assert.Equal(2504, si.IntValues["sampleSize"]);

            //checking out the next item that should be a copy number variant (both loss and gain)
            si = svItemList[1].GetSupplementaryInterval();

            Assert.Equal("esv3584977;esv3584978", si.StringValues["id"]);
            Assert.Equal(713045, si.Start);
            Assert.Equal(755966, si.End);
            Assert.Equal(VariantType.copy_number_variation, si.VariantType);

            //next one is a del (copy_number_loss)
            si = svItemList[2].GetSupplementaryInterval();

            Assert.Equal("esv3584979", si.StringValues["id"]);
            Assert.Equal(738571, si.Start);
            Assert.Equal(742020, si.End);
            Assert.Equal(VariantType.copy_number_loss, si.VariantType);
        }
    }
}
