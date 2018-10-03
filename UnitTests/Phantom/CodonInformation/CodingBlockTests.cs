using System.Collections.Generic;
using Phantom.CodonInformation;
using Xunit;

namespace UnitTests.Phantom.CodonInformation
{
    public sealed class CodingBlockTests
    {
        [Fact]
        public void GetHashCode_TestDifference()
        {
            var codingBlock  = new CodingBlock(10, 20, 0);
            var codingBlock2 = new CodingBlock(11, 20, 0);
            var codingBlock3 = new CodingBlock(10, 21, 0);
            var codingBlock4 = new CodingBlock(10, 20, 1);

            var hashCodes = new HashSet<int>
            {
                codingBlock.GetHashCode(),
                codingBlock2.GetHashCode(),
                codingBlock3.GetHashCode(),
                codingBlock4.GetHashCode()
            };

            Assert.Equal(4, hashCodes.Count);
        }
    }
}
