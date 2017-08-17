using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.Sequence
{
    public sealed class CompressedSequenceReaderTests
    {
        [Fact]
        public void GetCompressedSequence()
        {
            using (var reader = new CompressedSequenceReader(
                ResourceUtilities.GetReadStream(Resources.TopPath("TestSeq_reference.dat"))))
            {
                var sequence = reader.Sequence;
                var chromosome = new Chromosome("chrBob", "Bob", 0);
                reader.GetCompressedSequence(chromosome);

                Assert.Equal(0, sequence.Length);

                chromosome = new Chromosome("chrTestSeq", "TestSeq", 0);
                reader.GetCompressedSequence(chromosome);

                Assert.NotNull(reader.CytogeneticBands);
                Assert.Equal(GenomeAssembly.GRCh37, reader.Assembly);
                Assert.Equal(53, sequence.Length);
            }
        }
    }
}
