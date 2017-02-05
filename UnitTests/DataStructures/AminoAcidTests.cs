using UnitTests.Utilities;
using VariantAnnotation.Algorithms;
using VariantAnnotation.DataStructures;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class AminoAcidTests
    {
        [Fact]
        public void NothingPastTer()
        {
            // G|stop_gained&frameshift_variant|HIGH|EGFR|ENSG00000146648|Transcript|ENST00000275493|protein_coding|20/28||ENST00000275493.2:c.2402_2403insG|ENSP00000275493.2:p.Tyr801Ter|2579-2580|2402-2403|801|Y/*|tat/taGt|||1|HGNC|3236|YES|CCDS5514.1|ENSP00000275493|||Prints_domain:PR00109&Superfamily_domains:SSF56112&PIRSF_domain:PIRSF000619&SMART_domains:SM00219&Pfam_domain:PF07714&Gene3D:1.10.510.10&hmmpanther:PTHR24416:SF91&hmmpanther:PTHR24416&PROSITE_profiles:PS50011|||||
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000275493_chr7_Ensembl84"),
                "chr7\t55249104\t.\tA\tAG\t1000\tPASS\t.", "ENST00000275493", "G");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("Y/*", transcriptAllele.AminoAcids);
        }

        [Fact]
        public void SomethingPastTer()
        {
            // AGGTGGG|stop_gained&frameshift_variant|HIGH|KIT|ENSG00000157404|Transcript|ENST00000412167|protein_coding|8/21||ENST00000412167.2:c.1252_1253insAGGTGGG|ENSP00000390987.2:p.Tyr418Ter|1349-1350|1252-1253|418|Y/*VGX|tac/tAGGTGGGac|||1|HGNC|6342||CCDS47058.1|ENSP00000390987|||hmmpanther:PTHR24416:SF46&hmmpanther:PTHR24416&Pfam_domain:PF13927&PIRSF_domain:PIRSF000615&PIRSF_domain:PIRSF500951|||||
            var transcriptAllele = DataUtilities.GetTranscript(Resources.CacheGRCh37("ENST00000412167_chr4_Ensembl84"),
                "chr4\t55589770\t.\tT\tTAGGTGGG\t1000\tPASS\t.", "ENST00000412167", "AGGTGGG");
            Assert.NotNull(transcriptAllele);
            Assert.Equal("Y/*VGX", transcriptAllele.AminoAcids);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public void Shift3PrimeSTM()
        {
            // given a STM/- deletion in R[STM]STMP, we want to move to: RSTM[STM]P
            const string transcriptPeptides = "RSTMSTMP";
            var hn = new HgvsProteinNomenclature.HgvsNotation("STM", null, "bob", 2, 4) { Type = ProteinChange.Deletion };

            AminoAcids.Rotate3Prime(hn, transcriptPeptides);

            Assert.Equal(5, hn.Start);
            Assert.Equal(7, hn.End);
        }

        [Fact]
        public void Shift3PrimeS()
        {
            // given a S/- deletion in RS[S]SSSS, we want to move to: RSSSSS[S]
            const string transcriptPeptides = "RSSSSSS";
            var hn = new HgvsProteinNomenclature.HgvsNotation("S", null, "bob", 3, 3) { Type = ProteinChange.Deletion };

            AminoAcids.Rotate3Prime(hn, transcriptPeptides);

            Assert.Equal(7, hn.Start);
            Assert.Equal(7, hn.End);
        }

        [Fact]
        // ReSharper disable once InconsistentNaming
        public void Shift3PrimeSS()
        {
            // given a SS/- deletion in RS[SS]SSS, we want to move to: RSSSS[SS]
            const string transcriptPeptides = "RSSSSSS";
            var hn = new HgvsProteinNomenclature.HgvsNotation("SS", null, "bob", 3, 4) { Type = ProteinChange.Deletion };

            AminoAcids.Rotate3Prime(hn, transcriptPeptides);

            Assert.Equal(6, hn.Start);
            Assert.Equal(7, hn.End);
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