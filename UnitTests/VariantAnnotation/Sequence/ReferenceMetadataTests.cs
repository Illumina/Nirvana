using System.IO;
using System.Text;
using IO;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.VariantAnnotation.Sequence
{
    public sealed class ReferenceMetadataTests
    {
        [Fact]
        public void Serialization()
        {
            var expectedMetadata = new ReferenceMetadata("chr1", "1");
            ReferenceMetadata observedMetadata;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    expectedMetadata.Write(writer);
                }

                ms.Position = 0;

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    observedMetadata = ReferenceMetadata.Read(reader);
                }
            }

            Assert.Equal(expectedMetadata.EnsemblName, observedMetadata.EnsemblName);
            Assert.Equal(expectedMetadata.UcscName, observedMetadata.UcscName);
        }
    }
}
