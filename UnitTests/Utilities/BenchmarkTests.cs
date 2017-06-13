using System;
using System.Threading;
using CommandLine.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    public class BenchmarkTests
    {
        [Fact]
        public void GetElapsedTime()
        {
            var bench = new Benchmark();
            Thread.Sleep(100);
            var observedElapsedTime = bench.GetElapsedTime().TotalMilliseconds;
            Assert.True(observedElapsedTime > 0);
        }

        [Theory]
        [InlineData(0, 1, 2, 3, 400, "01:02:03.4")]
        [InlineData(1, 2, 3, 4, 500, "1:02:03:04.5")]
        public void ToHumanReadable(int days, int hours, int minutes, int seconds, int ms, string expectedResult)
        {
            var span = new TimeSpan(days, hours, minutes, seconds, ms);
            Assert.Equal(expectedResult, Benchmark.ToHumanReadable(span));
        }

        [Fact]
        public void GetElapsedIterationTime()
        {
            double unitsPerSecond;

            var bench = new Benchmark();
            Thread.Sleep(100);

            var observedElapsedTime = bench.GetElapsedIterationTime(3, "foobar", out unitsPerSecond);
            Assert.Contains("foobar/s", observedElapsedTime);
            Assert.True(unitsPerSecond > 0);
        }
    }
}
