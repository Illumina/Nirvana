using System;
using System.Text;
using Xunit;
using TrimAndUnifyVcfs;

namespace NirvanaUnitTests.FileHandling
{
	public sealed class OneKGenVcfProcessorTests
	{
		#region members

		private readonly OneKGenVcfProcessor _vcfProcessor;

		#endregion

		public OneKGenVcfProcessorTests()
		{
			_vcfProcessor = new OneKGenVcfProcessor(null, null, null, @"Resources/tmpSampleInfo.txt", @"Resources/tmpPopInfo.txt");
			_vcfProcessor.ReadHeader("#CHROM	POS	ID	REF	ALT	QUAL	FILTER	INFO	FORMAT	CHB0001	CHB0002	CHB0003	JPT0001	JPT0002	JPT0003	CEU0001	CEU0002	CEU0003	TSI0001	TSI0002	TSI0003	FIN0001	FIN0002	FIN0003	FIN0004");
		}


		[Fact]
		public void CheckOutHeader()
		{
			var header = _vcfProcessor.BuildSvHeaderString().ToString();
			string expectedHeader = "#Id\tChr\tStart\tEnd\tSvType\tSampleSize\tObservedGains\tObservedLosses\tVariantFreqAll\tCHB_size\tCHB_Freq\tJPT_size\tJPT_Freq\tCEU_size\tCEU_Freq\tTSI_size\tTSI_Freq\tFIN_size\tFIN_Freq\tEAS_size\tEAS_Freq\tEUR_size\tEUR_Freq";
			Assert.Equal(expectedHeader,header);
		}

		[Fact]
		public void ProcessCnvTests()
		{
			string vcfLine =
				"21	14504804	esv3646378;esv3646379	C	<CN0>,<CN2>	100	PASS	END=14530597;SVTYPE=CNV;VT=SV	GT	0|0	0|0	0|0	0|0	1|0	0|0	0|0	1|2	0|2	0|0	0|0	0|0	0|0	0|0	0|0	0|0";
			var vcfFields = vcfLine.Split('\t');
			var observedSb = _vcfProcessor.ParseStructuralVariatVcfLine(vcfFields, true);
			string expectedString = "esv3646378;esv3646379	21	14504805	14530597	CNV	16	1	1	0.1875	3	0	3	0.33333	3	0.66667	3	0	4	0	6	0.16667	10	0.2";
			Assert.Equal(expectedString, observedSb);
		}

		[Fact]
		public void ProcessChrXParCnvTests()
		{
			string vcfLine =
				"X	187604	DUP_gs.X_CNV_X_187604_218561	A	<CN0>,<CN2>	100	PASS	END=218561;SVTYPE=CNV	GT	1|2	0|0	1|1	1|2	0|0	0|0	0|0	0|0	0|0	0|0	0|1	1|0	0|0	2|2	0|2	0|0";
			var vcfFields = vcfLine.Split('\t');
			var observedSb = _vcfProcessor.ParseStructuralVariatVcfLine(vcfFields, true);
			string expectedString = "DUP_gs.X_CNV_X_187604_218561	X	187605	218561	CNV	16	2	3	0.4375	3	0.66667	3	0.33333	3	0	3	0.66667	4	0.5	6	0.5	10	0.4";
			Assert.Equal(expectedString, observedSb);
		}

		[Fact]
		public void ProcessChrXCnvTests()
		{
			string vcfLine =
				"X	154916967	BI_GS_DEL1_B3_P3052_45	G	<CN0>	100	PASS	END=154933972;SVTYPE=DEL;VT=SV	GT	0	1	0|0	0|1	0|0	0	0	1|0	1	1	1|1	1	0	1|0	1	1";
			var vcfFields = vcfLine.Split('\t');
			var observedSb = _vcfProcessor.ParseStructuralVariatVcfLine(vcfFields, true);
			string expectedString = "BI_GS_DEL1_B3_P3052_45	X	154916968	154933972	DEL	16	0	10	0.625	3	0.33333	3	0.33333	3	0.66667	3	1	4	0.75	6	0.33333	10	0.8";
			Assert.Equal(expectedString, observedSb);
		}

