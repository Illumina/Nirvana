using System;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    public class BenchmarkTests
    {
        [Fact]
        public void GetElapsedTime()
        {
            var bench = new Benchmark
            {
                StartTime = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 0, 200))
            };

            var observedElapsedTime = bench.GetElapsedTime();

            var tenths = observedElapsedTime.Milliseconds/100;
            Assert.Equal(tenths, 2);
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

            var bench = new Benchmark
            {
                StartTime = DateTime.Now.Subtract(new TimeSpan(0, 0, 0, 0, 200))
            };

            var observedElapsedTime = bench.GetElapsedIterationTime(3, "foobar", out unitsPerSecond);
            Assert.Contains("foobar/s", observedElapsedTime);
            Assert.InRange(unitsPerSecond, 12, 16);
        }
    }
}
