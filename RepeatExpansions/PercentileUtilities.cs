using System;
using System.Collections.Generic;
using System.Linq;

namespace RepeatExpansions
{
    public static class PercentileUtilities
    {
        public static double[] ComputePercentiles(int valueCount, IReadOnlyList<int> alleleCounts)
        {
            var percentiles       = new double[valueCount];
            var smallerValueCount = 0;
            int totalCount        = alleleCounts.Sum();

            percentiles[0] = 0;

            for (var i = 1; i < valueCount; i++)
            {
                smallerValueCount += alleleCounts[i - 1];
                percentiles[i]    =  100.0 * smallerValueCount / totalCount;
            }

            return percentiles;
        }

        public static double GetPercentile<T>(T inputValue, T[] referenceValues, double[] referencePercentiles)
        {
            int index = Array.BinarySearch(referenceValues, inputValue);
            if (index >= 0) return referencePercentiles[index];

            index = ~index;
            return index == referenceValues.Length ? 100.00 : referencePercentiles[index];
        }
    }
}