using System.Collections.Generic;
using System.Linq;
using Illumina.VariantAnnotation.DataStructures.SupplementaryAnnotations;
using InputFileParsers.OneKGen;
using InputFileParsers.SupplementaryData;
using Xunit;

namespace NirvanaUnitTests.FileHandling
{
    public sealed class OneKGenTest
    {
        private const string VcfLine1 = "1	10352	rs145072688	T	TA	100	PASS	AC=2191;AF=0.4375;AN=5008;NS=2504;DP=88915;EAS_AF=0.4306;AMR_AF=0.4107;AFR_AF=0.4788;EUR_AF=0.4264;SAS_AF=0.4192;AA=|||unknown(NO_COVERAGE)";

        private const string VcfLine2 = "1	15274	rs201931625	A	G,T	100	PASS	AC=1739,3210;AF=0.347244,0.640974;AN=5008;NS=2504;DP=23255;EAS_AF=0.4812,0.5188;AMR_AF=0.2752,0.7205;AFR_AF=0.323,0.6369;EUR_AF=0.2922,0.7078;SAS_AF=0.3497,0.6472;AA=g|||";
		
		private const string VcfLine3 = "1	10616	rs376342519	CCGCCGTTGCAAAGGCGCGCCG	C	100	PASS	AC=4973;AF=0.993011;AN=5008;NS=2504;DP=2365;EAS_AF=0.9911;AMR_AF=0.9957;AFR_AF=0.9894;EUR_AF=0.994;SAS_AF=0.9969";

        // have been modified to make the first alt allele very freq
        private const string VcfLine4 =
            "1	806324	.	G	GATA,T	100	PASS	AC=6,2;AF=0.9808,0.000399361;AN=5008;NS=2504;DP=17889;EAS_AF=0,0;AMR_AF=0,0;AFR_AF=0.0045,0.0015;EUR_AF=0,0;SAS_AF=0,0";

        private const string VcfLine5 =
            "1	985465	.	G	A,GT	100	PASS	AC=2,11;AF=0.0399361,0.00219649;AN=5008;NS=2504;DP=7139;EAS_AF=0,0;AMR_AF=0,0;AFR_AF=0,0;EUR_AF=0.002,0.002;SAS_AF=0,0.0092";

        // VcfLine6 is an artificial entry for testing only
        private const string VcfLine6 =
            "1	985465	.	G	C	100	PASS	AC=2,11;AF=0.9361;AN=5008;NS=2504;DP=7139;EAS_AF=0;AMR_AF=0;AFR_AF=0;EUR_AF=0.002;SAS_AF=0";

		private OneKGenReader _oneKGenReader = new OneKGenReader();
		[Fact]
        public void MultiEntryMixedVariant()
        {
	        var sa1 = new SupplementaryAnnotation();

			var oneKItem1 = _oneKGenReader.ExtractOneKGenItem(VcfLine5)[0];
            var oneKItem2 = _oneKGenReader.ExtractOneKGenItem(VcfLine6)[0];

	        oneKItem1.SetSupplementaryAnnotations(sa1);

            // additional items are ignored since they cannot be SNVs
            var sa2 = new SupplementaryAnnotation();
            oneKItem2.SetSupplementaryAnnotations(sa2);

            sa1.MergeAnnotations(sa2);

            Assert.True(sa1.IsRefMinorAllele);
        }

        [Fact]
        public void MixedVariantType()
        {
            var oneKItem = _oneKGenReader.ExtractOneKGenItem(VcfLine4)[1];

            var sa = new SupplementaryAnnotation();

            oneKItem.SetSupplementaryAnnotations(sa);

            Assert.False(sa.IsRefMinorAllele);
        }

	    [Fact]
	    public void AncestralAlleleN()
	    {
		    const string vcfLine =
			    "1	30923	rs806731	G	T	100	PASS	AC=4369;AF=0.872404;AN=5008;NS=2504;DP=13565;EAS_AF=0.996;AMR_AF=0.9164;AFR_AF=0.6687;EUR_AF=0.9364;SAS_AF=0.9233;AA=N|||;VT=SNP";
			
			var oneKItem = _oneKGenReader.ExtractOneKGenItem(vcfLine)[0];
			
			var sa = new SupplementaryAnnotation();

			oneKItem.SetSupplementaryAnnotations(sa);

			Assert.Equal("N", sa.AlleleSpecificAnnotations["T"].AncestralAllele);
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

			// in some cases, the merge happens using setSupplementaryAnnotation(). this unit test checks if that path is ok
			Assert.True(sa1.AlleleSpecificAnnotations["iTCTC"].HasMultipleOneKgenEntries);

		}

