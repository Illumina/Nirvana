using System.IO;
using System.Text;
using Compression.Utilities;
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
            using (var indexReader = GZipUtilities.GetAppropriateBinaryReader(Resources.TopPath("AU144A_BadVariant.vcf.gz.tbi")))
            {
                Index index = Reader.Read(indexReader);

                Assert.Equal(1, index.BeginIndex);
                Assert.Equal('#', index.CommentChar);
                Assert.Equal(-1, index.EndIndex);
                Assert.Equal(Constants.VcfFormat, index.Format);
                Assert.Equal(0, index.NumLinesToSkip);
                Assert.Equal(0, index.SequenceNameIndex);

                Assert.Single(index.ReferenceSequences);

                var refSeq = index.ReferenceSequences[0];
                Assert.Equal("chr17", refSeq.Chromosome);
                Assert.Equal(4028, refSeq.LinearFileOffsets.Length);
                Assert.Equal((ulong)6051, refSeq.LinearFileOffsets[4027]);

                Assert.Single(refSeq.IdToChunks);

                var chunks = refSeq.IdToChunks[8708];
                Assert.NotNull(chunks);
                Assert.Single(chunks);

                var chunk = chunks[0];
                Assert.Equal((ulong)6051, chunk.Begin);
                Assert.Equal((ulong)122880000, chunk.End);
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
                        Reader.Read(reader);
                    });
                }
            }
        }
    }
}
