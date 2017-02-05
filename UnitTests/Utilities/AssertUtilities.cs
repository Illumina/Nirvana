using System.Linq;
using System.Reflection;
using VariantAnnotation.Interface;
using Xunit;

namespace UnitTests.Utilities
{
    public static class AssertUtilities
    {
        // ReSharper disable once UnusedParameter.Global
        public static void CheckAlleleCount(int expectedCount, IAnnotatedVariant annotatedVariant)
        {
            var observedCount = annotatedVariant.AnnotatedAlternateAlleles.Count;
            Assert.Equal(expectedCount, observedCount);
        }

        // ReSharper disable once UnusedParameter.Global
        public static void CheckSampleCount(int expectedCount, IAnnotatedVariant annotatedVariant)
        {
            var observedCount = annotatedVariant.AnnotatedSamples.Count();
            Assert.Equal(expectedCount, observedCount);
        }

        // ReSharper disable once UnusedParameter.Global
        public static void CheckEnsemblTranscriptCount(int expectedCount, IAnnotatedAlternateAllele altAllele)
        {
            var observedCount = altAllele?.EnsemblTranscripts.Count;
            Assert.Equal(expectedCount, observedCount);
        }

        // ReSharper disable once UnusedParameter.Global
        public static void CheckRefSeqTranscriptCount(int expectedCount, IAnnotatedAlternateAllele altAllele)
        {
            var observedCount = altAllele?.RefSeqTranscripts.Count;
            Assert.Equal(expectedCount, observedCount);
        }

        // ReSharper disable once UnusedParameter.Global
        public static void CheckRegulatoryCount(int expectedCount, IAnnotatedAlternateAllele altAllele)
        {
            var observedCount = altAllele?.RegulatoryRegions.Count;
            Assert.Equal(expectedCount, observedCount);
        }

        // ReSharper disable once UnusedParameter.Global
        public static void CheckJsonContains(string expectedString, IAnnotatedVariant annotatedVariant)
        {
            var json = annotatedVariant.ToString();
            Assert.Contains(expectedString, json);
        }

        // ReSharper disable once UnusedParameter.Global
        public static void CheckJsonDoesNotContain(string expectedString, IAnnotatedVariant annotatedVariant)
        {
            var json = annotatedVariant.ToString();
            Assert.DoesNotContain(expectedString, json);
        }

        // ReSharper disable once UnusedParameter.Global
        public static void CheckIntervalCount(int expectedCount, IAnnotatedVariant annotatedVariant)
        {
            var observedCount = annotatedVariant.SupplementaryIntervals.Count();
            Assert.Equal(expectedCount, observedCount);
        }

        // ReSharper disable once UnusedParameter.Global
        public static void CheckAlleleCoverage(IAnnotatedAlternateAllele altAllele, int expectedValue, string key)
        {
	        int observedValue;
			Assert.True(int.TryParse(typeof(IAnnotatedAlternateAllele).GetProperty(key).GetValue(altAllele).ToString(),out observedValue));
            Assert.Equal(expectedValue, observedValue);
        }

        // ReSharper disable once UnusedParameter.Global
        public static void CheckAlleleFrequencies(IAnnotatedAlternateAllele altAllele, double expectedValue, string key)
        {
            double observedValue;
            Assert.True(double.TryParse(typeof(IAnnotatedAlternateAllele).GetProperty(key).GetValue(altAllele).ToString(), out observedValue));
            Assert.Equal(expectedValue, observedValue);
        }
    }
}