		[Fact]
		public void NonConflictingOneKitems1()
		{
			// NIR-1147
			const string vcfLine1 =
				"X	5331877	rs71800267	AAC	AACAC,A	100	PASS	AC=159,562;AF=0.0421192,0.148874;AN=3775;NS=2504;OLD_VARIANT=X:5331899:CAC/CACAC/C;DP=9474;AMR_AF=0.0014,0.0908;AFR_AF=0.025,0.2769;EUR_AF=0.0109,0.0835;SAS_AF=0.0481,0.0307;EAS_AF=0.0665,0.0188;VT=INDEL;MULTI_ALLELIC";

			const string vcfLine2 =
				"X	5331877	.	AACACACACAC	A	100	PASS	AC= 101;AF=0.026755;AN=3775;NS=2504;DP=9474;AMR_AF=0.0086;AFR_AF=0.0711;EUR_AF=0.001;SAS_AF=0;EAS_AF=0;VT=INDEL";

			var sa1 = new SupplementaryAnnotation();
			var sa2 = new SupplementaryAnnotation();
			var oneKitem1 = _oneKGenReader.ExtractOneKGenItem(vcfLine1)[1];

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
			// in some cases, the merge happens using setSupplementaryAnnotation(). this unit test checks if that path is ok
			Assert.False(sa1.AlleleSpecificAnnotations["2"].HasMultipleOneKgenEntries);
			Assert.NotNull(sa1.AlleleSpecificAnnotations["2"].OneKgAll);
			Assert.False(sa1.AlleleSpecificAnnotations["10"].HasMultipleOneKgenEntries);

		}

		[Fact]
		public void MissingRefMinor()
		{
			const string vcfLine =
				"1	15274	rs62636497	A	G,T	100	PASS	AC=1739,3210;AF=0.347244,0.640974;AN=5008;NS=2504;DP=23255;EAS_AF=0.4812,0.5188;AMR_AF=0.2752,0.7205;AFR_AF=0.323,0.6369;EUR_AF=0.2922,0.7078;SAS_AF=0.3497,0.6472;AA=g|||;VT=SNP;MULTI_ALLELIC";

			var sa = new SupplementaryAnnotation();

			foreach (var oneKitems in _oneKGenReader.ExtractOneKGenItem(vcfLine))
			{
				oneKitems.SetSupplementaryAnnotations(sa);
			}

			Assert.True(sa.IsRefMinorAllele);

		}

