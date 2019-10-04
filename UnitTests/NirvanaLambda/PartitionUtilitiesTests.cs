using System.Linq;
using NirvanaLambda;
using Xunit;

namespace UnitTests.NirvanaLambda
{
    public sealed class PartitionUtilitiesTests
    {
        [Fact]
        public void FindEqualOrClosestSmallerOffsets_AsExpected()
        {
            var sizeBasedOffsets = new long[] { 0, 100, 200, 300, 400 };
            var allLinearIndexes = new long[] { 15, 45, 97, 123, 146, 175, 200, 234, 265, 293, 401 };

            var blockOffsets = PartitionUtilities.FindEqualOrClosestSmallerOffsets(sizeBasedOffsets, allLinearIndexes);

            var expected = new long[] { 15, 97, 200, 293 };
            Assert.Equal(expected, blockOffsets);
        }

        [Fact]
        public void MergeConsecutiveEqualValues_AsExpected()
        {
            var input = new[] { 1, 2, 3, 3, 2, 5, 4, 4 };

            var expected = new[] { 1, 2, 3, 2, 5, 4 };

            Assert.Equal(expected, PartitionUtilities.MergeConsecutiveEqualValues(input).ToArray());
        }

        [Fact]
        public void GetEqualSizeOffsets_AsExpected()
        {
            var fileSize = 1001;
            var numPartitions = 3;

            var expected = new long[] { 0, 333, 666 };

            Assert.Equal(expected, PartitionUtilities.GetEqualSizeOffsets(fileSize, numPartitions));
        }
    }
}