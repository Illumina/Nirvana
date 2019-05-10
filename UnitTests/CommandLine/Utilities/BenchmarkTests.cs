using System;
using CommandLine.Utilities;
using Xunit;

namespace UnitTests.CommandLine.Utilities
{
    public sealed class BenchmarkTests
    {
        [Fact]
        public void ToHumanReadable_Days()
        {
            const string expectedString = "1:02:03:04.5";
            var timeSpan = new TimeSpan(1, 2, 3, 4, 500);
            var observedString = Benchmark.ToHumanReadable(timeSpan);
            Assert.Equal(expectedString, observedString);
        }

        [Fact]
        public void ToHumanReadable_LessThanOneDay()
        {
            const string expectedString = "01:02:03.4";
            var timeSpan = new TimeSpan(0, 1, 2, 3, 400);
            var observedString = Benchmark.ToHumanReadable(timeSpan);
            Assert.Equal(expectedString, observedString);
        }

        [Fact]
        public void Benchmark_EndToEnd()
        {
            var benchmark            = new Benchmark();
            // perform some work
            benchmark.GetElapsedIterationTime(100, out double unitsPerSecond);
            Assert.True(unitsPerSecond>0);
        }
    }
}
