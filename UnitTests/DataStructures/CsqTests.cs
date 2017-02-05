using System.Collections.Generic;
using System.Linq;
using UnitTests.Utilities;
using VariantAnnotation.DataStructures;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class CsqTests
    {
        #region  members

        private const string VcfInfoPrefix =
            "AC=1;AF=0.50;AN=2;BaseQRankSum=-3.425;DP=63;Dels=0.00;FS=33.739;HRun=3;HaplotypeScore=25.8062;MQ=60.00;MQ0=0;MQRankSum=-0.141;QD=0.24;ReadPosRankSum=2.555;";

        private const string VcfinfoSuffix = ";SB=-0.01";

        #endregion

        [Fact]
        public void UnwantedTranscriptInsertion()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("chr2_216305895_T_TAA_Ensembl84_pos"),
                "chr2\t216305895\t.\tT\tTAA\t130.00\tPASS\t.");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault();
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(1, altAllele);
        }

        [Fact]
        public void TranscriptInsertion5000()
        {
            var annotatedVariant = DataUtilities.GetVariant(Resources.CacheGRCh37("chr1_51801236_A_AA_Ensembl84_pos"),
                "chr1\t51801235\t.\tCA\tCAA,C\t134.00\tPASS\t.");
            Assert.NotNull(annotatedVariant);

            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(0);
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(7, altAllele);

            altAllele = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(1);
            Assert.NotNull(altAllele);

            AssertUtilities.CheckEnsemblTranscriptCount(8, altAllele);
        }

        [Fact]
        public void CsqParsing()
        {
            const string vcfInfoField = "CSQ=-|CCDS46658.1|CCDS46658.1|Transcript|inframe_deletion|196-198|196-198|66|C/-|TGC/-|||||||YES|1/10||CCDS46658.1|CCDS46658.1:c.196_198delNNN|CCDS46658.1:p.Cys66del||||||CCDS46658.1,-|23784|NM_001136213.1|Transcript|inframe_deletion|248-250|196-198|66|C/-|TGC/-|||||||YES|1/11|||NM_001136213.1:c.196_198delNNN|NP_001129685.1:p.Cys66del||||||NP_001129685.1";

            var csqEntries = new List<CsqEntry>();
            string combinedInfoField = $"{VcfInfoPrefix}{vcfInfoField}{VcfinfoSuffix}";
            CsqCommon.GetCsqEntries(combinedInfoField, csqEntries);

            // expected results
            const int expectedNumEntries = 2;
            Assert.True(expectedNumEntries == csqEntries.Count);

            var expectedFirstEntry = new CsqEntry
            {
                Allele = "-",
                Gene = "23784",
                Feature = "NM_001136213.1",
                FeatureType = "Transcript",
                Consequence = "inframe_deletion",
                ComplementaryDnaPosition = "248-250",
                CdsPosition = "196-198",
                ProteinPosition = "66",
                AminoAcids = "C/-",
                Codons = "TGC/-",
                ExistingVariation = "",
                Symbol = "",
                Sift = "",
                Distance = "",
                Ccds = "",
                MotifName = "",
                MotifPos = "",
                HighInfPos = "",
                MotifScoreChange = "",
                PolyPhen = "",
                Exon = "1/11",
                Intron = "",
                Domains = "",
                EnsemblProteinId = "NP_001129685.1",
                HgvsCodingSequenceName = "NM_001136213.1:c.196_198delNNN",
                HgvsProteinSequenceName = "NP_001129685.1:p.Cys66del",
                Canonical = "YES",
                CellType = ""
            };

            var expectedSecondEntry = new CsqEntry
            {
                Allele = "-",
                Gene = "CCDS46658.1",
                Feature = "CCDS46658.1",
                FeatureType = "Transcript",
                Consequence = "inframe_deletion",
                ComplementaryDnaPosition = "196-198",
                CdsPosition = "196-198",
                ProteinPosition = "66",
                AminoAcids = "C/-",
                Codons = "TGC/-",
                ExistingVariation = "",
                Symbol = "",
                Sift = "",
                Distance = "",
                Ccds = "CCDS46658.1",
                MotifName = "",
                MotifPos = "",
                HighInfPos = "",
                MotifScoreChange = "",
                PolyPhen = "",
                Exon = "1/10",
                Intron = "",
                Domains = "",
                EnsemblProteinId = "CCDS46658.1",
                HgvsCodingSequenceName = "CCDS46658.1:c.196_198delNNN",
                HgvsProteinSequenceName = "CCDS46658.1:p.Cys66del",
                Canonical = "YES",
                CellType = ""
            };

            if (expectedNumEntries == csqEntries.Count)
            {
                var observedFirstEntry = CsqCommon.GetEntry(csqEntries, expectedFirstEntry.Feature,
                    expectedFirstEntry.Allele);

                Assert.NotNull(observedFirstEntry);

                var observedSecondEntry = CsqCommon.GetEntry(csqEntries, expectedSecondEntry.Feature,
                    expectedSecondEntry.Allele);

                Assert.NotNull(observedSecondEntry);

                Assert.Equal(expectedFirstEntry, observedFirstEntry);
                Assert.Equal(expectedSecondEntry, observedSecondEntry);
            }
        }

        [Fact]
        public void HashCode()
        {
            var csqEntry = new CsqEntry { Allele = "AC" };
            const int expectedHashCode = 768942877;
            Assert.Equal(expectedHashCode, csqEntry.GetHashCode());

            const int expectedHashCode2 = 358252778;
            csqEntry.Feature = "ENST00000288135";
            Assert.Equal(expectedHashCode2, csqEntry.GetHashCode());
        }

        [Fact]
        public void StringRepresentation()
        {
            var csqEntry = new CsqEntry { Allele = "AC", Feature = "ENST00000288135", Symbol = "NRAS" };

            const string expectedResult =
                "Allele:                     AC\nFeature:                    ENST00000288135\nSymbol:                     NRAS\n";
            var observedResult = csqEntry.ToString();

            Assert.Equal(expectedResult, observedResult);
        }
    }
}