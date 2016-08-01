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
                using (var binaryWriter = new BinaryWriter(ms, Encoding.UTF8, true))
                {
                    var writer = new ExtendedBinaryWriter(binaryWriter);
                    writer.WriteInt(expectedNum);
                }

                ms.Seek(0, SeekOrigin.Begin);

                using (var binaryReader = new BinaryReader(ms))
                {
                    var reader = new ExtendedBinaryReader(binaryReader);
                    observedNum = reader.ReadInt();
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
                using (var binaryWriter = new BinaryWriter(ms, Encoding.UTF8, true))
                {
                    var writer = new ExtendedBinaryWriter(binaryWriter);
                    writer.WriteLong(expectedNum);
                }

                ms.Seek(0, SeekOrigin.Begin);

                using (var binaryReader = new BinaryReader(ms))
                {
                    var reader = new ExtendedBinaryReader(binaryReader);
                    observedNum = reader.ReadLong();
                }
            }

            Assert.Equal(expectedNum, observedNum);
        }
    }
}