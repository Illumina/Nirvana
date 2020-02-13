using System.IO;
using System.Linq;
using Moq;
using SAUtils.MitoHeteroplasmy;
using UnitTests.TestUtilities;
using VariantAnnotation.Interface.Providers;
using Xunit;

namespace UnitTests.SAUtils.MitoHeteroplasmy
{
    public sealed class MitoHeteroplasmyTests
    {
        private static ISequenceProvider GetSequenceProvider()
        {
            var mockProvider = new Mock<ISequenceProvider>();
            mockProvider.SetupGet(x => x.RefNameToChromosome).Returns(ChromosomeUtilities.RefNameToChromosome);
            mockProvider.SetupGet(x => x.RefIndexToChromosome).Returns(ChromosomeUtilities.RefIndexToChromosome);
            return mockProvider.Object;
        }
        private static Stream GetStream()
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);

            writer.WriteLine("## num_samples=246");
            writer.WriteLine("MT\t4\t5\t{}");// 0 items
            writer.WriteLine("MT\t5\t6\t{\"C:A\":{\"ad\":[1],\"allele_type\":\"alt\",\"vrf\":[0.006329113924050633],\"vrf_stats\":{\"kurtosis\":241.00408163265314,\"max\":0.0063291139240506328,\"mean\":2.5728105382319646e-05,\"min\":0.0,\"nobs\":246,\"skewness\":15.588588185998534,\"stdev\":0.00040352956522996095,\"variance\":1.6283611001468132e-07}}}");// 1 item
            writer.WriteLine("MT\t7\t8\t{\"G:A\":{\"ad\":[1,1,1,1],\"allele_type\":\"alt\",\"vrf\":[0.003205128205128205,0.002232142857142857,0.0037593984962406013,0.00273224043715847],\"vrf_stats\":{\"kurtosis\":64.96245848503843,\"max\":0.0037593984962406013,\"mean\":4.849150404743957e-05,\"min\":0.0,\"nobs\":246,\"skewness\":8.05974448165666,\"stdev\":0.00038478763089843624,\"variance\":1.4806152089243121e-07}},\"G:C\":{\"ad\":[1,1],\"allele_type\":\"alt\",\"vrf\":[0.0024813895781637717,0.004291845493562232],\"vrf_stats\":{\"kurtosis\":148.72822661048482,\"max\":0.0042918454935622317,\"mean\":2.7533475901325216e-05,\"min\":0.0,\"nobs\":246,\"skewness\":12.019856436922753,\"stdev\":0.00031552186298069995,\"variance\":9.9554046018811583e-08}},\"G:T\":{\"ad\":[1,1,1,1],\"allele_type\":\"alt\",\"vrf\":[0.0027624309392265192,0.002680965147453083,0.003236245954692557,0.0030211480362537764],\"vrf_stats\":{\"kurtosis\":57.92357810503749,\"max\":0.0032362459546925568,\"mean\":4.7564187307422503e-05,\"min\":0.0,\"nobs\":246,\"skewness\":7.717570354191911,\"stdev\":0.0003717728271743761,\"variance\":1.3821503502522855e-07}}}");//3 items

            writer.Flush();

            stream.Position = 0;
            return stream;
        }

        [Fact]
        public void ParseItems()
        {
            using (var parser = new MitoHeteroplasmyParser(GetStream(), GetSequenceProvider()))
            {
                var items = parser.GetItems().ToList();

                Assert.Equal(4, items.Count);

                Assert.Equal("\"vrfMean\":0.000026,\"vrfStdev\":0.000404", items[0].GetJsonString());
                
            }
        }

        [Fact]
        public void DeserializeStats()
        {
            var input = "{\"G:A\":{\"ad\":[1,1,1,1],\"allele_type\":\"alt\",\"vrf\":[0.003205128205128205,0.002232142857142857,0.0037593984962406013,0.00273224043715847],\"vrf_stats\":{\"kurtosis\":64.96245848503843,\"max\":0.0037593984962406013,\"mean\":4.849150404743957e-05,\"min\":0.0,\"nobs\":246,\"skewness\":8.05974448165666,\"stdev\":0.00038478763089843624,\"variance\":1.4806152089243121e-07}},\"G:C\":{\"ad\":[1,1],\"allele_type\":\"alt\",\"vrf\":[0.0024813895781637717,0.004291845493562232],\"vrf_stats\":{\"kurtosis\":148.72822661048482,\"max\":0.0042918454935622317,\"mean\":2.7533475901325216e-05,\"min\":0.0,\"nobs\":246,\"skewness\":12.019856436922753,\"stdev\":0.00031552186298069995,\"variance\":9.9554046018811583e-08}},\"G:T\":{\"ad\":[1,1,1,1],\"allele_type\":\"alt\",\"vrf\":[0.0027624309392265192,0.002680965147453083,0.003236245954692557,0.0030211480362537764],\"vrf_stats\":{\"kurtosis\":57.92357810503749,\"max\":0.0032362459546925568,\"mean\":4.7564187307422503e-05,\"min\":0.0,\"nobs\":246,\"skewness\":7.717570354191911,\"stdev\":0.0003717728271743761,\"variance\":1.3821503502522855e-07}}}";

            var stats = MitoHeteroplasmyParser.DeserializeStats(input);

            Assert.NotNull(stats.G_A);
            Assert.Equal(0.003205128205128205, stats.G_A.vrf[0]);
        }
    }
}