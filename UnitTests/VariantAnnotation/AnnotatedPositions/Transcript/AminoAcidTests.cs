using System;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.TranscriptAnnotation;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class AminoAcidTests
    {
        private readonly AminoAcids _standardAminoAcids = new(false);
        private readonly AminoAcids _mitoAminoAcids     = new(true);

        [Fact]
        public void AddUnknownAminoAcid_ExpectedResults()
        {
            const string aminoAcids = "MACGYIL";
            Assert.Equal(aminoAcids + 'X', AminoAcids.AddUnknownAminoAcid(aminoAcids));
        }

        [Fact]
        public void AddUnknownAminoAcid_SameIfStopCodon()
        {
            const string aminoAcids = "*";
            Assert.Equal(aminoAcids, AminoAcids.AddUnknownAminoAcid(aminoAcids));
        }

        [Fact]
        public void Translate_ExpectedResults()
        {
            SequenceChange aa = _standardAminoAcids.Translate("TTC", "CTC");
            Assert.Equal("F", aa.Reference);
            Assert.Equal("L", aa.Alternate);
        }

        [Fact]
        public void Translate_NullOrEmptyInput_ReturnEmpty()
        {
            SequenceChange aa = _standardAminoAcids.Translate(null, null);
            Assert.Equal("", aa.Reference);
            Assert.Equal("", aa.Alternate);

            aa = _standardAminoAcids.Translate("", "");
            Assert.Equal("", aa.Reference);
            Assert.Equal("", aa.Alternate);
        }

        [Fact]
        public void Translate_NsInInput_ReturnEmpty()
        {
            SequenceChange aa = _standardAminoAcids.Translate("ANA", "AAA");
            Assert.Equal("", aa.Reference);
            Assert.Equal("", aa.Alternate);

            aa = _standardAminoAcids.Translate("AAA", "ANA");
            Assert.Equal("", aa.Reference);
            Assert.Equal("", aa.Alternate);
        }

        [Fact]
        public void ConvertAminoAcidToAbbreviation_ThrowException()
        {
            Assert.Throws<NotSupportedException>(delegate { AminoAcids.ConvertAminoAcidToAbbreviation('a'); });
        }

        [Theory]
        [ClassData(typeof(StandardGeneticCodeData))]
        public void ConvertTripletToAminoAcid_StandardGeneticCode(char expectedResult, string[] triplets)
        {
            foreach (string triplet in triplets)
            {
                char actualResult = _standardAminoAcids.ConvertTripletToAminoAcid(triplet);
                Assert.Equal(expectedResult, actualResult);
            }
        }

        [Theory]
        [ClassData(typeof(VertebrateMitochondrialCodeData))]
        public void ConvertTripletToAminoAcid_VertebrateMitochondrialCode(char expectedResult, string[] triplets)
        {
            foreach (string triplet in triplets)
            {
                char actualResult = _mitoAminoAcids.ConvertTripletToAminoAcid(triplet);
                Assert.Equal(expectedResult, actualResult);
            }
        }

        [Fact]
        public void GetAbbreviations_ExpectedResults()
        {
            Assert.Equal("AspTyrCys", AminoAcids.GetAbbreviations("DYC"));
        }

        [Fact]
        public void GetAbbreviations_SingleAA_ExpectedResults()
        {
            Assert.Equal("Tyr", AminoAcids.GetAbbreviations("Y"));
        }

        [Fact]
        public void GetAbbreviations_NullOrEmptyInput_ReturnEmpty()
        {
            Assert.Equal("", AminoAcids.GetAbbreviations(null));
            Assert.Equal("", AminoAcids.GetAbbreviations(""));
        }

        [Fact]
        public void TranslateBases_ExpectedResults()
        {
            const string expectedResult = "RAD";
            string       actualResult   = _standardAminoAcids.TranslateBases("CGCGCAGAT", true);
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void TranslateBases_NullInput_ReturnNull()
        {
            Assert.Null(_standardAminoAcids.TranslateBases(null, true));
        }

        private sealed class StandardGeneticCodeData : TheoryData<char, string[]>
        {
            public StandardGeneticCodeData()
            {
                Add('A', new[] {"GCT", "GCC", "GCA", "GCG"});
                Add('R', new[] {"CGT", "CGC", "CGA", "CGG", "AGA", "AGG"});
                Add('N', new[] {"AAT", "AAC"});
                Add('D', new[] {"GAT", "GAC"});
                Add('C', new[] {"TGT", "TGC"});
                Add('Q', new[] {"CAA", "CAG"});
                Add('E', new[] {"GAA", "GAG"});
                Add('G', new[] {"GGT", "GGC", "GGA", "GGG"});
                Add('H', new[] {"CAT", "CAC"});
                Add('I', new[] {"ATT", "ATC", "ATA"});
                Add('L', new[] {"CTT", "CTC", "CTA", "CTG", "TTA", "TTG"});
                Add('K', new[] {"AAA", "AAG"});
                Add('M', new[] {"ATG"});
                Add('F', new[] {"TTT", "TTC"});
                Add('P', new[] {"CCT", "CCC", "CCA", "CCG"});
                Add('S', new[] {"TCT", "TCC", "TCA", "TCG", "AGT", "AGC"});
                Add('T', new[] {"ACT", "ACC", "ACA", "ACG"});
                Add('W', new[] {"TGG"});
                Add('Y', new[] {"TAT", "TAC"});
                Add('V', new[] {"GTT", "GTC", "GTA", "GTG"});
                Add('*', new[] {"TAA", "TGA", "TAG"});
            }
        }

        private sealed class VertebrateMitochondrialCodeData : TheoryData<char, string[]>
        {
            public VertebrateMitochondrialCodeData()
            {
                Add('A', new[] {"GCT", "GCC", "GCA", "GCG"});
                Add('R', new[] {"CGT", "CGC", "CGA", "CGG"});
                Add('N', new[] {"AAT", "AAC"});
                Add('D', new[] {"GAT", "GAC"});
                Add('C', new[] {"TGT", "TGC"});
                Add('Q', new[] {"CAA", "CAG"});
                Add('E', new[] {"GAA", "GAG"});
                Add('G', new[] {"GGT", "GGC", "GGA", "GGG"});
                Add('H', new[] {"CAT", "CAC"});
                Add('I', new[] {"ATT", "ATC"});
                Add('L', new[] {"CTT", "CTC", "CTA", "CTG", "TTA", "TTG"});
                Add('K', new[] {"AAA", "AAG"});
                Add('M', new[] {"ATG", "ATA"});
                Add('F', new[] {"TTT", "TTC"});
                Add('P', new[] {"CCT", "CCC", "CCA", "CCG"});
                Add('S', new[] {"TCT", "TCC", "TCA", "TCG", "AGT", "AGC"});
                Add('T', new[] {"ACT", "ACC", "ACA", "ACG"});
                Add('W', new[] {"TGG", "TGA"});
                Add('Y', new[] {"TAT", "TAC"});
                Add('V', new[] {"GTT", "GTC", "GTA", "GTG"});
                Add('*', new[] {"TAA", "TAG", "AGA", "AGG"});
            }
        }
    }
}