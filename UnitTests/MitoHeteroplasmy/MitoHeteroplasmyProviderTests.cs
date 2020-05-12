using MitoHeteroplasmy;
using UnitTests.TestUtilities;
using Xunit;

namespace UnitTests.MitoHeteroplasmy
{
    public sealed class MitoHeteroplasmyProviderTests
    {
        [Fact]
        public void GetVrfPercentiles_AsExpected()
        {
            var provider = new MitoHeteroplasmyProvider();
            provider.Add(1, "C", new []{0.123, 0.200, 0.301}, new []{1, 3, 4});
            provider.Add(1, "G", new[] { 0.101, 0.201}, new[] { 1, 2});

            var chrom = ChromosomeUtilities.ChrM;
            var position = 1;
            var altAlleles = new[] {"C", "T", "ACC"};
            
            var percentilesSample1 = provider.GetVrfPercentiles("0|1", chrom, position, altAlleles, new[] { 0.2});
            var percentilesSample2 = provider.GetVrfPercentiles("0/2", chrom, position, altAlleles, new[] { 0.12});
            var percentilesSample3 = provider.GetVrfPercentiles("0/3", chrom, position, altAlleles, new[] { 0.02});

            Assert.Equal(new double?[]{0.5}, percentilesSample1);
            Assert.Null(percentilesSample2);
            Assert.Null(percentilesSample3);
        }
    }
}
