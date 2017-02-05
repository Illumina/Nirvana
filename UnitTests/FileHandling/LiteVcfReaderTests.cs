using UnitTests.Utilities;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.FileHandling
{
    public sealed class LiteVcfReaderTests
    {
        [Fact]
        public void CopyNumberExtractionNormal()
        {
            using (var reader = new LiteVcfReader(ResourceUtilities.GetReadStream(Resources.InputFiles("canvas.vcf"))))
            {
                Assert.Equal(9, VcfCommon.GenotypeIndex);
				Assert.Equal("SAMPLE", reader.SampleNames[0]);
            }
        }

		[Fact]
		public void CopyNumberExtractionTumor()
        {
            using (var reader = new LiteVcfReader(ResourceUtilities.GetReadStream(Resources.InputFiles("tumor.vcf"))))
            {
                Assert.Equal(9, VcfCommon.GenotypeIndex);
                Assert.Equal("TUMOR", reader.SampleNames[1]);
            }
        }
    }
}
