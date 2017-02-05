using System.IO;
using System.Text;
using VariantAnnotation.FileHandling;
using Xunit;

namespace UnitTests.FileHandling
{
    public sealed class ExtendedBinaryIoTests
    {
        [Theory]
        [InlineData(int.MinValue)]
        [InlineData(0)]
        [InlineData(int.MaxValue)]
        public void Int(int expectedNum)
        {
            int observedNum;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    writer.WriteOpt(expectedNum);
                }

                ms.Seek(0, SeekOrigin.Begin);

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    observedNum = reader.ReadOptInt32();
                }
            }

            Assert.Equal(expectedNum, observedNum);
        }

        [Theory]
        [InlineData(long.MinValue)]
        [InlineData(0)]
        [InlineData(long.MaxValue)]
        public void Long(long expectedNum)
        {
            long observedNum;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    writer.WriteOpt(expectedNum);
                }

                ms.Seek(0, SeekOrigin.Begin);

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    observedNum = reader.ReadOptInt64();
                }
            }

            Assert.Equal(expectedNum, observedNum);
        }
    }
}