using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.Sequence
{
    public sealed class CompressedSequenceCommonTests
    {
        [Theory]
        [InlineData(1234, false)]
        [InlineData(1073743058, true)]
        public void HasSequenceOffset(int num, bool expectedResult)
        {
            var observedResult = CompressedSequenceCommon.HasSequenceOffset(num);
            Assert.Equal(expectedResult, observedResult);
        }
    }
}