		[Fact]
	    public void AlleleFrequencyArbitration()
	    {
		    const string vcfLine1 = "4	170887158	rs34734657	GCAAAA	G	100	PASS	AC=2222;AF=0.44369;AN=5008;NS=2504;DP=30687;EAS_AF=0.0675;AMR_AF=0.5447;AFR_AF=0.6377;EUR_AF=0.5547;SAS_AF=0.3834;VT=INDEL";
		    const string vcfLine2 = "4	170887158	rs556076439;rs34734657	GCAAAA	GCAAAACAAAA,G	100	PASS	AC=69,650;AF=0.013778,0.129792;AN=5008;NS=2504;DP=30687;EAS_AF=0.0109,0.2331;AMR_AF=0.0331,0.0303;AFR_AF=0.0045,0.1165;EUR_AF=0.0209,0.0099;SAS_AF=0.0082,0.2352;VT=INDEL;MULTI_ALLELIC";
		    const string vcfLine3 =
			    "4	170887158	rs536596553	GCAAAACAAAACAAAA	G	100	PASS	AC=32;AF=0.00638978;AN=5008;NS=2504;DP=30687;EAS_AF=0;AMR_AF=0.0029;AFR_AF=0.0023;EUR_AF=0.0268;SAS_AF=0;VT=INDEL";
			
			var sa1 = new SupplementaryAnnotation();
			
		    var additionalItems = new List<ISupplementaryDataItem>();

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(vcfLine1))
		    {
			    additionalItems.Add(oneKitem.SetSupplementaryAnnotations(sa1));
		    }

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(vcfLine2))
			{
				additionalItems.Add(oneKitem.SetSupplementaryAnnotations(sa1));
			}

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(vcfLine3))
			{
				additionalItems.Add(oneKitem.SetSupplementaryAnnotations(sa1));
			}

		    foreach (var item in additionalItems)
		    {
			    item.SetSupplementaryAnnotations(sa1);
		    }
			
			Assert.Null(sa1.AlleleSpecificAnnotations["5"].OneKgAll);
	    }

	    [Fact]
	    public void SpuriousRefMinor()
	    {
			// NIR-903
		    const string vcfLine =
			    "2	190634102	rs531674661;rs1225108	A	AC,C	100	PASS	AC=18,4905;AF=0.00359425,0.979433;AN=5008;NS=2504;DP=14024;EAS_AF=0.001,0.997;AMR_AF=0.0043,0.9899;AFR_AF=0,0.9402;EUR_AF=0.004,0.996;SAS_AF=0.0102,0.9898;VT=SNP,INDEL;MULTI_ALLELIC";

			var sa = new SupplementaryAnnotation();
		    var sa1 = new SupplementaryAnnotation();

		    var oneKitems = _oneKGenReader.ExtractOneKGenItem(vcfLine);

		    oneKitems[0].SetSupplementaryAnnotations(sa);
			oneKitems[1].SetSupplementaryAnnotations(sa1);
			
			Assert.False(sa.IsRefMinorAllele);
			Assert.True(sa1.IsRefMinorAllele);
	    }

	    [Fact]
		public void BadRefMinor()
		{
			// NIR-903
			const string vcfLine1 =
				"X	1619046	.	C	A	100	PASS	AC=2620;AF=0.523163;AN=5008;NS=2504;DP=15896;AMR_AF=0.6412;AFR_AF=0.1415;EUR_AF=0.6153;SAS_AF=0.5419;EAS_AF=0.8323;AA=c|||;VT=SNP";
			const string vcfLine2 =
				"X	1619046	.	C	A,G	100	PASS	AC=2163,730;AF=0.431909,0.145767;AN=5008;NS=2504;DP=15896;AMR_AF=0.428,0.3372;AFR_AF=0.1422,0.0159;EUR_AF=0.4036,0.3419;SAS_AF=0.4622,0.1299;EAS_AF=0.8135,0.004;AA=c|||;VT=SNP;MULTI_ALLELIC";

			var sa1 = new SupplementaryAnnotation();
			var sa2 = new SupplementaryAnnotation();

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(vcfLine1))
			{
				oneKitem.SetSupplementaryAnnotations(sa1);
			}

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(vcfLine2))
			{
				oneKitem.SetSupplementaryAnnotations(sa2);
			}


			sa1.MergeAnnotations(sa2);

			Assert.False(sa1.IsRefMinorAllele);

			// all onek entries should also be cleared
			Assert.True(sa1.AlleleSpecificAnnotations["A"].HasMultipleOneKgenEntries);
			Assert.False(sa1.AlleleSpecificAnnotations["G"].HasMultipleOneKgenEntries);
			Assert.Equal("0.145767", sa1.AlleleSpecificAnnotations["G"].OneKgAll);

		}

		[Fact]
		public void RefMinorNonSnv()
		{
			// NIR-903
			const string vcfLine1 =
				"X	1619046	.	C	A	100	PASS	AC=2620;AF=0.53163;AN=5008;NS=2504;DP=15896;AMR_AF=0.6412;AFR_AF=0.1415;EUR_AF=0.6153;SAS_AF=0.5419;EAS_AF=0.8323;AA=c|||;VT=SNP";
			const string vcfLine2 =
				"X	1619046	.	C	AG	100	PASS	AC=2163,730;AF=0.431909;AN=5008;NS=2504;DP=15896;AMR_AF=0.428;AFR_AF=0.1422;EUR_AF=0.4036;SAS_AF=0.4622;EAS_AF=0.8135;AA=c|||;VT=SNP;MULTI_ALLELIC";

			var oneKItem1 = _oneKGenReader.ExtractOneKGenItem(vcfLine1)[0];
			var oneKItem2 = _oneKGenReader.ExtractOneKGenItem(vcfLine2)[0];

			var sa1 = new SupplementaryAnnotation();
			var sa2 = new SupplementaryAnnotation();
			oneKItem1.SetSupplementaryAnnotations(sa1);
			oneKItem2.SetSupplementaryAnnotations(sa2);

			sa1.MergeAnnotations(sa2);

			Assert.False(sa1.IsRefMinorAllele);

		}
	    [Fact]
        public void AlleleFrequencyTest()
        {
            var oneKItem = _oneKGenReader.ExtractOneKGenItem(VcfLine1)[0];

            Assert.Equal(oneKItem.AllFreq, "0.4375");
            Assert.Equal(oneKItem.EasFreq, "0.4306");
            Assert.Equal(oneKItem.AmrFreq, "0.4107");
            Assert.Equal(oneKItem.AfrFreq, "0.4788");
            Assert.Equal(oneKItem.EurFreq, "0.4264");
            Assert.Equal(oneKItem.SasFreq, "0.4192");
            Assert.True(oneKItem.AncestralAllele == null);
        }

        [Fact]
        public void MultiAltAlleleTest()
        {
            var sa = new SupplementaryAnnotation();

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(VcfLine2))
			{
				oneKitem.SetSupplementaryAnnotations(sa);
			}

            Assert.Equal(sa.AlleleSpecificAnnotations["G"].OneKgAll, "0.347244");
            Assert.Equal(sa.AlleleSpecificAnnotations["G"].OneKgEas, "0.4812");
            Assert.Equal(sa.AlleleSpecificAnnotations["G"].OneKgAmr, "0.2752");
            Assert.Equal(sa.AlleleSpecificAnnotations["G"].OneKgAfr, "0.323");
            Assert.Equal(sa.AlleleSpecificAnnotations["G"].OneKgEur, "0.2922");
            Assert.Equal(sa.AlleleSpecificAnnotations["G"].OneKgSas, "0.3497");

            Assert.Equal(sa.AlleleSpecificAnnotations["T"].OneKgAll, "0.640974");
            Assert.Equal(sa.AlleleSpecificAnnotations["T"].OneKgEas, "0.5188");
            Assert.Equal(sa.AlleleSpecificAnnotations["T"].OneKgAmr, "0.7205");
            Assert.Equal(sa.AlleleSpecificAnnotations["T"].OneKgAfr, "0.6369");
            Assert.Equal(sa.AlleleSpecificAnnotations["T"].OneKgEur, "0.7078");
            Assert.Equal(sa.AlleleSpecificAnnotations["T"].OneKgSas, "0.6472");
        }

        [Fact]
        public void MultiAltAlleleAncesterTest()
        {
            var sa = new SupplementaryAnnotation();

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(VcfLine2))
			{
				oneKitem.SetSupplementaryAnnotations(sa);
			}


            Assert.Equal(sa.AlleleSpecificAnnotations["G"].AncestralAllele, "g");
            Assert.Equal(sa.AlleleSpecificAnnotations["T"].AncestralAllele, "g");
        }

        [Fact]
        public void RefAlleleMinor()
        {
            var sa       = new SupplementaryAnnotation();

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(VcfLine2))
			{
				oneKitem.SetSupplementaryAnnotations(sa);
			}

            Assert.Equal(sa.IsRefMinorAllele, true);
        }

        [Fact]
        public void RefAlleleMinorDeletion()
        {
            var oneKItem = _oneKGenReader.ExtractOneKGenItem(VcfLine3)[0];
            var sa       = new SupplementaryAnnotation();

	        var additionalItems = new List<ISupplementaryDataItem>()
	        {
		        oneKItem.SetSupplementaryAnnotations(sa)
	        };

            sa.Clear();
            foreach (var item in additionalItems)
            {
                item.SetSupplementaryAnnotations(sa);
            }

            // when only SNVs are considered this should be false
            Assert.False(sa.IsRefMinorAllele);
        }

        [Fact]
        public void RefAlleleMajor()
        {
			var sa       = new SupplementaryAnnotation();

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(VcfLine1))
			{
				oneKitem.SetSupplementaryAnnotations(sa);
			}

            Assert.False(sa.IsRefMinorAllele);
        }

		[Fact]
		public void BadRefMinor2()
		{
			// NIR-1368
			const string vcfLine1 =
				"X	1389061	.	A	C	100	PASS	AC=3235;AF=0.645966;AN=5008;NS=2504;DP=13425;AMR_AF=0.7262;AFR_AF=0.2504;EUR_AF=0.8827;SAS_AF=0.7955;EAS_AF=0.7282;AA=a|||;VT=SNP";
			const string vcfLine2 =
				"X	1389061	.	A	C,T	100	PASS	AC=2120,1771;AF=0.423323,0.353634;AN=5008;NS=2504;DP=13425;AMR_AF=0.4625,0.2997;AFR_AF=0.087,0.5998;EUR_AF=0.6551,0.2306;SAS_AF=0.5859,0.2157;EAS_AF=0.4484,0.3244;AA=a|||;VT=SNP;MULTI_ALLELIC";

			var sa1 = new SupplementaryAnnotation();
			var sa2 = new SupplementaryAnnotation();

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(vcfLine1))
			{
				oneKitem.SetSupplementaryAnnotations(sa1);
			}

			foreach (var oneKitem in _oneKGenReader.ExtractOneKGenItem(vcfLine2))
			{
				oneKitem.SetSupplementaryAnnotations(sa2);
			}


			sa1.MergeAnnotations(sa2);

			Assert.False(sa1.IsRefMinorAllele);

		}

	    [Fact]
	    public void PrioretizingSymbolicAllele4Svs()
	    {
		    const string vcfLine =
			    "X	101155257	rs373174489	GTGCAAAAGCTCTTTAGTTTAATTAGGTCTCAGCTATTTATCTTTGTTCTTAT	G	100	PASS	AC=1723;AF=0.456424;AN=3775;NS=2504;DP=19960;AMR_AF=0.2594;AFR_AF=0.6346;EUR_AF=0.4364;SAS_AF=0.1789;EAS_AF=0.0893;END=101155309;SVTYPE=DEL;CS=DEL_pindel;VT=SV";

			var oneKItems = _oneKGenReader.ExtractOneKGenItem(vcfLine);
			Assert.False(oneKItems[0].IsInterval);

			var sa = new SupplementaryAnnotation();

			var additionalItems = new List<ISupplementaryDataItem>()
			{
				oneKItems[0].SetSupplementaryAnnotations(sa)
			};

			foreach (var oneKitem in additionalItems)
			{
				oneKitem.SetSupplementaryAnnotations(sa);
			}

			Assert.Equal(sa.AlleleSpecificAnnotations["52"].OneKgAll, "0.456424");
			Assert.Equal(sa.AlleleSpecificAnnotations["52"].OneKgEas, "0.0893");
			Assert.Equal(sa.AlleleSpecificAnnotations["52"].OneKgAmr, "0.2594");
			Assert.Equal(sa.AlleleSpecificAnnotations["52"].OneKgAfr, "0.6346");
			Assert.Equal(sa.AlleleSpecificAnnotations["52"].OneKgEur, "0.4364");
			Assert.Equal(sa.AlleleSpecificAnnotations["52"].OneKgSas, "0.1789");

		}

		[Fact(Skip="not in use")]
	    public void ExtractingCnv()
	    {
		    const string vcfLine = "1	713044	esv3584977;esv3584978	C	<CN0>,<CN2>	100	PASS	AC=3,206;AF=0.000599042,0.0411342;AN=5008;CS=DUP_gs;END=755966;NS=2504;SVTYPE=CNV;DP=20698;EAS_AF=0.001,0.0615;AMR_AF=0.0014,0.0259;AFR_AF=0,0.0303;EUR_AF=0.001,0.0417;SAS_AF=0,0.045;VT=SV;EX_TARGET";

			var oneKItems = _oneKGenReader.ExtractOneKGenItem(vcfLine);
			Assert.True(oneKItems[0].IsInterval);

			var firstItem = oneKItems[0].GetSupplementaryInterval();

			Assert.Equal(713045, firstItem.Start);
			Assert.Equal(755966, firstItem.End);
			Assert.Equal("<CN0>", firstItem.AlternateAllele);
			Assert.Equal("CNV", firstItem.VariantType.ToString());
			Assert.Equal("1000 Genomes Project", firstItem.Source);
			Assert.Equal("esv3584977;esv3584978", firstItem.StringValues["Id"]);
			Assert.Equal(0.000599042, firstItem.PopulationFrequencies["OneKgAll"]);
			Assert.Equal(0.001, firstItem.PopulationFrequencies["OneKgEas"]);
			Assert.Equal(0.0014, firstItem.PopulationFrequencies["OneKgAmr"]);
			Assert.Equal(0, firstItem.PopulationFrequencies["OneKgAfr"]);
			Assert.Equal(0.001, firstItem.PopulationFrequencies["OneKgEur"]);
			Assert.Equal(0, firstItem.PopulationFrequencies["OneKgSas"]);

			var secondItem = oneKItems[1].GetSupplementaryInterval();

			Assert.Equal(713045, secondItem.Start);
			Assert.Equal(755966, secondItem.End);
			Assert.Equal("<CN2>", secondItem.AlternateAllele);
			Assert.Equal("CNV", secondItem.VariantType.ToString());
			Assert.Equal("1000 Genomes Project", secondItem.Source);
			Assert.Equal(0.0411342, secondItem.PopulationFrequencies["OneKgAll"]);
			Assert.Equal(0.0615, secondItem.PopulationFrequencies["OneKgEas"]);
			Assert.Equal(0.0259, secondItem.PopulationFrequencies["OneKgAmr"]);
			Assert.Equal(0.0303, secondItem.PopulationFrequencies["OneKgAfr"]);
			Assert.Equal(0.0417, secondItem.PopulationFrequencies["OneKgEur"]);
			Assert.Equal(0.045, secondItem.PopulationFrequencies["OneKgSas"]);

		}
	}
}
