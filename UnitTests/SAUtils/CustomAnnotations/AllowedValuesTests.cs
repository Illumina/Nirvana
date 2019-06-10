using ErrorHandling.Exceptions;
using Xunit;
using SAUtils.Custom;

namespace UnitTests.SAUtils.CustomAnnotations
{
    public sealed class AllowedValuesTests
    {
        [Fact]
        public void IsEmptyValue_AsExpected()
        {
            Assert.True(AllowedValues.IsEmptyValue(""));
            Assert.True(AllowedValues.IsEmptyValue("."));
            Assert.False(AllowedValues.IsEmptyValue("-"));
        }

        [Fact]
        public void ValidatePredictionValue_Pass()
        {
            AllowedValues.ValidatePredictionValue("", "");
            AllowedValues.ValidatePredictionValue(".", "");
            AllowedValues.ValidatePredictionValue("P", "");
            AllowedValues.ValidatePredictionValue("Likely Benign", "");
            AllowedValues.ValidatePredictionValue("Vus", "");
        }

        [Fact]
        public void ValidatePredictionValue_ThrowException()
        {
            Assert.Throws<UserErrorException>(() => AllowedValues.ValidatePredictionValue("LikelyBenign", ""));
            Assert.Throws<UserErrorException>(() => AllowedValues.ValidatePredictionValue("Likely Benign, LB", ""));
        }
    }
}