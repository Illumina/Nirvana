using System.Collections.Generic;
using System.Linq;
using Illumina.VariantAnnotation.DataStructures.SupplementaryAnnotations;
using InputFileParsers.ClinVar;
using InputFileParsers.Cosmic;
using InputFileParsers.DbSnp;
using InputFileParsers.Evs;
using InputFileParsers.OneKGen;
using InputFileParsers.SupplementaryData;
using Xunit;

namespace NirvanaUnitTests.SupplementaryAnnotations
{
    public sealed class MergeSaTests
    {
		private OneKGenReader _oneKGenReader = new OneKGenReader();
		[Fact]
        public void MergeDbSnpItems()
        {
            const string vcfLine1 = "1	10228	rs143255646	TA	T	.	.	RS=143255646;RSPOS=10229;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000020005000002000200;WGT=1;VC=DIV;R5;ASP";
            const string vcfLine2 = "1	10228	rs200462216	TAACCCCTAACCCTAACCCTAAACCCTA	T	.	.	RS=200462216;RSPOS=10229;dbSNPBuildID=137;SSR=0;SAO=0;VP=0x050000020005000002000200;WGT=1;VC=DIV;R5;ASP";
            
            var sa = new SupplementaryAnnotation();
            var dbSnpItem1 = DbSnpReader.ExtractDbSnpItem(vcfLine1)[0];
			var dbSnpItem2 = DbSnpReader.ExtractDbSnpItem(vcfLine2)[0];

			var additionalItems= new List<ISupplementaryDataItem>
			{
				dbSnpItem1.SetSupplementaryAnnotations(sa),
				dbSnpItem2.SetSupplementaryAnnotations(sa)
			};

	        sa.Clear();
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa);
            }

