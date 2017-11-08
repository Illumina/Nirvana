using System;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public static class ComputingUtilities
    {
        public static string ComputeFrequency(int? alleleNumber, int? alleleCount)
        {
            return alleleNumber != null && alleleNumber.Value > 0 && alleleCount != null
                ? ((double)alleleCount / alleleNumber.Value).ToString(JsonCommon.FrequencyRoundingFormat)
                : null;
        }

        public static int GetCoverage(double depth, double allAlleleNumber)
        {
            return (int) Math.Round(depth / allAlleleNumber, 0, MidpointRounding.AwayFromZero);
        }
    }
}