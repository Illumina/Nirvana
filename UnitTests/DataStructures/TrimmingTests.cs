using UnitTests.Fixtures;
using UnitTests.Utilities;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    [Collection("ChromosomeRenamer")]
    public sealed class TrimmingTests
    {
        private readonly ChromosomeRenamer _renamer;

        /// <summary>
        /// constructor
        /// </summary>
        public TrimmingTests(ChromosomeRenamerFixture fixture)
        {
            _renamer = fixture.Renamer;
        }

        [Fact]
        public void Deletion()
        {
            var variant = VcfUtilities.GetVariant("chr1\t99\t.\tGTAGGT\tG\t.\t.\t.", _renamer);

            variant.TrimAlternateAlleles();
            var altAllele = variant.AlternateAlleles[0];

            Assert.Equal(100, altAllele.Start);
            Assert.Equal(104, altAllele.End);
            Assert.Equal("TAGGT", altAllele.ReferenceAllele);
            Assert.Equal("", altAllele.AlternateAllele);
        }

        [Fact]
        public void Insertion()
        {
            var variant = VcfUtilities.GetVariant("chr1\t100\t.\tA\tACT\t.\t.\t.", _renamer);

            variant.TrimAlternateAlleles();
            var altAllele = variant.AlternateAlleles[0];

            Assert.Equal(101, altAllele.Start);
            Assert.Equal(100, altAllele.End);
            Assert.Equal("", altAllele.ReferenceAllele);
            Assert.Equal("CT", altAllele.AlternateAllele);
        }

        [Fact]
        public void Mnv()
        {
            var variant = VcfUtilities.GetVariant("chr1\t100\t.\tTAGGT\tACTTA\t.\t.\t.", _renamer);

            variant.TrimAlternateAlleles();
            var altAllele = variant.AlternateAlleles[0];

            Assert.Equal(100, altAllele.Start);
            Assert.Equal(104, altAllele.End);
            Assert.Equal("TAGGT", altAllele.ReferenceAllele);
            Assert.Equal("ACTTA", altAllele.AlternateAllele);
        }

        [Fact]
        public void MultipleAlleleTrimming()
        {
            const string vcfLine = "17\t2888571\t.\tATGT\tAT,ATG\t24\tLowGQX\tCIGAR=1M2D1M,3M1D;RU=TG,T;REFREP=1,13;IDREP=0,12;CSQT=-|RAP1GAP2|ENST00000254695|intron_variant&feature_truncation,ATG|RAP1GAP2|ENST00000254695|intron_variant\tGT:GQ:GQX:DPI:AD\t1/2:636:596:26:1,14,9";

            var variant = VcfUtilities.GetVariant(vcfLine, _renamer);

            foreach (var altAllele in variant.AlternateAlleles)
            {
                Assert.Equal("", altAllele.AlternateAllele);
            }
        }

        [Fact]
        public void Snv()
        {
            var variant = VcfUtilities.GetVariant("chr1\t100\t.\tA\tG\t.\t.\t.", _renamer);

            variant.TrimAlternateAlleles();
            var altAllele = variant.AlternateAlleles[0];

            Assert.Equal(100, altAllele.Start);
            Assert.Equal(100, altAllele.End);
            Assert.Equal("A", altAllele.ReferenceAllele);
            Assert.Equal("G", altAllele.AlternateAllele);
        }

        [Fact]
        public void TrimBothEnds()
        {
            var variant = VcfUtilities.GetVariant("chr1\t100\t.\tACTGA\tAGTCA\t.\t.\t.", _renamer);

            variant.TrimAlternateAlleles();
            var altAllele = variant.AlternateAlleles[0];

            Assert.Equal(101, altAllele.Start);
            Assert.Equal(103, altAllele.End);
            Assert.Equal("CTG", altAllele.ReferenceAllele);
            Assert.Equal("GTC", altAllele.AlternateAllele);
        }
    }
}