using CommandLine.Utilities;
using Xunit;

namespace UnitTests.CommandLine.Utilities
{
    public sealed class MemoryUtilitiesTests
    {
        [Fact]
        public void ToHumanReadable_Convert_Bytes()
        {
            var observedValue = MemoryUtilities.ToHumanReadable(123);
            Assert.Equal("123 B", observedValue);
        }

        [Fact]
        public void ToHumanReadable_Convert_KiloBytes()
        {
            var observedValue = MemoryUtilities.ToHumanReadable(1_234);
            Assert.Equal("1.2 KB", observedValue);
        }

        [Fact]
        public void ToHumanReadable_Convert_MegaBytes()
        {
            var observedValue = MemoryUtilities.ToHumanReadable(1_234_567);
            Assert.Equal("1.2 MB", observedValue);
        }

        [Fact]
        public void ToHumanReadable_Convert_GigaBytes()
        {
            var observedValue = MemoryUtilities.ToHumanReadable(1_234_567_890);
            Assert.Equal("1.150 GB", observedValue);
        }
    }
}