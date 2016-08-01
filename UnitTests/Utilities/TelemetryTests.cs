using System;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    public class TelemetryTests
    {
        [Theory]
        [InlineData(0, 0, 40, 51, "00:40:51")]
        [InlineData(0, 0, 60, 00, "01:00:00")]
        [InlineData(1, 0, 30, 12, "24:30:12")]
        public void GetWallTime(int days, int hours, int minutes, int seconds, string expectedResult)
        {
            var span = new TimeSpan(days, hours, minutes, seconds);
            var wallTime = Telemetry.GetWallTime(span);
            Assert.Equal(expectedResult, wallTime);
        }
    }
}
