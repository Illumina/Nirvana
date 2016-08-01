using System.Linq;
using System.Reflection;
using VariantAnnotation.Interface;
using Xunit;

namespace UnitTests.Utilities
{
    public static class AssertUtilities
    {
        public static void CheckAlleleCount(int expectedCount, IAnnotatedVariant annotatedVariant)
        {
            var observedCount = annotatedVariant.AnnotatedAlternateAlleles.Count();
            Assert.Equal(expectedCount, observedCount);
        }

        public static void CheckSampleCount(int expectedCount, IAnnotatedVariant annotatedVariant)
        {
            var observedCount = annotatedVariant.AnnotatedSamples.Count();
            Assert.Equal(expectedCount, observedCount);
        }

        public static void CheckEnsemblTranscriptCount(int expectedCount, IAnnotatedAlternateAllele altAllele)
        {
            var observedCount = altAllele?.EnsemblTranscripts.Count();
            Assert.Equal(expectedCount, observedCount);
        }

        public static void CheckRefSeqTranscriptCount(int expectedCount, IAnnotatedAlternateAllele altAllele)
        {
            var observedCount = altAllele?.RefSeqTranscripts.Count();
            Assert.Equal(expectedCount, observedCount);
        }

        public static void CheckRegulatoryCount(int expectedCount, IAnnotatedAlternateAllele altAllele)
        {
            var observedCount = altAllele?.RegulatoryRegions.Count();
            Assert.Equal(expectedCount, observedCount);
        }

        public static void CheckJsonContains(string expectedString, IAnnotatedVariant annotatedVariant)
        {
            var json = annotatedVariant.ToString();
            Assert.Contains(expectedString, json);
        }

        public static void CheckJsonDoesNotContain(string expectedString, IAnnotatedVariant annotatedVariant)
        {
            var json = annotatedVariant.ToString();
            Assert.DoesNotContain(expectedString, json);
        }

        public static void CheckIntervalCount(int expectedCount, IAnnotatedVariant annotatedVariant)
        {
            var observedCount = annotatedVariant.SupplementaryIntervals.Count();
            Assert.Equal(expectedCount, observedCount);
        }

        public static void CheckAlleleCoverage(IAnnotatedAlternateAllele altAllele, int expectedValue, string key)
        {
	        int observedValue;
			Assert.True(int.TryParse(typeof(IAnnotatedAlternateAllele).GetTypeInfo().GetProperty(key).GetValue(altAllele).ToString(),out observedValue));
            Assert.Equal(expectedValue, observedValue);
        }

        public static void CheckAlleleFrequencies(IAnnotatedAlternateAllele altAllele, double expectedValue, string key)
        {
            double observedValue;
            Assert.True(double.TryParse(typeof(IAnnotatedAlternateAllele).GetTypeInfo().GetProperty(key).GetValue(altAllele).ToString(), out observedValue));
            Assert.Equal(expectedValue, observedValue);
        }
    }
}
