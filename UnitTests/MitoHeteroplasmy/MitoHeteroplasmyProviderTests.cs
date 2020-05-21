using MitoHeteroplasmy;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.MitoHeteroplasmy
{
    public sealed class MitoHeteroplasmyProviderTests
    {

        private static MitoHeteroplasmyProvider GetProvider()
        {
            var provider = new MitoHeteroplasmyProvider();
            provider.Add(1, "C", new[] { 0.123, 0.200, 0.301 }, new[] { 1, 3, 4 });
            provider.Add(1, "G", new[] { 0.101, 0.201 }, new[] { 1, 2 });
            provider.Add(2, "T", new[] { 0, 0.001, 0.002, 0.003 }, new[] { 134, 1111, 936, 203 });

            return provider;
        }

        [Fact]
        public void GetVrfPercentiles_AsExpected()
        {
            var provider = GetProvider();

            var chrom = ChromosomeUtilities.ChrM;
            var position = 1;
            var altAlleles = new[] { "C", "G", "T" };

            var percentilesSample = provider.GetVrfPercentiles(chrom, position, altAlleles, new[] { 0.2, 0.15, 0.02 });

            Assert.Equal(3, percentilesSample.Length);
            Assert.True(percentilesSample[0].HasValue);
            Assert.Equal(100 / 8.0, percentilesSample[0].Value, 3);
            Assert.True(percentilesSample[1].HasValue);
            Assert.Equal(100 / 3.0, percentilesSample[1].Value, 3);
            Assert.Null(percentilesSample[2]);
        }

        [Fact]
        public void GetVrfPercentiles_NullIfNoValue()
        {
            var provider = GetProvider();

            var chrom = ChromosomeUtilities.ChrM;
            var position = 1;
            var altAlleles = new[] { "T", "ACC" };

            var percentiles = provider.GetVrfPercentiles(chrom, position, altAlleles, new[] { 0.24, 0.12 });

            Assert.Null(percentiles);
        }

        [Fact]
        public void GetVrfPercentiles_ProperRounding()
        {
            var provider = GetProvider();

            var chrom      = ChromosomeUtilities.ChrM;
            var position   = 2;
            var altAlleles = new[] { "T" };

            var percentilesSample = provider.GetVrfPercentiles(chrom, position, altAlleles, new[] { 0.0014 });
            
            Assert.Single(percentilesSample);
            Assert.True(percentilesSample[0].HasValue);
            Assert.Equal(5.62, percentilesSample[0].Value, 2);
        }
    }
}
