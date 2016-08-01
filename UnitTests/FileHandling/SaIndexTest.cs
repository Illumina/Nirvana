using System.IO;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.FileHandling
{
    public sealed class SaIndexTest : RandomFileBase
    {
        [Fact]
        public void Search()
        {
            var saIndex = new SaIndex(3);
            var nodeCount = 3 * SaIndexNode.SaNodeWidth;
            for (uint i = 0; i < nodeCount; i++)
                saIndex.Add(100 + i * 3, 1000 + i * 30, i % 3 == 0);//every third item is ref minor

            var randomPath = GetRandomPath();
            saIndex.Write(randomPath, "chr1");

            using (var reader = new BinaryReader(FileUtilities.GetFileStream(randomPath)))
            {
                var readSaIndex = new SaIndex(reader);
                Assert.Equal(uint.MinValue, readSaIndex.GetFileLocation(99));// prior to index
                Assert.Equal((uint)1000, readSaIndex.GetFileLocation(100));
                Assert.Equal(uint.MinValue, readSaIndex.GetFileLocation(101));//not present position
                Assert.Equal((uint)1030, readSaIndex.GetFileLocation(103));

                Assert.Equal(uint.MinValue, readSaIndex.GetFileLocation(131));//location past the index
            }

            File.Delete(randomPath);

        }

        [Fact]
        public void RefMinorSearch()
        {
            var saIndex = new SaIndex(3);
            var nodeCount = 3 * SaIndexNode.SaNodeWidth;
            for (uint i = 0; i < nodeCount; i++)
                saIndex.Add(100 + i * 3, 1000 + i * 30, i % 3 == 0);//every third item is ref minor

            var randomPath = GetRandomPath();
            saIndex.Write(randomPath, "chr1");

            using (var reader = new BinaryReader(FileUtilities.GetFileStream(randomPath)))
            {
                var readSaIndex = new SaIndex(reader);

                Assert.True(readSaIndex.IsRefMinor(100));
                //this result should be cached for the next query
                Assert.Equal((uint)1000, readSaIndex.GetFileLocation(100));

                Assert.False(readSaIndex.IsRefMinor(101));//not present position

                Assert.Equal((uint)1030, readSaIndex.GetFileLocation(103));
                //this result should be cached for the next query
                Assert.True(readSaIndex.IsRefMinor(109));

                Assert.Equal(uint.MinValue, readSaIndex.GetFileLocation(131));//location past the index
            }

            File.Delete(randomPath);
        }
    }
}
