using System.IO;
using System.IO.Compression;
using Jasix;
using Jasix.DataStructures;
using VariantAnnotation.Utilities;
using Xunit;
using UnitTests.TestUtilities;
using Compression.FileHandling;

namespace UnitTests.Jasix
{
    public sealed class JasixTests
    {
        [Fact]
        public void Query_succeedes_when_it_overlaps_tail_of_previous_bin()
        {
            var chrIndex = new JasixChrIndex("chr1");

            for (var i = 100; i < 100 + JasixCommons.PreferredNodeCount; i++)
            {
                chrIndex.Add(i, i + 5, 100_000 + i);
            }

            for (var i = 102 + JasixCommons.PreferredNodeCount; i < 152 + JasixCommons.PreferredNodeCount; i++)
            {
                chrIndex.Add(i, i + 5, 100_020 + i);
            }

            //close current node
            chrIndex.Flush();

            Assert.Equal(100_100, chrIndex.FindFirstSmallVariant(102, 103));
        }

        [Fact]
        public void JasixIndexTest()
        {
            var index = new JasixIndex();

            //creating two nodes each containing 50 entries
            for (var i = 0; i < 2 * JasixCommons.PreferredNodeCount; i++)
            {
                index.Add("chr1", 100 + i, 101 + i, 100_000 + i);
            }

            index.Add("chr1", 160 + 2 * JasixCommons.PreferredNodeCount, 166 + 2 * JasixCommons.PreferredNodeCount, 200_100);
            index.Add("chr2", 100, 100, 200_150);
            index.Add("chr2", 102, 105, 200_200);

            index.Flush();

            Assert.Equal(100_000, index.GetFirstVariantPosition("chr1", 100, 102));
            Assert.Equal(100_000 + JasixCommons.PreferredNodeCount, index.GetFirstVariantPosition("chr1", 2 * JasixCommons.PreferredNodeCount + 55, 2 * JasixCommons.PreferredNodeCount + 55));
            Assert.Equal(-1, index.GetFirstVariantPosition("chr1", 2 * JasixCommons.PreferredNodeCount + 120, 2 * JasixCommons.PreferredNodeCount + 124));
            Assert.Equal(200_100, index.GetFirstVariantPosition("chr1", 2 * JasixCommons.PreferredNodeCount + 158, 2 * JasixCommons.PreferredNodeCount + 160));
            Assert.Equal(200_150, index.GetFirstVariantPosition("chr2", 103, 105));
        }


        [Fact]
        public void AllFromAchromosome()
        {
            var index = new JasixIndex();

            index.Add("chr1", 100, 101, 100000);
            index.Add("chr1", 105, 109, 100050);
            index.Add("chr1", 160, 166, 100100);
            index.Add("chr2", 100, 100, 100150);
            index.Add("chr2", 102, 105, 100200);

            index.Flush();

            var chrPos = Utilities.ParseQuery("chr1");

            Assert.Equal(100000, index.GetFirstVariantPosition(chrPos.Item1, chrPos.Item2, chrPos.Item3));

            chrPos = Utilities.ParseQuery("chr2");
            Assert.Equal(100150, index.GetFirstVariantPosition(chrPos.Item1, chrPos.Item2, chrPos.Item3));
        }

        [Fact]
        public void FindLargeVaritants_method_does_not_return_small_variants()
        {
            var index = new JasixIndex();

            index.Add("chr1", 100, 101, 100_000);
            index.Add("chr1", 105, 109, 100_050);
            index.Add("chr1", 160, 166, 100_100);
            index.Add("chr1", 200, 1000, 100_075);//large variant
            index.Add("chr2", 100, 100, 100_150);
            index.Add("chr2", 102, 105, 100_200);

            index.Flush();

            //checking large variants
            Assert.Null(index.LargeVariantPositions("chr1", 100, 199));
            var largeVariants = index.LargeVariantPositions("chr1", 100, 201);
            Assert.NotNull(largeVariants);
            Assert.Single(largeVariants);
            Assert.Equal(100075, largeVariants[0]);
        }

        [Fact]
        public void IndexWriteRead()
        {
            var index = new JasixIndex();

            index.Add("chr1", 100, 101, 100000);
            index.Add("chr1", 105, 109, 100050);
            index.Add("chr1", 150, 1000, 100075);//large variant
            index.Add("chr1", 160, 166, 100100);
            index.Add("chr2", 100, 100, 100150);
            index.Add("chr2", 102, 105, 100200);

            var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            using (var writer = FileUtilities.GetCreateStream(tempFile))
            {
                index.Write(writer);
            }

            JasixIndex readBackIndex;
            using (var stream = FileUtilities.GetReadStream(tempFile))
            {
                readBackIndex = new JasixIndex(stream);
            }
            File.Delete(tempFile);

            Assert.Equal(100000, readBackIndex.GetFirstVariantPosition("chr1", 100, 102));
            Assert.Equal(100000, readBackIndex.GetFirstVariantPosition("chr1", 103, 104));
            Assert.Equal(100000, readBackIndex.GetFirstVariantPosition("chr1", 120, 124));
            Assert.Equal(100000, readBackIndex.GetFirstVariantPosition("chr1", 158, 160));
            Assert.Equal(100150, readBackIndex.GetFirstVariantPosition("chr2", 103, 105));

            //checking large variants
            Assert.Null(readBackIndex.LargeVariantPositions("chr1", 100, 149));
            var largeVariants = readBackIndex.LargeVariantPositions("chr1", 100, 201);
            Assert.NotNull(largeVariants);
            Assert.Single(largeVariants);
            Assert.Equal(100075, largeVariants[0]);
        }

        [Fact]
        public void JasixStreamReader()
        {
            var stream = ResourceUtilities.GetReadStream(Resources.TopPath("TinyAnnotated.json"));

            var lineCount = 0;
            using (var jasixReader = new BgzipTextReader(stream))
            {
                while (jasixReader.ReadLine() != null)
                {
                    lineCount++;
                }
            }

            Assert.Equal(4, lineCount);
        }

        [Fact]
        public void TestIndexCreation()
        {
            var readStream = new BlockGZipStream(ResourceUtilities.GetReadStream(Resources.TopPath("cosmicv72.indels.json.gz")), CompressionMode.Decompress);
            var tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            using (var indexCreator = new IndexCreator(readStream, FileUtilities.GetCreateStream(tempFile)))
            {
                indexCreator.CreateIndex();
            }
            JasixIndex readBackIndex;
            using (var stream = FileUtilities.GetReadStream(tempFile))
            {
                readBackIndex = new JasixIndex(stream);
            }

            Assert.Equal(1591, readBackIndex.GetFirstVariantPosition("chr1", 9775924, 9775924));
            Assert.Equal(11500956299, readBackIndex.GetFirstVariantPosition("chr2", 16081096, 16081096));
            Assert.Equal(372100991296, readBackIndex.GetFirstVariantPosition("chr20", 36026164, 36026164));
            Assert.Equal(377682846863, readBackIndex.GetFirstVariantPosition("chrX", 66765044, 66765044));

            File.Delete(tempFile);
        }
    }
}
