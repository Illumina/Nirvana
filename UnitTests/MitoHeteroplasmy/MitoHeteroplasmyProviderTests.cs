using MitoHeteroplasmy;
using UnitTests.TestUtilities;
using Variants;
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

            var position = 1;

            IVariant[] variants          = {
                new Variant(ChromosomeUtilities.ChrM, position, position, "N", "C", VariantType.SNV,
                    null, false, false, false, null, AnnotationBehavior.SmallVariants, false),
                new Variant(ChromosomeUtilities.ChrM, position, position, "N", "G", VariantType.SNV,
                    null, false, false, false, null, AnnotationBehavior.SmallVariants, false),
                new Variant(ChromosomeUtilities.ChrM, position, position, "N", "T", VariantType.SNV,
                    null, false, false, false, null, AnnotationBehavior.SmallVariants, false)
            };
            var percentilesSample = provider.GetVrfPercentiles(variants, new[] { 0.2, 0.15, 0.02 });

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

            var position = 1;
            
            IVariant[] variants = {
                new Variant(ChromosomeUtilities.ChrM, position, position, "N", "T", VariantType.SNV,
                    null, false, false, false, null, AnnotationBehavior.SmallVariants, false),
                new Variant(ChromosomeUtilities.ChrM, position, position, "N", "ACC", VariantType.insertion,
                    null, false, false, false, null, AnnotationBehavior.SmallVariants, false)
            };
            var percentiles = provider.GetVrfPercentiles(variants, new[] { 0.24, 0.12 });

            Assert.Null(percentiles);
        }

        [Fact]
        public void GetVrfPercentiles_ProperRounding()
        {
            var provider = GetProvider();
            var position = 2;
            
            IVariant[] variants = {
                new Variant(ChromosomeUtilities.ChrM, position, position, "N", "T", VariantType.SNV,
                    null, false, false, false, null, AnnotationBehavior.SmallVariants, false)
            };


            var percentilesSample = provider.GetVrfPercentiles(variants, new[] { 0.0014 });
            
            Assert.Single(percentilesSample);
            Assert.True(percentilesSample[0].HasValue);
            Assert.Equal(52.22, percentilesSample[0].Value, 2);
        }
        [Fact]
        public void GetVrfPercentiles_zero()
        {
            var provider   = GetProvider();
            var position   = 1;
            
            IVariant[] variants = {
                new Variant(ChromosomeUtilities.ChrM, position, position, "N", "G", VariantType.SNV,
                    null, false, false, false, null, AnnotationBehavior.SmallVariants, false)
            };

            var percentilesSample = provider.GetVrfPercentiles(variants, new[] { 0.0034 });
            
            Assert.Single(percentilesSample);
            Assert.True(percentilesSample[0].HasValue);
            Assert.Equal(0, percentilesSample[0].Value, 2);
        }
        
        [Fact]
        public void GetVrfPercentiles_100()
        {
            var provider   = GetProvider();
            var position   = 2;

            IVariant[] variants = {
                new Variant(ChromosomeUtilities.ChrM, position, position, "N", "T", VariantType.SNV,
                    null, false, false, false, null, AnnotationBehavior.SmallVariants, false)
            };
            
            var percentilesSample = provider.GetVrfPercentiles(variants, new[] { 0.0034 });
            
            Assert.Single(percentilesSample);
            Assert.True(percentilesSample[0].HasValue);
            Assert.Equal(100, percentilesSample[0].Value, 2);
        }

        [Fact]
        public void CapVrf()
        {
            var provider = new MitoHeteroplasmyProvider();
            provider.Add(750, "G", new[] { 0.0,0.001,0.002,0.991,0.994,0.995,0.996,0.997,0.998,0.999 }, new[] { 24,4,2,3,2,1,1,4,3,2460});
            var position   = 750;
            
            IVariant[] variants = {
                new Variant(ChromosomeUtilities.ChrM, position, position, "N", "G", VariantType.SNV,
                    null, false, false, false, null, AnnotationBehavior.SmallVariants, false)
            };
            var percentilesSample = provider.GetVrfPercentiles(variants, new[] { 1.0 });
            
            Assert.Single(percentilesSample);
            Assert.True(percentilesSample[0].HasValue);
            Assert.Equal(1.76, percentilesSample[0].Value, 2);
        }
    }
}
