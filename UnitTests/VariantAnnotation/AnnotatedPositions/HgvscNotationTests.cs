using VariantAnnotation.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
	public sealed class HgvscNotationTests
	{
		//NM_004006.1:c.93G>T
		[Fact]
		public void ToString_substitution()
		{
			var startPosOff = new PositionOffset(93);
			var endPosOff = new PositionOffset(93);
			startPosOff.Value = "93";
			endPosOff.Value = "93";

			var hgvsc = new HgvscNotation("G", "T", "NM_004006.1", GenomicChange.Substitution,  startPosOff, endPosOff, true) ;

			Assert.Equal("NM_004006.1:c.93G>T", hgvsc.ToString());
		}

		//NM_012232.1:c.19del (one nucleotide)
		[Fact]
		public void ToString_deletion_one_base()
		{
			var startPosOff = new PositionOffset(19);
			var endPosOff = new PositionOffset(19);
			startPosOff.Value = "19";
			endPosOff.Value = "19";

			var hgvsc = new HgvscNotation("T", "", "NM_012232.1", GenomicChange.Deletion, startPosOff, endPosOff, true);

			Assert.Equal("NM_012232.1:c.19delT", hgvsc.ToString());
		}

		//NM_012232.1:c.19_21delTGC (multiple nucleotide)
		[Fact]
		public void ToString_deletion_multiple_base()
		{
			var startPosOff = new PositionOffset(19);
			var endPosOff = new PositionOffset(21);
			startPosOff.Value = "19";
			endPosOff.Value = "21";

			var hgvsc = new HgvscNotation("TGC", "", "NM_012232.1", GenomicChange.Deletion, startPosOff, endPosOff, true);

			Assert.Equal("NM_012232.1:c.19_21delTGC", hgvsc.ToString());
		}

		//NM_012232.1:c.7dupT (one base duplication)
		[Fact]
		public void ToString_one_base_duplication()
		{
			var startPosOff = new PositionOffset(7);
			var endPosOff = new PositionOffset(7);
			startPosOff.Value = "7";
			endPosOff.Value = "7";

			var hgvsc = new HgvscNotation("T", "T", "NM_012232.1", GenomicChange.Duplication, startPosOff, endPosOff, true);

			Assert.Equal("NM_012232.1:c.7dupT", hgvsc.ToString());
		}

		//NM_012232.1:c.6_8dupTGC (multi base duplication)
		[Fact]
		public void ToString_multi_base_duplication()
		{
			var startPosOff = new PositionOffset(6);
			var endPosOff = new PositionOffset(8);
			startPosOff.Value = "6";
			endPosOff.Value = "8";

			var hgvsc = new HgvscNotation("TGC", "TGC", "NM_012232.1", GenomicChange.Duplication, startPosOff, endPosOff, true);

			Assert.Equal("NM_012232.1:c.6_8dupTGC", hgvsc.ToString());
		}

		//NM_012232.1:c.5756_5757insAGG (multi base insertion)
		[Fact]
		public void ToString_insertion()
		{
			var startPosOff = new PositionOffset(5756);
			var endPosOff = new PositionOffset(5757);
			startPosOff.Value = "5756";
			endPosOff.Value = "5757";

			var hgvsc = new HgvscNotation("", "AGG", "NM_012232.1", GenomicChange.Insertion, startPosOff, endPosOff, true);

			Assert.Equal("NM_012232.1:c.5756_5757insAGG", hgvsc.ToString());
		}

		//NM_012232.1:c.142_144delinsTGG 
		//[Fact]
		//public void ToString_delins()
		//{
		//	var startPosOff = new PositionOffset(142);
		//	var endPosOff = new PositionOffset(144);
		//	startPosOff.Value = "142";
		//	endPosOff.Value = "144";

		//	var hgvsc = new HgvscNotation("AT", "TGG", "NM_012232.1", GenomicChange.DelIns, startPosOff, endPosOff, true);

		//	Assert.Equal("NM_012232.1:c.142_144delinsTGG", hgvsc.ToString());
		//}


	}
}