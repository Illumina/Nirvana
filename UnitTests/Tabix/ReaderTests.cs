using System.Collections.Generic;
using System.IO;
using System.Text;
using Genome;
using IO;
using Tabix;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.Tabix
{
    public sealed class ReaderTests
    {
        [Fact]
        public void Read_Nominal()
        {
            var refNameToChromosome = new Dictionary<string, IChromosome> { ["chr17"] = new Chromosome("chr17", "17", 16) };

            using (var stream = FileUtilities.GetReadStream(Resources.TopPath("miniHEXA_minimal.vcf.gz.tbi")))
            {
                var index = Reader.GetTabixIndex(stream, refNameToChromosome);

                Assert.Equal(1, index.BeginIndex);
                Assert.Equal('#', index.CommentChar);
                Assert.Equal(-1, index.EndIndex);
                Assert.Equal(Constants.VcfFormat, index.Format);
                Assert.Equal(0, index.NumLinesToSkip);
                Assert.Equal(0, index.SequenceNameIndex);

                Assert.Single(index.ReferenceSequences);

                var refSeq = index.ReferenceSequences[0];
                Assert.Equal("chr15", refSeq.Chromosome.UcscName);
                Assert.Equal(4675, refSeq.LinearFileOffsets.Length);
                Assert.Equal((ulong)4587, refSeq.LinearFileOffsets[4370]);

                Assert.Equal(306, refSeq.IdToChunks.Count);

                var chunks = refSeq.IdToChunks[9062];
                Assert.NotNull(chunks);
                Assert.Single(chunks);

                var chunk = chunks[0];
                Assert.Equal((ulong)61269, chunk.Begin);
                Assert.Equal((ulong)991626923, chunk.End);
            }
        }

        [Fact]
        public void Read_NotTabixFormat()
        {
            using (var ms = new MemoryStream())
            {
                using (var writer = new BinaryWriter(ms, Encoding.UTF8, true))
                {
                    writer.Write("The quick brown fox jumped over the lazy dog.");
                }

                ms.Position = 0;

                using (var reader = new BinaryReader(ms))
                {
                    Assert.Throws<InvalidDataException>(delegate
                    {
                        Reader.Read(reader, null);
                    });
                }
            }
        }
    }
}
