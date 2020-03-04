using Genome;
using ReferenceSequence.IO;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.VariantAnnotation.Sequence
{
    public sealed class CompressedSequenceReaderTests
    {
        [Fact]
        public void GetCompressedSequence()
        {
            using (var reader = new CompressedSequenceReader(ResourceUtilities.GetReadStream(Resources.TopPath("TestSeq_reference.dat"))))
            {
                Assert.Equal(GenomeAssembly.GRCh37, reader.Assembly);
                var sequence = reader.Sequence;

                var chromosome = new Chromosome("chrBob", "Bob", null, null, 1, 1);
                reader.GetCompressedSequence(chromosome);

                Assert.Null(sequence.CytogeneticBands);
                Assert.Equal(0, sequence.Length);

                chromosome = new Chromosome("chrTestSeq", "TestSeq", null, null, 1, 0);
                reader.GetCompressedSequence(chromosome);
                var bases = sequence.Substring(0, 100);

                Assert.NotNull(sequence.CytogeneticBands);
                Assert.Equal(53, sequence.Length);
                Assert.Equal("NNATGTTTCCACTTTCTCCTCATTAGANNNTAACGAATGGGTGATTTCCCTAN", bases);
            }
        }
    }
}
