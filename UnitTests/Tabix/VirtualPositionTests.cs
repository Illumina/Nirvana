using Tabix;
using Xunit;

namespace UnitTests.Tabix
{
    public sealed class VirtualPositionTests
    {
        [Fact]
        public void VirtualPosition_LoopBack()
        {
            const long expectedVirtualPosition = 3591443256775;

            (long fileOffset, int blockOffset) = VirtualPosition.From(expectedVirtualPosition);
            long observedVirtualPosition = VirtualPosition.To(fileOffset, blockOffset);

            Assert.Equal(expectedVirtualPosition, observedVirtualPosition);
        }
    }
}
