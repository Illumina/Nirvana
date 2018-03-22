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
            const string aminoAcids = "*";
            Assert.Equal(aminoAcids, AminoAcids.AddUnknownAminoAcid(aminoAcids));
        }

        [Fact]
        public void AddUnknownAminoAcid_incomplete_peptideSeq()
        {
            const string aminoAcids = "MACGYIL";
            Assert.Equal(aminoAcids + 'X', AminoAcids.AddUnknownAminoAcid(aminoAcids));
        }

        [Fact]
        public void Assign_null_or_empty_input()
        {
            var aminoAcids = new AminoAcids(true);

            // null
            var aa = aminoAcids.Translate(null, null);
            Assert.Equal("", aa.Reference);
            Assert.Equal("", aa.Alternate);

            // empty
            aa = aminoAcids.Translate("", "");
            Assert.Equal("", aa.Reference);
            Assert.Equal("", aa.Alternate);
        }

        [Fact]
        public void Assign_codons_with_N()
        {
            var aminoAcids = new AminoAcids(true);

            // referenceCodons with "N"
            var aa = aminoAcids.Translate("ANA", "AAA");
            Assert.Equal("", aa.Reference);
            Assert.Equal("", aa.Alternate);

            // alternateCodons with "N"
            aa = aminoAcids.Translate("AAA", "ANA");
            Assert.Equal("", aa.Reference);
            Assert.Equal("", aa.Alternate);
        }

        [Fact]
        public void Assign_translate()
        {
            var aminoAcids = new AminoAcids(false);
            var aa = aminoAcids.Translate("TTC", "CTC");
            Assert.Equal("F", aa.Reference);
            Assert.Equal("L", aa.Alternate);
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
            Assert.Equal("", AminoAcids.GetAbbreviations(null));
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
            Assert.Null(aminoAcids.TranslateBases(null, true));

        }
    }
}