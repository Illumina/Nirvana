using Vcf.Sample;
using Xunit;

namespace UnitTests.Vcf.Samples
{
    public sealed class VariantFrequencyTests
    {
        [Fact]
        public void GetVariantFrequencies_VF_OneAltAllele()
        {
            double[] expectedResults = { 0.75 };
            double[] observedResults = VariantFrequency.GetVariantFrequencies(0.75, null, 1);
            Evaluate(expectedResults, observedResults);
        }

        [Fact]
        public void GetVariantFrequencies_VF_MultipleAltAlleles_ReturnNull()
        {
            double[] observedResults = VariantFrequency.GetVariantFrequencies(0.75, null, 2);
            Assert.Null(observedResults);
        }

        [Fact]
        public void GetVariantFrequencies_OverrideAD_UseVF()
        {
            double[] expectedResults = { 0.75 };
            double[] observedResults = VariantFrequency.GetVariantFrequencies(0.75, new[] { 10, 20 }, 1);
            Evaluate(expectedResults, observedResults);
        }

        [Fact]
        public void GetVariantFrequencies_AD_WrongAlleleCount_ReturnNull()
        {
            double[] observedResults = VariantFrequency.GetVariantFrequencies(null, new[] { 10, 20 }, 3);
            Assert.Null(observedResults);
        }

        [Fact]
        public void GetVariantFrequencies_AD()
        {
            double[] expectedResults = { 0.35, 0.4 };
            double[] observedResults = VariantFrequency.GetVariantFrequencies(null, new[] { 5, 7, 8 }, 2);
            Evaluate(expectedResults, observedResults);
        }

        [Fact]
        public void GetVariantFrequencies_AD_ZeroSumAlleleCount_ReturnZeros()
        {
            double[] expectedResults = { 0.0, 0.0 };
            double[] observedResults = VariantFrequency.GetVariantFrequencies(null, new[] { 0, 0, 0 }, 2);
            Evaluate(expectedResults, observedResults);
        }

        private void Evaluate(double[] expectedResults, double[] observedResults)
        {
            if (expectedResults == null || observedResults == null)
            {
                Assert.Equal(expectedResults, observedResults);
                return;
            }

            Assert.Equal(expectedResults.Length, observedResults.Length);

            for (int i = 0; i < expectedResults.Length; i++)
            {
                Assert.Equal(expectedResults[i], observedResults[i], 10);
            }
        }
    }
}
