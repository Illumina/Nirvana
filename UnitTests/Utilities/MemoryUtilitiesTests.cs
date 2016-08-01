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

        [Fact]
        public void GetPeakMemoryUsage()
        {
            var peakMemoryUsage = MemoryUtilities.ToHumanReadable(MemoryUtilities.GetPeakMemoryUsage());

            var cols = peakMemoryUsage.Split(' ');
            Assert.Equal(2, cols.Length);

            var units = cols[1];
            Assert.Equal("MB", units);
        }

        [Theory]
        [InlineData(10, "10 B")]
        [InlineData(10000, "9.8 KB")]
        [InlineData(10000000, "9.5 MB")]
        [InlineData(10000000000, "9.313 GB")]
        public void ToHumanReadable(long numBytes, string expectedResult)
        {
            Assert.Equal(expectedResult, MemoryUtilities.ToHumanReadable(numBytes));
        }

        [Theory]
        [InlineData(524288, 35000)]
        public void NumBytesUsed(int numInts, int epsilon)
        {
            const int maxNumMeasurements = 10;
            int expectedNumBytesUsed = numInts * 4;

            long minDiff = long.MaxValue;

            for (int i = 0; i < maxNumMeasurements; i++)
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

        /// <summary>
        /// returns the number of bytes used
        /// </summary>
        private static long NumBytesUsed(bool forceFullCollection)
        {
            return GC.GetTotalMemory(forceFullCollection);
        }

        private static long GetNumBytesUsed(int numInts)
        {
            // force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();

            var begin = NumBytesUsed(true);

            var ints = new int[numInts];
            ints[0] = 123;

            var end = NumBytesUsed(true);            

            // force the array to be in memory until we have our measurements
            Assert.Equal(123, ints[0]);

            return end - begin;
        }
    }
}
