using System;
using System.Threading;
using VariantAnnotation.Utilities;
using Xunit;
using Xunit.Abstractions;

namespace UnitTests.Utilities
{
    public class MemoryUtilitiesTests
    {
        private readonly ITestOutputHelper _output;

        /// <summary>
        /// constructor
        /// </summary>
        public MemoryUtilitiesTests(ITestOutputHelper output)
        {
            _output = output;
        }

        //[Fact]
        //public void GetPeakMemoryUsage()
        //{
        //    var peakMemoryUsage = MemoryUtilities.ToHumanReadable(MemoryUtilities.GetPeakMemoryUsage());

        //    var cols = peakMemoryUsage.Split(' ');
        //    Assert.Equal(2, cols.Length);

        //    var units = cols[1];
        //    Assert.Equal("MB", units);
        //}

        [Theory]
        [InlineData(10, "10 B")]
        [InlineData(10000, "9.8 KB")]
        [InlineData(10000000, "9.5 MB")]
        [InlineData(10000000000, "9.313 GB")]
        public void ToHumanReadable(long numBytes, string expectedResult)
        {
            Assert.Equal(expectedResult, MemoryUtilities.ToHumanReadable(numBytes));
        }

        [Theory(Skip = "Flaky test")]
        [InlineData(524288, 35000)]
        public void NumBytesUsed(int numInts, int epsilon)
        {
            const int maxNumMeasurements = 10;
            var expectedNumBytesUsed = numInts * 4;

            var minDiff = long.MaxValue;

            for (var i = 0; i < maxNumMeasurements; i++)
            {
                var numBytesUsed = GetNumBytesUsed(numInts);
                var diff = Math.Abs(numBytesUsed - expectedNumBytesUsed);
                if (diff < minDiff) minDiff = diff;

                _output.WriteLine($"{i}: expected: {expectedNumBytesUsed}\tactual: {numBytesUsed}\tdiff: {diff}\tepsilon: {epsilon}\tminDiff: {minDiff}");

                if (diff < epsilon) break;

                Thread.Sleep(300);
            }

            Assert.InRange(minDiff, 0, epsilon);
        }

        private static long GetNumBytesUsed(int numInts)
        {
            // force garbage collection
            GC.Collect();

            var begin = MemoryUtilities.NumBytesUsed(true);

            var ints = new int[numInts];
            ints[0] = 123;

            var end = MemoryUtilities.NumBytesUsed(true);            

            // force the array to be in memory until we have our measurements
            Assert.Equal(123, ints[0]);

            return end - begin;
        }
    }
}
