using System.IO;
using UnitTests.Utilities;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.FileHandling
{
    public sealed class SaIndexNodeTests : RandomFileBase
    {
        [Fact]
        public void NodePopulation()
        {
            uint position = 100;
            uint fileLocation = 100000;
            var saIndexNode = new SaIndexNode(position, fileLocation);

            for (var i = 0; i < SaIndexNode.SaNodeWidth; i++)
                Assert.True(saIndexNode.TryAdd(position + (uint)i * 3, fileLocation + (uint)i * 30, false));

            //the node should be full by now
            Assert.False(saIndexNode.TryAdd(position + SaIndexNode.SaNodeWidth, fileLocation + ushort.MaxValue - 1, false));
        }

        [Fact]
        public void AddOutOfRangePosition()
        {
            uint position = 100;
            uint fileLocation = 100000;
            var saIndexNode = new SaIndexNode(position, fileLocation);

            // the node is out of range
            Assert.False(saIndexNode.TryAdd(position + uint.MaxValue, fileLocation + ushort.MaxValue - 1, false));// location should not cause any problem
            Assert.False(saIndexNode.TryAdd(position + uint.MaxValue - 1, fileLocation + ushort.MaxValue, false));// location should cause failure
        }

        [Fact]
        public void NodeWriteRead()
        {
            var saIndexNode = MakeSaIndexNode(100);

            var randomFilePath = GetRandomPath();
            using (var writer = new BinaryWriter(FileUtilities.GetCreateStream(randomFilePath)))
            {
                saIndexNode.Write(writer);
            }

            using (var reader = new BinaryReader(FileUtilities.GetReadStream(randomFilePath)))
            {
                var readNode = new SaIndexNode(reader);
                Assert.NotNull(readNode);

                Assert.Equal((uint)100, readNode.GetFirstPosition());
                Assert.Equal((uint)100000, readNode.GetFileLocation(100));
            }

            File.Delete(randomFilePath);
        }

        [Fact]
        public void NodeQuery()
        {
            var saIndexNode = MakeSaIndexNode(100);

            Assert.Equal(uint.MinValue, saIndexNode.GetFileLocation(99));
            Assert.Equal((uint)100000, saIndexNode.GetFileLocation(100));
            Assert.True(saIndexNode.IsRefMinor(100));
            Assert.Equal((uint)100030, saIndexNode.GetFileLocation(103));
            Assert.True(saIndexNode.IsRefMinor(109));
            Assert.Equal(uint.MinValue, saIndexNode.GetFileLocation(101));
            Assert.Equal(uint.MinValue, saIndexNode.GetFileLocation(150));
        }

        [Fact]
        public void CompareToInt()
        {
            var saIndexNode = MakeSaIndexNode(100);

            Assert.Equal(1, saIndexNode.CompareTo(99));
            Assert.Equal(0, saIndexNode.CompareTo(100));
            Assert.Equal(-1, saIndexNode.CompareTo(103));
        }

        [Fact]
        public void CompareToObject()
        {
            var saIndexNode100 = MakeSaIndexNode(100);
            var saIndexNode101 = MakeSaIndexNode(101);
            var saIndexNode102 = MakeSaIndexNode(102);

            Assert.Equal(0, saIndexNode100.CompareTo(saIndexNode100));
            Assert.Equal(1, saIndexNode101.CompareTo(saIndexNode100));
            Assert.Equal(-1, saIndexNode101.CompareTo(saIndexNode102));
        }

        private static SaIndexNode MakeSaIndexNode(uint position)
        {
            uint fileLocation = 100000;
            var saIndexNode = new SaIndexNode(position, fileLocation, true);

            for (var i = 0; i < SaIndexNode.SaNodeWidth; i++)
                saIndexNode.TryAdd(position + (uint)i * 3, fileLocation + (uint)i * 30, i % 3 == 0);//make every third node a ref minor
            return saIndexNode;
        }
    }
}