            Assert.Equal(sa.AlleleSpecificAnnotations["1"].DbSnp, new List<string> { "143255646" });
            Assert.Equal(sa.AlleleSpecificAnnotations["27"].DbSnp, new List<string> { "200462216" });
        }

        [Fact]
        public void MergeMultipleDbSnpItems()
        {
            const string vcfLine1 =
                "1	1469597	rs3118506	GCG	GC,GG	.	.	RS=3118506;RSPOS=1469598;RV;dbSNPBuildID=103;SSR=0;SAO=0;VP=0x050000800005000002000110;WGT=1;VC=SNV;U3;ASP;NOC";
            const string vcfLine2 =
                "1	1469598	rs368645009	CG	C	.	.	RS=368645009;RSPOS=1469599;RV;dbSNPBuildID=138;SSR=0;SAO=0;VP=0x050000800005000002000200;WGT=1;VC=DIV;U3;ASP";

            var sa1        = new SupplementaryAnnotation();
            var sa2        = new SupplementaryAnnotation();
            var dbSnpItems = DbSnpReader.ExtractDbSnpItem(vcfLine1);
	        
            var dbSnpItem2 = DbSnpReader.ExtractDbSnpItem(vcfLine2)[0];

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

            sa1.MergeAnnotations(sa2);

            var expectedDbSnp = new List<string> { "3118506","368645009" };
            Assert.Equal(expectedDbSnp, sa1.AlleleSpecificAnnotations["1"].DbSnp);            
        }

        [Fact]
        public void MultipleDbsnpMerge()
        {
            // NIR-778, 805. The second dbSNP id is missing from the SA database.
            const string vcfLine1 =
                "17	3616153	rs34081014	C	G	.	.	RS=34081014;RSPOS=3616153;dbSNPBuildID=126;SSR=0;SAO=0;VP=0x050000000005140136000100;WGT=1;VC=SNV;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=0.9297,0.07029;COMMON=1";

            const string vcfLine2 =
                "17	3616152	rs71362546	GCTG	GCTT,GGTG	.	.	RS=71362546;RSPOS=3616153;dbSNPBuildID=130;SSR=0;SAO=0;VP=0x050100000005000102000810;WGT=1;VC=MNV;SLO;ASP;GNO;NOC";

            var sa1 = new SupplementaryAnnotation();
            var sa2 = new SupplementaryAnnotation();
            var dbSnpItem1 = DbSnpReader.ExtractDbSnpItem(vcfLine1)[0];
            var dbSnpItems = DbSnpReader.ExtractDbSnpItem(vcfLine2);

            dbSnpItem1.SetSupplementaryAnnotations(sa1);

			var additionalItems = dbSnpItems.Select(dbSnpItem => dbSnpItem.SetSupplementaryAnnotations(sa1)).ToList();

	        foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa2);
            }

            sa1.MergeAnnotations(sa2);

            Assert.Equal(sa1.AlleleSpecificAnnotations["G"].DbSnp, new List<string> { "34081014", "71362546" });

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

			var dbSnpItems1 = DbSnpReader.ExtractDbSnpItem(vcfLine1);
			var dbSnpItems2 = DbSnpReader.ExtractDbSnpItem(vcfLine2);
			var dbSnpItems3 = DbSnpReader.ExtractDbSnpItem(vcfLine3);


			var sa1 = new SupplementaryAnnotation();
			var sa2 = new SupplementaryAnnotation();
			var sa3 = new SupplementaryAnnotation();

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
			sa1.MergeAnnotations(sa2);
			sa1.MergeAnnotations(sa3);
			
			Assert.Equal(2, sa1.AlleleSpecificAnnotations["iAAT"].DbSnp.Count);
			Assert.Equal("34500567", sa1.AlleleSpecificAnnotations["iAAT"].DbSnp[0]);
			Assert.Equal("59792241", sa1.AlleleSpecificAnnotations["iAAT"].DbSnp[1]);
		}

		
        [Fact]
        public void MergeConflictingOneKitems()
        {
            const string vcfLine1 =
                "1	11408760	rs112877363	CTATG	C	100	PASS	AC=6;AF=0.00119808;AN=5008;NS=2504;DP=23213;EAS_AF=0;AMR_AF=0;AFR_AF=0.0045;EUR_AF=0;SAS_AF=0";
            const string vcfLine2 =
                "1	11408760	rs59160279	CTATG	CTATGTATG,C	100	PASS	AC=174,763;AF=0.0347444,0.152356;AN=5008;NS=2504;DP=23213;EAS_AF=0.0069,0.0615;AMR_AF=0.0259,0.062;AFR_AF=0.0749,0.4213;EUR_AF=0.0378,0.0239;SAS_AF=0.0123,0.0787";

            var sa1 = new SupplementaryAnnotation();
            var sa2 = new SupplementaryAnnotation();
            var oneKitem1 = _oneKGenReader.ExtractOneKGenItem(vcfLine1)[0];

	        var additionalItems = new List<ISupplementaryDataItem>()
	        {
		        oneKitem1.SetSupplementaryAnnotations(sa1)
	        };
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa1);
            }

			additionalItems.Clear();
	        additionalItems.AddRange(_oneKGenReader.ExtractOneKGenItem(vcfLine2).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa2)));

	        foreach (var item in additionalItems)
			{
				item.SetSupplementaryAnnotations(sa2);
			}


            sa1.MergeAnnotations(sa2);

			// For conflicting entries, we clear all fields
			Assert.True(sa1.AlleleSpecificAnnotations["4"].HasMultipleOneKgenEntries);
			// Assert.Null( sa1.AlleleSpecificAnnotations["4"].OneKgAll);

        }

        [Fact]
        public void MergeConflictingOneKitemsSnv()
        {
            const string vcfLine1 =
                "X	129354240	rs1160681	C	A	100	PASS	AC=1996;AF=0.528742;AN=3775;NS=2504;DP=10421;AMR_AF=0.353;AFR_AF=0.5953;EUR_AF=0.3052;SAS_AF=0.3896;EAS_AF=0.2738;AA=C|||;VT=SNP";

            const string vcfLine2 =
                "X	129354240	.	C	A,G	100	PASS	AC=1981,15;AF=0.524768,0.00397351;AN=3775;NS=2504;DP=10421;AMR_AF=0.353,0;AFR_AF=0.584,0.0113;EUR_AF=0.3052,0;SAS_AF=0.3896,0;EAS_AF=0.2738,0;AA=C|||;VT=SNP;MULTI_ALLELIC";

            var sa1 = new SupplementaryAnnotation();
            var sa2 = new SupplementaryAnnotation();
            var oneKitem1 = _oneKGenReader.ExtractOneKGenItem(vcfLine1)[0];
            
            oneKitem1.SetSupplementaryAnnotations(sa1);

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(vcfLine2))
			{
				oneKitem.SetSupplementaryAnnotations(sa2);
			}

            sa1.MergeAnnotations(sa2);

			Assert.True(sa1.AlleleSpecificAnnotations["A"].HasMultipleOneKgenEntries);
			// Assert.Null(sa1.AlleleSpecificAnnotations["A"].OneKgAll);

        }

        [Fact]
        public void MergeConflictingOneKitems1()
        {
            const string vcfLine1 =
                "1	20505705	rs35377696	C	CTCTG,CTG,CTGTG	100	PASS	AC=46,1513,152;AF=0.0091853,0.302117,0.0303514;AN=5008;NS=2504;DP=23578;EAS_AF=0,0.2718,0.0268;AMR_AF=0.0086,0.2939,0.0072;AFR_AF=0.0303,0.2693,0.0756;EUR_AF=0,0.3032,0.001;SAS_AF=0,0.3824,0.0194";
            const string vcfLine2 =
                "1	20505705	.	C	CTG	100	PASS	AC=4;AF=0.000798722;AN=5008;NS=2504;DP=23578;EAS_AF=0.002;AMR_AF=0;AFR_AF=0.0008;EUR_AF=0.001;SAS_AF=0";

            var sa1 = new SupplementaryAnnotation();
            var sa2 = new SupplementaryAnnotation();
            
            var additionalItems = _oneKGenReader.ExtractOneKGenItem(vcfLine1).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa1)).ToList();

	        foreach (var item in additionalItems)
	        {
		        item.SetSupplementaryAnnotations(sa1);
	        }

			additionalItems.Clear();
	        additionalItems.AddRange(_oneKGenReader.ExtractOneKGenItem(vcfLine2).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa2)));

	        foreach (var item in additionalItems)
	        {
		        item?.SetSupplementaryAnnotations(sa2);
	        }

	        sa1.MergeAnnotations(sa2);

			Assert.True(sa1.AlleleSpecificAnnotations["iTG"].HasMultipleOneKgenEntries);
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

			var sa1 = new SupplementaryAnnotation();
			var sa2 = new SupplementaryAnnotation();
			var oneKitem1 = _oneKGenReader.ExtractOneKGenItem(vcfLine1)[0];

			var additionalItems = new List<ISupplementaryDataItem>()
			{
				oneKitem1.SetSupplementaryAnnotations(sa1)
			};
			foreach (var item in additionalItems)
			{
				item.SetSupplementaryAnnotations(sa1);
			}

			additionalItems.Clear();
			additionalItems.AddRange(_oneKGenReader.ExtractOneKGenItem(vcfLine2).Select(oneKitem => oneKitem.SetSupplementaryAnnotations(sa2)));

			foreach (var item in additionalItems)
			{
				item?.SetSupplementaryAnnotations(sa2);
			}
			
			sa1.MergeAnnotations(sa2);

			Assert.True(sa1.AlleleSpecificAnnotations["iTCTC"].HasMultipleOneKgenEntries);
			// Assert.Null(sa1.AlleleSpecificAnnotations["iTCTC"].OneKgAll);

		}

		[Fact]
        public void MergeDbSnpCosmic()
        {
            const string vcfLine1 = "1	10228	rs143255646	TA	T	.	.	RS=143255646;RSPOS=10229;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000020005000002000200;WGT=1;VC=DIV;R5;ASP";
            

            var sa = new SupplementaryAnnotation();
            var dbSnpItem1 = DbSnpReader.ExtractDbSnpItem(vcfLine1)[0];
	        var additionalItems = new List<ISupplementaryDataItem>()
	        {
		        dbSnpItem1.SetSupplementaryAnnotations(sa)
	        };

            var cosmicItem1 = new CosmicItem("1", 10229, "COSM1000", "TA", "T", "TP53", "carcinoma", "oesophagus", "");
            var cosmicItem2 = new CosmicItem("1", 10229, "COSM1000", "TA", "T", "TP53", "carcinoma", "large_intestine", "01");

            additionalItems.Add(cosmicItem1.SetSupplementaryAnnotations(sa));
            additionalItems.Add(cosmicItem2.SetSupplementaryAnnotations(sa));

            sa.Clear();
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa);
            }

            Assert.Equal(sa.AlleleSpecificAnnotations["1"].DbSnp, new List<string> { "143255646"});
            Assert.True(sa.ContainsCosmicItem("COSM1000"));
        }

        [Fact]
        public void MergeDbSnpCosmic1Kg()
        {
            const string vcfLine1 = "1	10228	rs143255646	TA	T	.	.	RS=143255646;RSPOS=10229;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050000020005000002000200;WGT=1;VC=DIV;R5;ASP";
            const string vcfLine2="1	10228	.	TA	T	100	PASS	AC=2130;AF=0.425319;AN=5008;NS=2504;DP=103152;EAS_AF=0.3363;AMR_AF=0.3602;AFR_AF=0.4909;EUR_AF=0.4056;SAS_AF=0.4949;AA=|||unknown(NO_COVERAGE)";

            var sa = new SupplementaryAnnotation();
            var dbSnpItem1 = DbSnpReader.ExtractDbSnpItem(vcfLine1)[0];
			var additionalItems = new List<ISupplementaryDataItem>()
	        {
		        dbSnpItem1.SetSupplementaryAnnotations(sa)
	        };


            var cosmicItem1 = new CosmicItem("1", 10229, "COSM1000", "TA", "T", "TP53", "carcinoma", "oesophagus", "");
            additionalItems.Add(cosmicItem1.SetSupplementaryAnnotations(sa));

            var oneKGenItem = _oneKGenReader.ExtractOneKGenItem(vcfLine2)[0];
            additionalItems.Add(oneKGenItem.SetSupplementaryAnnotations(sa));

            sa.Clear();
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa);
            }

            Assert.Equal(sa.AlleleSpecificAnnotations["1"].DbSnp, new List<string> { "143255646" });
            Assert.Equal(sa.AlleleSpecificAnnotations["1"].OneKgAll, "0.425319");

            Assert.True(sa.ContainsCosmicItem("COSM1000"));
        }

        [Fact]
        public void MergeDbSnp1KpEvs()
        {
            const string vcfLine1 = "1	69428	rs140739101	T	G	.	.	RS=140739101;RSPOS=69428;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050200000a05140026000100;WGT=1;VC=SNV;S3D;NSM;REF;ASP;VLD;KGPhase3;CAF=0.981,0.01897;COMMON=1";
            const string vcfLine2 = "1	69428	rs140739101	T	G	100	PASS	AC=95;AF=0.0189696;AN=5008;NS=2504;DP=17611;EAS_AF=0.003;AMR_AF=0.036;AFR_AF=0.0015;EUR_AF=0.0497;SAS_AF=0.0153;AA=.|||";
            const string vcfLine3 = "1	69428	rs140739101	T	G	.	PASS	BSNP=dbSNP_134;EA_AC=313,6535;AA_AC=14,3808;TAC=327,10343;MAF=4.5707,0.3663,3.0647;GTS=GG,GT,TT;EA_GTC=92,129,3203;AA_GTC=1,12,1898;GTC=93,141,5101;DP=110;GL=OR4F5;CP=1.0;CG=0.9;AA=T;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_001005484.1:missense;HGVS_CDNA_VAR=NM_001005484.1:c.338T>G;HGVS_PROTEIN_VAR=NM_001005484.1:p.(F113C);CDS_SIZES=NM_001005484.1:918;GS=205;PH=probably-damaging:0.999;EA_AGE=.;AA_AGE=.";

            var sa = new SupplementaryAnnotation();
            
            var dbSnpItem = DbSnpReader.ExtractDbSnpItem(vcfLine1)[0];
            dbSnpItem.SetSupplementaryAnnotations(sa);

            var oneKGenItem = _oneKGenReader.ExtractOneKGenItem(vcfLine2)[0];
            oneKGenItem.SetSupplementaryAnnotations(sa);

			var evsItem = EvsReader.ExtractEvsItem(vcfLine3)[0];
			evsItem.SetSupplementaryAnnotations(sa);

            Assert.Equal(new List<string> { "140739101" }, sa.AlleleSpecificAnnotations["G"].DbSnp);
            Assert.Equal("0.0497", sa.AlleleSpecificAnnotations["G"].OneKgEur);
            Assert.Equal("0.045707", sa.AlleleSpecificAnnotations["G"].EvsEur);
            Assert.False(sa.IsRefMinorAllele);
        }

        [Fact]
        public void MergeDbSnp1KpEvsRefMinor()
        {
            const string vcfLine1 = "1	69428	rs140739101	T	G	.	.	RS=140739101;RSPOS=69428;dbSNPBuildID=134;SSR=0;SAO=0;VP=0x050200000a05140026000100;WGT=1;VC=SNV;S3D;NSM;REF;ASP;VLD;KGPhase3;CAF=0.981,0.01897;COMMON=1";
            const string vcfLine2 = "1	69428	rs140739101	T	G	100	PASS	AC=95;AF=0.989696;AN=5008;NS=2504;DP=17611;EAS_AF=0.003;AMR_AF=0.036;AFR_AF=0.0015;EUR_AF=0.0497;SAS_AF=0.0153;AA=.|||";
            const string vcfLine3 = "1	69428	rs140739101	T	G	.	PASS	BSNP=dbSNP_134;EA_AC=313,6535;AA_AC=14,3808;TAC=327,10343;MAF=4.5707,0.3663,3.0647;GTS=GG,GT,TT;EA_GTC=92,129,3203;AA_GTC=1,12,1898;GTC=93,141,5101;DP=110;GL=OR4F5;CP=1.0;CG=0.9;AA=T;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_001005484.1:missense;HGVS_CDNA_VAR=NM_001005484.1:c.338T>G;HGVS_PROTEIN_VAR=NM_001005484.1:p.(F113C);CDS_SIZES=NM_001005484.1:918;GS=205;PH=probably-damaging:0.999;EA_AGE=.;AA_AGE=.";

            var sa = new SupplementaryAnnotation();

            var dbSnpItem = DbSnpReader.ExtractDbSnpItem(vcfLine1)[0];
            dbSnpItem.SetSupplementaryAnnotations(sa);

            var oneKGenItem = _oneKGenReader.ExtractOneKGenItem(vcfLine2)[0];
            oneKGenItem.SetSupplementaryAnnotations(sa);

			var evsItem = EvsReader.ExtractEvsItem(vcfLine3)[0];
			evsItem.SetSupplementaryAnnotations(sa);

            Assert.Equal(new List<string> { "140739101" }, sa.AlleleSpecificAnnotations["G"].DbSnp);
            Assert.Equal(true, sa.IsRefMinorAllele);
        }

        [Fact]
        public void MultiAlleleMergeDbSnp1KpEvs()
        {
            const string vcfLine1 = "1	1564952	rs112177324	TG	T	.	.	RS=112177324;RSPOS=1564953;dbSNPBuildID=132;SSR=0;SAO=0;VP=0x05010008000514013e000200;WGT=1;VC=DIV;SLO;INT;ASP;VLD;GNO;KGPhase1;KGPhase3;CAF=0.8468,0.1506;COMMON=1";
            const string vcfLine2 = "1	1564952	rs112177324	TG	TGG,T	100	PASS	AC=13,754;AF=0.00259585,0.150559;AN=5008;NS=2504;DP=8657;EAS_AF=0,0.0933;AMR_AF=0.0014,0.2046;AFR_AF=0.0091,0.0182;EUR_AF=0,0.3588;SAS_AF=0,0.136";
            const string vcfLine3 = "1	1564952	rs112177324	TG	TGG,T	.	PASS	BSNP=dbSNP_132;EA_AC=2,3039,4701;AA_AC=44,279,3231;TAC=46,3318,7932;MAF=39.2793,9.0884,29.7805;GTS=A1A1,A1A2,A1R,A2A2,A2R,RR;EA_GTC=0,1,1,707,1624,1538;AA_GTC=4,4,32,41,193,1503;GTC=4,5,33,748,1817,3041;DP=10;GL=MIB2;CP=0.8;CG=-0.0;AA=.;CA=.;EXOME_CHIP=no;GWAS_PUBMED=.;FG=NM_080875.2:intron,NM_080875.2:intron,NM_001170689.1:intron,NM_001170689.1:intron,NM_001170688.1:intron,NM_001170688.1:intron,NM_001170687.1:intron,NM_001170687.1:intron,NM_001170686.1:intron,NM_001170686.1:intron;HGVS_CDNA_VAR=NM_080875.2:c.2908+7del1,NM_080875.2:c.2908+6_2908+7insG,NM_001170689.1:c.2187-66del1,NM_001170689.1:c.2187-67_2187-66insG,NM_001170688.1:c.2713+7del1,NM_001170688.1:c.2713+6_2713+7insG,NM_001170687.1:c.2866+7del1,NM_001170687.1:c.2866+6_2866+7insG,NM_001170686.1:c.2896+7del1,NM_001170686.1:c.2896+6_28967insG;HGVS_PROTEIN_VAR=.,.,.,.,.,.,.,.,.,.;CDS_SIZES=NM_080875.2:3213,NM_080875.2:3213,NM_001170689.1:2262,NM_001170689.1:2262,NM_001170688.1:3018,NM_001170688.1:3018,NM_001170687.1:3171,NM_001170687.1:3171,NM_001170686.1:3201,NM_001170686.1:3201;GS=.,.,.,.,.,.,.,.,.,.;PH=.,.,.,.,.,.,.,.,.,.;EA_AGE=.;AA_AGE=.";

            var sa = new SupplementaryAnnotation();

            var dbSnpItem = DbSnpReader.ExtractDbSnpItem(vcfLine1)[0];
			var additionalItems = new List<ISupplementaryDataItem>()
	        {
		        dbSnpItem.SetSupplementaryAnnotations(sa)
	        };

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(vcfLine2))
			{
				additionalItems.Add(oneKitem.SetSupplementaryAnnotations(sa));
			}
            
			var evsItemsList = EvsReader.ExtractEvsItem(vcfLine3);

			foreach (var evsItem in evsItemsList)
			{
				additionalItems.Add(evsItem.SetSupplementaryAnnotations(sa));
			}

			foreach (var item in additionalItems)
			{
				item.SetSupplementaryAnnotations(sa);
			}


            sa.Clear();
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa);
            }

            Assert.Equal(new List<string> { "112177324" }, sa.AlleleSpecificAnnotations["1"].DbSnp);

            Assert.Equal(sa.AlleleSpecificAnnotations["iG"].OneKgAll, "0.00259585");
            Assert.Equal(sa.AlleleSpecificAnnotations["1"].OneKgAll, "0.150559");
            
            Assert.Equal( "0.012380" , sa.AlleleSpecificAnnotations["iG"].EvsAfr);
            Assert.Equal( "0.000258", sa.AlleleSpecificAnnotations["iG"].EvsEur);
            Assert.Equal("0.004072", sa.AlleleSpecificAnnotations["iG"].EvsAll);

            Assert.Equal("0.078503", sa.AlleleSpecificAnnotations["1"].EvsAfr);
            Assert.Equal("0.392534", sa.AlleleSpecificAnnotations["1"].EvsEur);
            Assert.Equal("0.293732", sa.AlleleSpecificAnnotations["1"].EvsAll);
        }

        [Fact]
        public void MergeDbSnpClinVar()
        {
            const string vcfLine = "1	2160305	rs387907306	G	A,T	.	.	RS=387907306;RSPOS=2160305;dbSNPBuildID=137;SSR=0;SAO=0;VP=0x050060000a05000002110100;GENEINFO=SKI:6497;WGT=1;VC=SNV;PM;NSM;REF;ASP;LSD;OM;CLNALLE=1,2;CLNHGVS=NC_000001.10:g.2160305G>A,NC_000001.10:g.2160305G>T;CLNSRC=ClinVar|OMIM_Allelic_Variant,ClinVar|OMIM_Allelic_Variant;CLNORIGIN=1,1;CLNSRCID=NM_003036.3:c.100G>A|164780.0004,NM_003036.3:c.100G>T|164780.0005;CLNSIG=5,5;CLNDSDB=GeneReviews:MedGen:OMIM:Orphanet:SNOMED_CT,GeneReviews:MedGen:OMIM:Orphanet:SNOMED_CT;CLNDSDBID=NBK1277:C1321551:182212:ORPHA2462:83092002,NBK1277:C1321551:182212:ORPHA2462:83092002;CLNDBN=Shprintzen-Goldberg_syndrome,Shprintzen-Goldberg_syndrome;CLNREVSTAT=single,single;CLNACC=RCV000030819.24,RCV000030820.24";

            var dbSnpItems = DbSnpReader.ExtractDbSnpItem(vcfLine);
            
            var sa = new SupplementaryAnnotation();
            var sa1 = new SupplementaryAnnotation();

	        foreach (var dbSnpItem in dbSnpItems)
	        {
				dbSnpItem.SetSupplementaryAnnotations(sa);    
	        }

            var clinVarItems = ClinVarReader.ExtractClinVarItems(vcfLine);

            foreach (var clinVarItem in clinVarItems)
            {
                sa1.Clear();
                clinVarItem.SetSupplementaryAnnotations(sa1);
                sa.MergeAnnotations(sa1);
            }
            Assert.Equal(2, sa.ClinVarEntries.Count);
           Assert.Equal(sa.AlleleSpecificAnnotations["A"].DbSnp, new List<string> { "387907306" });
           Assert.Equal(sa.AlleleSpecificAnnotations["T"].DbSnp, new List<string> { "387907306" });

           foreach (var clinVarEntry in sa.ClinVarEntries)
           {
               if (clinVarEntry.SaAltAllele.Equals("A"))
               {
                   Assert.Equal(clinVarEntry.ID, "RCV000030819.24");
                   Assert.Equal(clinVarEntry.OrphanetID, "ORPHA2462");
                   Assert.Equal(clinVarEntry.Phenotype, "Shprintzen-Goldberg_syndrome");
               }
               if (clinVarEntry.SaAltAllele.Equals("T"))
               {
                   Assert.Equal(clinVarEntry.ID, "RCV000030820.24");
                   Assert.Equal(clinVarEntry.OrphanetID, "ORPHA2462");
                   Assert.Equal(clinVarEntry.Phenotype, "Shprintzen-Goldberg_syndrome");
               }
           }
        }
    }
}
