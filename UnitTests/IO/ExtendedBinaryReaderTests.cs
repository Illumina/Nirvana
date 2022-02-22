using System;
using System.IO;
using System.Text;
using IO;
using Xunit;

namespace UnitTests.IO
{
    public sealed class ExtendedBinaryReaderTests
    {
        [Theory]
        [InlineData(3)]
        [InlineData(0)]
        [InlineData(-2)]
        public void ReadOptInt32_HandleSmallIntegers(int expectedValue)
        {
            int observedValue = GetObservedValue(writer => writer.WriteOpt(expectedValue), reader => reader.ReadOptInt32());
            Assert.Equal(expectedValue, observedValue);
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void ReadOptInt32_HandleExtremeIntegers(int expectedValue)
        {
            int observedValue = GetObservedValue(writer => writer.WriteOpt(expectedValue), reader => reader.ReadOptInt32());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadOptInt32_ThrowException_WithCorruptData()
        {
            Assert.Throws<FormatException>(delegate
            {
                using (var ms = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(ms, Encoding.UTF8, true))
                    {
                        const ulong corruptInt = 0xffffffffffffffff;
                        writer.Write(corruptInt);
                    }

                    ms.Position = 0;

                    using (var reader = new ExtendedBinaryReader(ms))
                    {
                        reader.ReadOptInt32();
                    }
                }
            });
        }

        [Theory]
        [InlineData(ushort.MaxValue)]
        [InlineData(ushort.MinValue)]
        public void ReadOptUInt16_HandleExtremeIntegers(ushort expectedValue)
        {
            ushort observedValue = GetObservedValue(writer => writer.WriteOpt(expectedValue), reader => reader.ReadOptUInt16());
            Assert.Equal(expectedValue, observedValue);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(0)]
        [InlineData(-2)]
        public void ReadOptInt64_HandleSmallIntegers(long expectedValue)
        {
            long observedValue = GetObservedValue(writer => writer.WriteOpt(expectedValue), reader => reader.ReadOptInt64());
            Assert.Equal(expectedValue, observedValue);
        }

        [Theory]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue)]
        public void ReadOptInt64_HandleExtremeIntegers(long expectedValue)
        {
            long observedValue = GetObservedValue(writer => writer.WriteOpt(expectedValue), reader => reader.ReadOptInt64());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadOptInt64_ThrowException_WithCorruptData()
        {
            Assert.Throws<FormatException>(delegate
            {
                using (var ms = new MemoryStream())
                {
                    using (var writer = new BinaryWriter(ms, Encoding.UTF8, true))
                    {
                        const ulong corruptData = 0xffffffffffffffff;
                        writer.Write(corruptData);
                        writer.Write(corruptData);
                    }

                    ms.Position = 0;

                    using (var reader = new ExtendedBinaryReader(ms))
                    {
                        reader.ReadOptInt64();
                    }
                }
            });
        }

        [Fact]
        public void ReadAsciiString_NullString()
        {
            string observedValue = GetObservedValue(writer => writer.WriteOptAscii(null), reader => reader.ReadAsciiString());
            Assert.Null(observedValue);
        }

        [Fact]
        public void BufferedBinaryReader_EndToEnd_DoNotLeaveOpen()
        {
            const int expectedResult = 5;
            int observedResult;
            byte[] data;

            using (var ms = new MemoryStream())
            using (var writer = new ExtendedBinaryWriter(ms))
            {
                writer.Write(expectedResult);
                data = ms.ToArray();
            }

            using (var ms = new MemoryStream(data))
            using (var reader = new ExtendedBinaryReader(ms))
            {
                observedResult = reader.ReadInt32();
            }

            Assert.Equal(expectedResult, observedResult);
        }

        private static T GetObservedValue<T>(Action<ExtendedBinaryWriter> writeMethod, Func<ExtendedBinaryReader, T> readMethod)
        {
            T observedValue;
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(memoryStream, Encoding.UTF8, true))
                {
                    writeMethod(writer);
                }

                memoryStream.Position = 0;

                using (var reader = new ExtendedBinaryReader(memoryStream))
                {
                    observedValue = readMethod(reader);
                }
            }

            return observedValue;
        }
    }
}
