using System;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.DataStructures
{
	public sealed class SaUtilitiesTests
	{
		[Theory]
		[InlineData(".", "", true)]
		[InlineData("A",null,true)]
		[InlineData("A", "NNNN", true)]
		[InlineData("ACT","ACTGG",true)]
		[InlineData("ACT", "ATTGG", false)]
		[InlineData("ACTGGCTTGGG","ACT",true)]
		[InlineData("ACTGGCTTGGG", "ATT", false)]
		public void ValidateRefAlleleTests(string refAllele, string refBases, bool result)
		{
			Assert.Equal(result,SupplementaryAnnotationUtilities.ValidateRefAllele(refAllele,refBases));
		}

		[Theory]
		[InlineData(null, null)]
		[InlineData("5","-")]
		[InlineData("iAGC","AGC")]
		[InlineData("A","A")]
		[InlineData("","-")]
		[InlineData("5AGC","AGC")]
		[InlineData("i","-")]
		public void ReverSAReducedAlleleTests(string saAltAllele, string outAllele)
		{
			Assert.Equal(outAllele,SupplementaryAnnotationUtilities.ReverseSaReducedAllele(saAltAllele));
		}

		[Theory]
		[InlineData(100, "ATG", "", 100, "ATG", "3")]
		public void GetReducedAllelesTests(int start, string refAllele, string altallele, int outStart, string outRef, string outAlt)
		{
			var output = Tuple.Create(outStart, outRef, outAlt);
			Assert.Equal(output, SupplementaryAnnotationUtilities.GetReducedAlleles(start,refAllele,altallele));
		}


	}
}