using System;
using VariantAnnotation.AnnotatedPositions.Transcript;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
	public sealed class AminoAcidTests
	{
		[Fact]
		public void AddUnknownAminoAcid_identity_seq()
		{
			var aminoAcids = "*";
			Assert.Equal(aminoAcids, AminoAcids.AddUnknownAminoAcid(aminoAcids));
		}

		[Fact]
		public void AddUnknownAminoAcid_incomplete_peptideSeq()
		{
			var aminoAcids = "MACGYIL";
			Assert.Equal(aminoAcids+'X', AminoAcids.AddUnknownAminoAcid(aminoAcids));
		}

	    [Fact]
	    public void Assign_null_or_empty_input()
	    {
            var aminoAcids = new AminoAcids(true);
            // null
            aminoAcids.Assign(null, null, out string referenceAminoAcids, out string alternateAminoAcids);
            Assert.Null(referenceAminoAcids);
	        Assert.Null(alternateAminoAcids);
            // empty
	        aminoAcids.Assign("", "", out referenceAminoAcids, out alternateAminoAcids);
	        Assert.Null(referenceAminoAcids);
	        Assert.Null(alternateAminoAcids);
        }

	    [Fact]
	    public void Assign_codons_with_N()
	    {
	        var aminoAcids = new AminoAcids(true);
	        // referenceCodons with "N"
            aminoAcids.Assign("ANA", "AAA", out string referenceAminoAcids, out string alternateAminoAcids);
	        Assert.Null(referenceAminoAcids);
	        Assert.Null(alternateAminoAcids);
	        // alternateCodons with "N"
            aminoAcids.Assign("AAA", "ANA", out referenceAminoAcids, out alternateAminoAcids);
	        Assert.Null(referenceAminoAcids);
	        Assert.Null(alternateAminoAcids);
	    }

	    [Fact]
	    public void Assign_translate()
	    {
	        var aminoAcids = new AminoAcids(false);
	        aminoAcids.Assign("TTC", "CTC", out string referenceAminoAcids, out string alternateAminoAcids);
	        Assert.Equal("F", referenceAminoAcids);
	        Assert.Equal("L", alternateAminoAcids);
	    }

	    [Fact]
	    public void ConvertAminoAcidToAbbreviation_not_support()
	    {
	        Assert.Throws<NotSupportedException>(delegate
	        {
	            AminoAcids.ConvertAminoAcidToAbbreviation('a');
	        });
	    }

	    [Fact]
	    public void ConvertTripletToAminoAcid_mitochondrial_codon()
	    {
            var aminoAcids = new AminoAcids(true);
            Assert.Equal('W', aminoAcids.ConvertTripletToAminoAcid("TGA"));
	    }

	    [Fact]
	    public void GetAbbreviations_null_or_empty_input()
	    {
            // null
            Assert.Equal("",AminoAcids.GetAbbreviations(null));
            // empty
	        Assert.Equal("", AminoAcids.GetAbbreviations(""));
	    }

	    [Fact]
	    public void GetAbbreviations_string_input()
	    {
	        Assert.Equal("AspTyrCys", AminoAcids.GetAbbreviations("DYC"));
	    }

	    [Fact]
	    public void TranslateBases_nulls_input()
	    {
	        var aminoAcids = new AminoAcids(true);
            // null
            Assert.Null(aminoAcids.TranslateBases(null,true));

	    }
    }
}