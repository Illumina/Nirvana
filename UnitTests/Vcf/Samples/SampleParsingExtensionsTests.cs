using Vcf.Sample;
using Xunit;

namespace UnitTests.Vcf.Samples
{
    public sealed class SampleParsingExtensionsTests
    {
        [Fact]
        public void GetString()
        {
            var cols = new[] { "knatte", "fnatte", "tjatte" };
            string observedResult = cols.GetString(2);
            Assert.Equal(cols[2], observedResult);
        }

        [Fact]
        public void GetString_NullIndex_ReturnNull()
        {
            var cols = new[] { "temp" };
            string observedResult = cols.GetString(null);
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetFloat()
        {
            var observedResult = "1.23".GetFloat();
            Assert.Equal(1.23f, observedResult);
        }

        [Fact]
        public void GetFloat_NotFloat_ReturnNull()
        {
            float? observedResult = "test".GetFloat();
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetFloat_NullString_ReturnNull()
        {
            string s = null;
            float? observedResult = s.GetFloat();
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetDouble()
        {
            double? observedResult = "1.23".GetDouble();
            Assert.Equal(1.23, observedResult);
        }

        [Fact]
        public void GetDouble_NotDouble_ReturnNull()
        {
            double? observedResult = "test".GetDouble();
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetDouble_NullString_ReturnNull()
        {
            string s = null;
            double? observedResult = s.GetDouble();
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetInteger()
        {
            int? observedResult = "17".GetInteger();
            Assert.Equal(17, observedResult);
        }

        [Fact]
        public void GetInteger_NotInteger_ReturnNull()
        {
            int? observedResult = "test".GetInteger();
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetInteger_NullString_ReturnNull()
        {
            string s = null;
            int? observedResult = s.GetInteger();
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetStrings()
        {
            string[] observedResult = "17,test,13".GetStrings();
            Assert.Equal(new[] { "17", "test", "13" }, observedResult);
        }

        [Fact]
        public void GetStrings_NullString_ReturnNull()
        {
            string s = null;
            string[] observedResult = s.GetStrings();
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetIntegers()
        {
            int[] observedResult = "17,13,11".GetIntegers();
            Assert.Equal(new[] { 17, 13, 11 }, observedResult);
        }

        [Fact]
        public void GetIntegers_NotInteger_ReturnNull()
        {
            int[] observedResult = "10,13,bobby".GetIntegers();
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetIntegers_NullString_ReturnNull()
        {
            string s = null;
            int[] observedResult = s.GetIntegers();
            Assert.Null(observedResult);
        }
    }
}
