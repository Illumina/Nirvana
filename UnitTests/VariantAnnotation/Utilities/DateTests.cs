using System.Text.RegularExpressions;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.VariantAnnotation.Utilities
{
    public sealed class DateTests
    {
        [Fact]
        public void GetTimeStamp_CheckFormat()
        {
            var timeStamp = Date.CurrentTimeStamp;
            var regex = new Regex(@"\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}");
            Assert.True(regex.Match(timeStamp).Success);
        }
    }
}
