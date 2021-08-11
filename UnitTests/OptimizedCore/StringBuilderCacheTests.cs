using OptimizedCore;
using Xunit;

namespace UnitTests.OptimizedCore
{
    public sealed class StringBuilderCacheTests
    {
        [Fact]
        public void Acquire_UseAndRelease()
        {
            const string expectedString  = "ABC123";
            const string expectedString2 = "The quick brown fox jumps over the lazy dog.";

            var sb = StringBuilderPool.Get();
            sb.Append(expectedString);
            Assert.Equal(expectedString, StringBuilderPool.GetStringAndReturn(sb));

            // acquire an existing string builder
            sb = StringBuilderPool.Get();
            sb.Append(expectedString2);
            Assert.Equal(expectedString2, StringBuilderPool.GetStringAndReturn(sb));
        }
    }
}
