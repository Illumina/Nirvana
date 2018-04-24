using Vcf;
using Xunit;

namespace UnitTests.Vcf
{
    public sealed class StringExtensionsTests
    {

        [Theory]
        [InlineData("12",12)]
        [InlineData("12.0", null)]
        public void GetNullableValue_int(string input, int? exp)
        {
            var observe = input.GetNullableValue<int>(int.TryParse);
            Assert.Equal(exp,observe);
        }

        [Theory]
        [InlineData("12", 12)]
        [InlineData("12.0", 12.0)]
        [InlineData("a.8",null)]
        public void GetNullableValue_double(string input, double? exp)
        {
            var observe = input.GetNullableValue<double>(double.TryParse);
            Assert.Equal(exp, observe);
        }


        [Theory]
        [InlineData("12", new[]{12})]
        [InlineData("12,13", new[]{12,13})]
        [InlineData("12,13.0", null)]
        public void SplitToArray_int(string input, int[] exp)
        {
            var observe = input.SplitToArray();
            Assert.Equal(exp, observe);
        }


        //[Theory]
        //[InlineData("12", new double[] { 12 })]
        //[InlineData("12,13", new double[] { 12, 13 })]
        //[InlineData("12,13.0", new[] { 12, 13.0})]
        //[InlineData("12.a,13.0", null)]
        //public void SplitToArray_double(string input, double[] exp)
        //{
        //    var observe = input.SplitToArray<double>(',', double.TryParse);
        //    Assert.Equal(exp, observe);
        //}

    }
}