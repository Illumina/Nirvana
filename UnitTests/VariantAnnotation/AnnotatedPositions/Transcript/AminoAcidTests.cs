using VariantAnnotation.AnnotatedPositions.AminoAcids;
using VariantAnnotation.Interface.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class AminoAcidTests
    {
        [Fact]
        public void AddUnknownAminoAcid_ExpectedResults()
        {
            const string aminoAcids = "MACGYIL";
            Assert.Equal(aminoAcids + 'X', AminoAcid.AddUnknownAminoAcid(aminoAcids));
        }

        [Fact]
        public void AddUnknownAminoAcid_SameIfStopCodon()
        {
            const string aminoAcids = "*";
            Assert.Equal(aminoAcids, AminoAcid.AddUnknownAminoAcid(aminoAcids));
        }

        [Fact]
        public void Translate_ExpectedResults()
        {
            (string actualRefAa, string actualAltAa) = AminoAcidCommon.StandardAminoAcids.Translate("TTC", "CTC", null, 1);
            Assert.Equal("F", actualRefAa);
            Assert.Equal("L", actualAltAa);
        }

        [Fact]
        public void Translate_NullOrEmptyInput_ReturnEmpty()
        {
            (string actualRefAa, string actualAltAa) = AminoAcidCommon.StandardAminoAcids.Translate(null, null, null, 1);
            Assert.Equal("", actualRefAa);
            Assert.Equal("", actualAltAa);

            (actualRefAa, actualAltAa) = AminoAcidCommon.StandardAminoAcids.Translate("", "", null, 1);
            Assert.Equal("", actualRefAa);
            Assert.Equal("", actualAltAa);
        }

        [Fact]
        public void Translate_NsInInput_ReturnEmpty()
        {
            (string actualRefAa, string actualAltAa) = AminoAcidCommon.StandardAminoAcids.Translate("ANA", "AAA", null, 1);
            Assert.Equal("", actualRefAa);
            Assert.Equal("", actualAltAa);

            (actualRefAa, actualAltAa) = AminoAcidCommon.StandardAminoAcids.Translate("AAA", "ANA", null, 1);
            Assert.Equal("", actualRefAa);
            Assert.Equal("", actualAltAa);
        }

        [Theory]
        [ClassData(typeof(StandardGeneticCodeData))]
        public void ConvertTripletToAminoAcid_StandardGeneticCode(char expected, string[] triplets)
        {
            string expectedResult = expected.ToString();
            foreach (string triplet in triplets)
            {
                string actualResult = AminoAcidCommon.StandardAminoAcids.TranslateBases(triplet, null, 1, false);
                Assert.Equal(expectedResult, actualResult);
            }
        }

        [Theory]
        [ClassData(typeof(VertebrateMitochondrialCodeData))]
        public void ConvertTripletToAminoAcid_VertebrateMitochondrialCode(char expected, string[] triplets)
        {
            string expectedResult = expected.ToString();
            foreach (string triplet in triplets)
            {
                string actualResult = AminoAcidCommon.MitochondrialAminoAcids.TranslateBases(triplet, null, 1, false);
                Assert.Equal(expectedResult, actualResult);
            }
        }
        
        [Fact]
        public void TranslateBases_ExpectedResults()
        {
            const string expectedResult = "RAD";
            string       actualResult   = AminoAcidCommon.StandardAminoAcids.TranslateBases("CGCGCAGAT", null, 1, true);
            Assert.Equal(expectedResult, actualResult);
        }
        
        [Fact]
        public void TranslateBases2_NoAminoAcidEdits_NoChanges()
        {
            const string expectedResult = "RAD";
            string       actualResult   = AminoAcidCommon.StandardAminoAcids.TranslateBases("CGCGCAGAT", null, 1, true);
            Assert.Equal(expectedResult, actualResult);
        }
        
        [Fact]
        public void TranslateBases2_AminoAcidEditBefore_NoChanges()
        {
            const string expectedResult = "AD";
            
            AminoAcidEdit[] aaEdits =
            {
                new AminoAcidEdit(1, 'M')
            };
            
            string actualResult = AminoAcidCommon.StandardAminoAcids.TranslateBases("GCAGAT", aaEdits, 2, true);
            Assert.Equal(expectedResult, actualResult);
        }
        
        [Fact]
        public void TranslateBases2_AminoAcidEditAfter_NoChanges()
        {
            const string expectedResult = "AD";
            
            AminoAcidEdit[] aaEdits =
            {
                new AminoAcidEdit(4, 'M')
            };
            
            string actualResult = AminoAcidCommon.StandardAminoAcids.TranslateBases("GCAGAT", aaEdits, 2, true);
            Assert.Equal(expectedResult, actualResult);
        }
        
        [Fact]
        public void TranslateBases2_AminoAcidEdits_OverrideTwoAminoAcids()
        {
            const string expectedResult = "MAT";

            AminoAcidEdit[] aaEdits =
            {
                new AminoAcidEdit(1, 'M'),
                new AminoAcidEdit(3, 'T')
            };

            string actualResult = AminoAcidCommon.StandardAminoAcids.TranslateBases("CGCGCAGAT", aaEdits, 1, true);
            Assert.Equal(expectedResult, actualResult);
        }
        
        [Fact]
        public void TranslateBases2_AminoAcidEdits_WithOffset_OverrideTwoAminoAcids()
        {
            const string expectedResult = "NI";

            AminoAcidEdit[] aaEdits =
            {
                new AminoAcidEdit(2, 'N'),
                new AminoAcidEdit(3, 'I')
            };

            string actualResult = AminoAcidCommon.StandardAminoAcids.TranslateBases("GCAGAT", aaEdits, 2, true);
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("CGCGCAGA")]
        [InlineData("CGCGCAG")]
        public void TranslateBases_Incomplete_ExpectedResults(string cdsBases)
        {
            const string expectedResult = "RAX";
            string       actualResult   = AminoAcidCommon.StandardAminoAcids.TranslateBases(cdsBases, null, 1, false);
            Assert.Equal(expectedResult, actualResult);
        }

        [Theory]
        [InlineData("CGCGCAGA")]
        [InlineData("CGCGCAG")]
        public void TranslateBases_Incomplete_ForceNonTriplet_ExpectedResults(string cdsBases)
        {
            const string expectedResult = "RA";
            string       actualResult   = AminoAcidCommon.StandardAminoAcids.TranslateBases(cdsBases, null, 1, true);
            Assert.Equal(expectedResult, actualResult);
        }

        [Fact]
        public void TranslateBases_NullInput_ReturnNull()
        {
            Assert.Null(AminoAcidCommon.StandardAminoAcids.TranslateBases(null, null, 1, true));
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