using Genome;
using Intervals;
using Moq;
using UnitTests.TestDataStructures;
using VariantAnnotation.AnnotatedPositions;
using Variants;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class TranscriptUtilitiesTests
    {
        private static readonly IChromosome Chromosome = new Chromosome("chr21", "short", 21);
        private readonly ISequence _refSequence = new SimpleSequence("ACTTCGGGC", 12340);

        [Fact]
        public void IsDuplicateWithinInterval_not_intertion()
        {
            var simpleVar = GenSimpleDeletionMock();
            Assert.False(HgvsUtilities.IsDuplicateWithinInterval(_refSequence, simpleVar.Object, new Interval(1, 3), false));
        }

        [Fact]
        public void IsDuplicateWithinInterval_outside_interval()
        {
            var simpleVar = GenSimpleInsertionMock();

            // forward strand
            Assert.False(HgvsUtilities.IsDuplicateWithinInterval(_refSequence, simpleVar.Object, new Interval(12344, 12370), false));

            // reverse strand
            Assert.False(HgvsUtilities.IsDuplicateWithinInterval(_refSequence, simpleVar.Object, new Interval(12340, 12347), true));
        }

        [Fact]
        public void IsDuplicateWithinInterval_insertion_not_dup()
        {
            var simpleVar = GenSimpleInsertionMock();

            // forward strand
            Assert.False(HgvsUtilities.IsDuplicateWithinInterval(new SimpleSequence("ACTTCGGGC", 12340),
                simpleVar.Object, new Interval(12300, 12400), false));

            // reverse strand
            Assert.False(HgvsUtilities.IsDuplicateWithinInterval(new SimpleSequence("ACTTCGGGC", 12340),
                simpleVar.Object, new Interval(12300, 12400), true));
        }

        [Fact]
        public void IsDuplicateWithinInterval_insertion_is_dup()
        {
            var simpleVar = GenSimpleInsertionMock();

            // forward strand
            Assert.True(HgvsUtilities.IsDuplicateWithinInterval(new SimpleSequence("ACCTGGGGC", 12340),
                simpleVar.Object, new Interval(12300, 12400), false));

            // reverse strand
            Assert.True(HgvsUtilities.IsDuplicateWithinInterval(new SimpleSequence("ACTTCCTGC", 12340),
                simpleVar.Object, new Interval(12300, 12400), true));
        }

        private static Mock<ISimpleVariant> GenSimpleDeletionMock()
        {
            var simpleVar = new Mock<ISimpleVariant>();
            simpleVar.SetupGet(x => x.Chromosome).Returns(Chromosome);
            simpleVar.SetupGet(x => x.Start).Returns(12345);
            simpleVar.SetupGet(x => x.End).Returns(12348);
            simpleVar.SetupGet(x => x.RefAllele).Returns("CTG");
            simpleVar.SetupGet(x => x.AltAllele).Returns("");
            simpleVar.SetupGet(x => x.Type).Returns(VariantType.deletion);
            return simpleVar;
        }

        private static Mock<ISimpleVariant> GenSimpleInsertionMock()
        {
            var simpleVar = new Mock<ISimpleVariant>();
            simpleVar.SetupGet(x => x.Chromosome).Returns(Chromosome);
            simpleVar.SetupGet(x => x.Start).Returns(12346);
            simpleVar.SetupGet(x => x.End).Returns(12345);
            simpleVar.SetupGet(x => x.RefAllele).Returns("");
            simpleVar.SetupGet(x => x.AltAllele).Returns("CTG");
            simpleVar.SetupGet(x => x.Type).Returns(VariantType.insertion);
            return simpleVar;
        }
    }
}