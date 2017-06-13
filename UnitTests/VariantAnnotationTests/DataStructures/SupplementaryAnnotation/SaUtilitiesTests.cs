using System;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using Xunit;

namespace UnitTests.VariantAnnotationTests.DataStructures.SupplementaryAnnotation
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
		[InlineData(100, "", "ATG", 100, "", "iATG")]
		[InlineData(100, "CTA", "ATG", 100, "CTA", "ATG")]
		[InlineData(100, "CTA", "AT", 100, "CTA", "3AT")]
		[InlineData(100, "CTA", "CGA", 101, "T", "G")]
		[InlineData(100, "", "N", 100, "", "iN")]
		[InlineData(100, "GTC", "GTC", 100, "GTC", "GTC")]
		[InlineData(100, "G", "G", 100, "G", "G")]
		public void GetReducedAllelesTests(int start, string refAllele, string altallele, int outStart, string outRef, string outAlt)
		{
			var output = Tuple.Create(outStart, outRef, outAlt);
			Assert.Equal(output, SupplementaryAnnotationUtilities.GetReducedAlleles(start,refAllele,altallele));
		}

		[Theory]
		[InlineData("input string one, input string two.", "input_string_one\\x2c_input_string_two.")]
		[InlineData("conflicting interpretations of pathogenicity, not provided", "conflicting_interpretations_of_pathogenicity\\x2c_not_provided")]
		public void GetVcfInfoStringTests(string input, string expected)
		{
			Assert.Equal(expected, SupplementaryAnnotationUtilities.ConvertToVcfInfoString(input));
		}
	}
}