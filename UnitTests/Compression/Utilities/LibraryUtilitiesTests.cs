using Xunit;
using Compression.Utilities;

namespace UnitTests.Compression.Utilities
{
    public sealed class LibraryUtilitiesTests
    {
        [Fact]
        public void CheckLibrary_ValidLibrary_NoExceptionThrown()
        {
            var ex = Record.Exception(() =>
            {
                LibraryUtilities.CheckLibrary();
            });

            Assert.Null(ex);
        }
    }
}
