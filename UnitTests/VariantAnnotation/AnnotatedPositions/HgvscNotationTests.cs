using VariantAnnotation.AnnotatedPositions;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class HgvscNotationTests
    {
        // NM_004006.1:c.93G>T
        [Fact]
        public void ToString_substitution()
        {
            var startPosOff = new PositionOffset(93, 0, "93", false);
            var endPosOff   = new PositionOffset(93, 0, "93", false);

            var hgvsc = new HgvscNotation("G", "T", "NM_004006.1", GenomicChange.Substitution, startPosOff, endPosOff, true);

            Assert.Equal("NM_004006.1:c.93G>T", hgvsc.ToString());
        }

        // NM_012232.1:c.19del (one nucleotide)
        [Fact]
        public void ToString_deletion_one_base()
        {
            var startPosOff = new PositionOffset(19, 0, "19", false);
            var endPosOff   = new PositionOffset(19, 0, "19", false);

            var hgvsc = new HgvscNotation("T", "", "NM_012232.1", GenomicChange.Deletion, startPosOff, endPosOff, true);

            Assert.Equal("NM_012232.1:c.19delT", hgvsc.ToString());
        }

        // NM_012232.1:c.19_21delTGC (multiple nucleotide)
        [Fact]
        public void ToString_deletion_multiple_base()
        {
            var startPosOff = new PositionOffset(19, 0, "19", false);
            var endPosOff   = new PositionOffset(21, 0, "21", false);

            var hgvsc = new HgvscNotation("TGC", "", "NM_012232.1", GenomicChange.Deletion, startPosOff, endPosOff, true);

            Assert.Equal("NM_012232.1:c.19_21delTGC", hgvsc.ToString());
        }

        // NM_012232.1:c.7dupT (one base duplication)
        [Fact]
        public void ToString_one_base_duplication()
        {
            var startPosOff = new PositionOffset(7, 0, "7", false);
            var endPosOff   = new PositionOffset(7, 0, "7", false);

            var hgvsc = new HgvscNotation("T", "T", "NM_012232.1", GenomicChange.Duplication, startPosOff, endPosOff, true);

            Assert.Equal("NM_012232.1:c.7dupT", hgvsc.ToString());
        }

        // NM_012232.1:c.6_8dupTGC (multi base duplication)
        [Fact]
        public void ToString_multi_base_duplication()
        {
            var startPosOff = new PositionOffset(6, 0, "6", false);
            var endPosOff   = new PositionOffset(8, 0, "8", false);

            var hgvsc = new HgvscNotation("TGC", "TGC", "NM_012232.1", GenomicChange.Duplication, startPosOff, endPosOff, true);

            Assert.Equal("NM_012232.1:c.6_8dupTGC", hgvsc.ToString());
        }

        // NM_012232.1:c.5756_5757insAGG (multi base insertion)
        [Fact]
        public void ToString_insertion()
        {
            var startPosOff = new PositionOffset(5756, 0, "5756", false);
            var endPosOff   = new PositionOffset(5757, 0, "5757", false);

            var hgvsc = new HgvscNotation("", "AGG", "NM_012232.1", GenomicChange.Insertion, startPosOff, endPosOff, true);

            Assert.Equal("NM_012232.1:c.5756_5757insAGG", hgvsc.ToString());
        }
    }
}