using System;
using System.IO;
using System.Text;
using VariantAnnotation.IO;
using Xunit;

namespace UnitTests.VariantAnnotation.IO
{
    public sealed class ExtendedBinaryReaderTests
    {
        [Theory]
        [InlineData(3)]
        [InlineData(0)]
        [InlineData(-2)]
        public void ReadOptInt32_HandleSmallIntegers(int expectedInteger)
        {
            int observedInteger = GetObservedInteger(expectedInteger);
            Assert.Equal(expectedInteger, observedInteger);
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(int.MinValue)]
        public void ReadOptInt32_HandleExtremeIntegers(int expectedInteger)
        {
            int observedInteger = GetObservedInteger(expectedInteger);
            Assert.Equal(expectedInteger, observedInteger);
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
        public void ReadOptUInt16_HandleExtremeIntegers(ushort expectedInteger)
        {
            ushort observedInteger = GetObservedShort(expectedInteger);
            Assert.Equal(expectedInteger, observedInteger);
        }

        [Theory]
        [InlineData(3)]
        [InlineData(0)]
        [InlineData(-2)]
        public void ReadOptInt64_HandleSmallIntegers(long expectedLong)
        {
            long observedLong = GetObservedLong(expectedLong);
            Assert.Equal(expectedLong, observedLong);
        }

        [Theory]
        [InlineData(long.MaxValue)]
        [InlineData(long.MinValue)]
        public void ReadOptInt64_HandleExtremeIntegers(long expectedLong)
        {
            long observedLong = GetObservedLong(expectedLong);
            Assert.Equal(expectedLong, observedLong);
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
            string observedResult;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    writer.WriteOptAscii(null);
                }

                ms.Position = 0;

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    observedResult = reader.ReadAsciiString();
                }
            }

            Assert.Null(observedResult);
        }

        [Fact]
        public void ExtendedBinaryReader_EndToEnd_DoNotLeaveOpen()
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

        [Fact]
        public void ReadOptArray_ThreeElements()
        {
            var expectedStrings = new[] { "Huey", "Duey", "Louie" };
            string[] observedStrings;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    writer.WriteOptArray(expectedStrings, writer.WriteOptAscii);
                }

                ms.Position = 0;

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    observedStrings = reader.ReadOptArray(reader.ReadAsciiString);
                }
            }

            Assert.Equal(expectedStrings, observedStrings);
        }

        [Fact]
        public void ReadOptArray_NoElements()
        {
            string[] observedStrings;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    writer.WriteOptArray(null as string[], writer.WriteOptAscii);
                }

                ms.Position = 0;

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    observedStrings = reader.ReadOptArray(reader.ReadAsciiString);
                }
            }

            Assert.Null(observedStrings);
        }

        private static ushort GetObservedShort(ushort expectedShort)
        {
            ushort observedShort;

            using (var memoryStream = GetMemoryStream(expectedShort))
            using (var reader = new ExtendedBinaryReader(memoryStream))
            {
                observedShort = reader.ReadOptUInt16();
            }

            return observedShort;
        }

        private static int GetObservedInteger(int expectedInteger)
        {
            int observedInteger;

            using (var memoryStream = GetMemoryStream(expectedInteger))
            using (var reader = new ExtendedBinaryReader(memoryStream))
            {
                observedInteger = reader.ReadOptInt32();
            }

            return observedInteger;
        }

        private static long GetObservedLong(long expectedLong)
        {
            long observedLong;

            using (var memoryStream = GetMemoryStream(expectedLong))
            using (var reader = new ExtendedBinaryReader(memoryStream))
            {
                observedLong = reader.ReadOptInt64();
            }

            return observedLong;
        }

        private static Stream GetMemoryStream(ushort num)
        {
            var memoryStream = new MemoryStream();

            using (var writer = new ExtendedBinaryWriter(memoryStream, Encoding.UTF8, true))
            {
                writer.WriteOpt(num);
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

        private static Stream GetMemoryStream(int num)
        {
            var memoryStream = new MemoryStream();

            using (var writer = new ExtendedBinaryWriter(memoryStream, Encoding.UTF8, true))
            {
                writer.WriteOpt(num);
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

        private static Stream GetMemoryStream(long num)
        {
            var memoryStream = new MemoryStream();

            using (var writer = new ExtendedBinaryWriter(memoryStream, Encoding.UTF8, true))
            {
                writer.WriteOpt(num);
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