		[Fact]
		public void ProcessChrYSegmentalDupCnvTests()
		{
			//cannot used in real sample, since here our samples contain females
			string vcfLine =
				"Y	6128381	GS_SD_M2_Y_6128381_6230094_Y_9650284_9752225	C	<CN1>,<CN3>	100	PASS	END=6230094;NS=1233;SVTYPE=CNV	GT:CN:CNL:CNP:CNQ:GP:GQ	0:2:-1000,-138.78,0,-38.53:-1000,-141.27,0,-41.33:99:0,-141.27,-41.33:99	2:3:-1000,-53.32,0,-17.85:-1000,-55.81,0,-20.64:99:0,-55.81,-20.64:99	0:2:-1000,-71.83,0,-32.5:-1000,-74.32,0,-35.29:99:0,-74.32,-35.29:99	0:2:-1000,-60.96,0,-20.29:-1000,-63.45,0,-23.08:99:0,-63.45,-23.08:99	0:2:-1000,-77.6,0,-31.45:-1000,-80.09,0,-34.24:99:0,-80.09,-34.24:99	0:2:-1000,-76.64,0,-36.23:-1000,-79.13,0,-39.02:99:0,-79.13,-39.02:99	0:2:-1000,-62.12,0,-21.79:-1000,-64.61,0,-24.58:99:0,-64.61,-24.58:99	1:1:-1000,-72.69,0,-23.59:-1000,-75.18,0,-26.38:99:0,-75.18,-26.38:99	0:2:-1000,-40.94,0,-19.2:-1000,-43.44,0,-21.99:99:0,-43.44,-21.99:99	0:2:-1000,-98.43,0,-39.89:-1000,-100.92,0,-42.69:99:0,-100.92,-42.69:99	0:2:-1000,-29.46,0,-23.64:-1000,-31.95,0,-26.43:99:0,-31.95,-26.43:99	0:2:-1000,-73.52,0,-30.86:-1000,-76.02,0,-33.65:99:0,-76.02,-33.65:99	0:2:-1000,-67.19,0,-27.73:-1000,-69.68,0,-30.52:99:0,-69.68,-30.52:99	0:2:-1000,-90.89,0,-28.6:-1000,-93.38,0,-31.39:99:0,-93.38,-31.39:99	0:2:-1000,-75.95,0,-26.7:-1000,-78.44,0,-29.49:99:0,-78.44,-29.49:99	1:1:-1000,-76.16,0,-34.97:-1000,-78.65,0,-37.77:99:0,-78.65,-37.77:99";
			var vcfFields = vcfLine.Split('\t');
			var observedSb = _vcfProcessor.ParseStructuralVariatVcfLine(vcfFields, true);
			string expectedString =
				  "GS_SD_M2_Y_6128381_6230094_Y_9650284_9752225	Y	6128382	6230094	CNV	10	1	1	0.2	2	0.5	1	0	2	0	2	0	3	0.33333	3	0.33333	7	0.14286";
			Assert.Equal(expectedString, observedSb);
		}

		[Fact]
		public void ProcessChrYCnvTests()
		{
			//made-up example
			//cannot used in real sample, since here our samples contain females
			string vcfLine =
				"Y	6128381	Y_6128381_6230094_Y_9650284_9752225	C	<CN2>,<CN3>	100	PASS	END=6230094;NS=1233;SVTYPE=CNV	GT:CN:CNL:CNP:CNQ:GP:GQ	1:2:-1000,-138.78,0,-38.53:-1000,-141.27,0,-41.33:99:0,-141.27,-41.33:99	2:3:-1000,-53.32,0,-17.85:-1000,-55.81,0,-20.64:99:0,-55.81,-20.64:99	1:2:-1000,-71.83,0,-32.5:-1000,-74.32,0,-35.29:99:0,-74.32,-35.29:99	1:2:-1000,-60.96,0,-20.29:-1000,-63.45,0,-23.08:99:0,-63.45,-23.08:99	1:2:-1000,-77.6,0,-31.45:-1000,-80.09,0,-34.24:99:0,-80.09,-34.24:99	1:2:-1000,-76.64,0,-36.23:-1000,-79.13,0,-39.02:99:0,-79.13,-39.02:99	1:2:-1000,-62.12,0,-21.79:-1000,-64.61,0,-24.58:99:0,-64.61,-24.58:99	0:1:-1000,-72.69,0,-23.59:-1000,-75.18,0,-26.38:99:0,-75.18,-26.38:99	1:2:-1000,-40.94,0,-19.2:-1000,-43.44,0,-21.99:99:0,-43.44,-21.99:99	1:2:-1000,-98.43,0,-39.89:-1000,-100.92,0,-42.69:99:0,-100.92,-42.69:99	1:2:-1000,-29.46,0,-23.64:-1000,-31.95,0,-26.43:99:0,-31.95,-26.43:99	1:2:-1000,-73.52,0,-30.86:-1000,-76.02,0,-33.65:99:0,-76.02,-33.65:99	1:2:-1000,-67.19,0,-27.73:-1000,-69.68,0,-30.52:99:0,-69.68,-30.52:99	0:2:-1000,-90.89,0,-28.6:-1000,-93.38,0,-31.39:99:0,-93.38,-31.39:99	1:2:-1000,-75.95,0,-26.7:-1000,-78.44,0,-29.49:99:0,-78.44,-29.49:99	0:1:-1000,-76.16,0,-34.97:-1000,-78.65,0,-37.77:99:0,-78.65,-37.77:99";
			var vcfFields = vcfLine.Split('\t');
			var observedSb = _vcfProcessor.ParseStructuralVariatVcfLine(vcfFields, true);
			string expectedString =
				  "Y_6128381_6230094_Y_9650284_9752225	Y	6128382	6230094	CNV	10	9	0	0.9	2	1	1	1	2	1	2	1	3	0.66667	3	1	7	0.85714";
			Assert.Equal(expectedString, observedSb);
		}
		[Fact]
		public void ProcessInvTests()
		{
			string vcfLine =
				"1	21530509	esv3585441	G	<INV>	100	PASS	CIEND=-307,307;CIPOS=-307,307;CS=INV_delly;END=21531320;NS=2504;SVTYPE=INV;VT=SV	GT	0|0	0|0	0|0	0|0	1|0	0|0	0|0	1|2	0|2	0|0	0|0	0|0	0|0	0|0	0|0	0|0";
			var vcfFields = vcfLine.Split('\t');
			var observedSb = _vcfProcessor.ParseStructuralVariatVcfLine(vcfFields, false);
			string expectedString =
				"esv3585441	1	21530510	21531320	INV	16	0	0	0.1875	3	0	3	0.33333	3	0.66667	3	0	4	0	6	0.16667	10	0.2";
			Assert.Equal(expectedString, observedSb);
		}
	}
}