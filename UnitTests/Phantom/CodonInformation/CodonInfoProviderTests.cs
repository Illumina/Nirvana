using Phantom.CodonInformation;
using Xunit;

namespace UnitTests.Phantom.CodonInformation
{
    public sealed class CodonInfoProviderTests
    {
        [Fact]
        public void GetCodonRange_AsExpected()
        {
            var position = 73115941;
            var codingBlock = new CodingBlock(73115838, 73116000, 1);

            int range = CodonInfoProvider.GetCodonRange(position, codingBlock);

            Assert.Equal(73115941, range);
        }
    }
}