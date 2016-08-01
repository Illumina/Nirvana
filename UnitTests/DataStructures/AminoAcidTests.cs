using UnitTests.Utilities;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("Chromosome 1 collection")]
    public sealed class AminoAcidTests
    {
        [Fact]
        public void NothingPastTer()
        {
            // G|stop_gained&frameshift_variant|HIGH|EGFR|ENSG00000146648|Transcript|ENST00000275493|protein_coding|20/28||ENST00000275493.2:c.2402_2403insG|ENSP00000275493.2:p.Tyr801Ter|2579-2580|2402-2403|801|Y/*|tat/taGt|||1|HGNC|3236|YES|CCDS5514.1|ENSP00000275493|||Prints_domain:PR00109&Superfamily_domains:SSF56112&PIRSF_domain:PIRSF000619&SMART_domains:SM00219&Pfam_domain:PF07714&Gene3D:1.10.510.10&hmmpanther:PTHR24416:SF91&hmmpanther:PTHR24416&PROSITE_profiles:PS50011|||||
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000275493_chr7_Ensembl84.ndb",
                "chr7\t55249104\t.\tA\tAG\t1000\tPASS\t.", "ENST00000275493", "G");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("Y/*", transcriptAllele.AminoAcids);
        }

        [Fact]
        public void SomethingPastTer()
        {
            // AGGTGGG|stop_gained&frameshift_variant|HIGH|KIT|ENSG00000157404|Transcript|ENST00000412167|protein_coding|8/21||ENST00000412167.2:c.1252_1253insAGGTGGG|ENSP00000390987.2:p.Tyr418Ter|1349-1350|1252-1253|418|Y/*VGX|tac/tAGGTGGGac|||1|HGNC|6342||CCDS47058.1|ENSP00000390987|||hmmpanther:PTHR24416:SF46&hmmpanther:PTHR24416&Pfam_domain:PF13927&PIRSF_domain:PIRSF000615&PIRSF_domain:PIRSF500951|||||
            var transcriptAllele = DataUtilities.GetTranscript("ENST00000412167_chr4_Ensembl84.ndb",
                "chr4\t55589770\t.\tT\tTAGGTGGG\t1000\tPASS\t.", "ENST00000412167", "AGGTGGG");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("Y/*VGX", transcriptAllele.AminoAcids);
        }

        [Fact]
        public void TrimPrefix()
        {
            // RSS/R
            var hn = new HgvsProteinNomenclature.HgvsNotation("RSS", "R", "bob", 100, 102) { Type = ProteinChange.Deletion };

            AminoAcids.RemovePrefixAndSuffix(hn);

            const string expectedReference = "SS";
            Assert.Equal(expectedReference, hn.ReferenceAminoAcids);

            const string expectedAlternate = null;
            Assert.Equal(expectedAlternate, hn.AlternateAminoAcids);

            const int expectedStart = 101;
            Assert.Equal(expectedStart, hn.Start);

            const int expectedEnd = 102;
            Assert.Equal(expectedEnd, hn.End);
        }

        [Fact]
        public void TrimBothPrefixAndSuffix()
        {
            // RT/RMLMLT
            var hn = new HgvsProteinNomenclature.HgvsNotation("RT", "RMLMLT", "bob", 100, 101) { Type = ProteinChange.Insertion };

            AminoAcids.RemovePrefixAndSuffix(hn);

            const string expectedReference = null;
            Assert.Equal(expectedReference, hn.ReferenceAminoAcids);

            const string expectedAlternate = "MLML";
            Assert.Equal(expectedAlternate, hn.AlternateAminoAcids);

            const int expectedStart = 101;
            Assert.Equal(expectedStart, hn.Start);

            const int expectedEnd = 100;
            Assert.Equal(expectedEnd, hn.End);
        }
    }
}