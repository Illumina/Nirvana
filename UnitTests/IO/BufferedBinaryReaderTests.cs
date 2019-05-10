using System;
using System.IO;
using System.Text;
using IO;
using Xunit;

namespace UnitTests.IO
{
    public sealed class BufferedBinaryReaderTests
    {
        [Fact]
        public void ReadBoolean()
        {
            const bool expectedValue = true;
            bool observedValue = GetObservedValue(writer => writer.Write(expectedValue), reader => reader.ReadBoolean());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadByte()
        {
            const byte expectedValue = byte.MaxValue;
            byte observedValue = GetObservedValue(writer => writer.Write(expectedValue), reader => reader.ReadByte());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadBytes()
        {
            byte[] expectedValue = Encoding.ASCII.GetBytes("Hello world");
            byte[] observedValue = GetObservedValue(writer => writer.Write(expectedValue), reader => reader.ReadBytes(expectedValue.Length));
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadInt16()
        {
            const short expectedValue = short.MaxValue;
            short observedValue = GetObservedValue(writer => writer.Write(expectedValue), reader => reader.ReadInt16());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadInt32()
        {
            const int expectedValue = int.MaxValue;
            int observedValue = GetObservedValue(writer => writer.Write(expectedValue), reader => reader.ReadInt32());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadUInt16()
        {
            const ushort expectedValue = ushort.MaxValue;
            ushort observedValue = GetObservedValue(writer => writer.Write(expectedValue), reader => reader.ReadUInt16());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadUInt32()
        {
            const uint expectedValue = uint.MaxValue;
            uint observedValue = GetObservedValue(writer => writer.Write(expectedValue), reader => reader.ReadUInt32());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadInt64()
        {
            const long expectedValue = long.MaxValue;
            long observedValue = GetObservedValue(writer => writer.Write(expectedValue), reader => reader.ReadInt64());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadUInt64()
        {
            const ulong expectedValue = ulong.MaxValue;
            ulong observedValue = GetObservedValue(writer => writer.Write(expectedValue), reader => reader.ReadUInt64());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadDouble()
        {
            const double expectedValue = double.MaxValue;
            double observedValue = GetObservedValue(writer => writer.Write(expectedValue), reader => reader.ReadDouble());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadString()
        {
            const string expectedValue = "ひらがな";
            string observedValue = GetObservedValue(writer => writer.Write(expectedValue), reader => reader.ReadString());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void ReadAsciiString()
        {
            const string expectedValue = "Hello world";
            string observedValue = GetObservedValue(writer => writer.Write(expectedValue), reader => reader.ReadAsciiString());
            Assert.Equal(expectedValue, observedValue);
        }

        [Theory]
        [InlineData(ushort.MaxValue)]
        [InlineData(3)]
        [InlineData(0)]
        public void ReadOptUInt16_HandleExtremeIntegers(ushort expectedValue)
        {
            ushort observedValue = GetObservedValue(writer => writer.WriteOpt(expectedValue), reader => reader.ReadOptUInt16());
            Assert.Equal(expectedValue, observedValue);
        }

        [Theory]
        [InlineData(int.MaxValue)]
        [InlineData(-1)]
        [InlineData(int.MinValue)]
        public void ReadOptInt32_HandleExtremeIntegers(int expectedValue)
        {
            int observedValue = GetObservedValue(writer => writer.WriteOpt(expectedValue), reader => reader.ReadOptInt32());
            Assert.Equal(expectedValue, observedValue);
        }

        [Theory]
        [InlineData(long.MaxValue)]
        [InlineData(-1)]
        [InlineData(long.MinValue)]
        public void ReadOptInt64_HandleExtremeIntegers(long expectedValue)
        {
            long observedValue = GetObservedValue(writer => writer.WriteOpt(expectedValue), reader => reader.ReadOptInt64());
            Assert.Equal(expectedValue, observedValue);
        }

        [Fact]
        public void Reset()
        {
            const string s = "The quick brown fox jumped over the lazy dog.";
            string expectedValue = s.Substring(4);
            string observedValue;
            long observedBufferPosition;
            byte observedByte;
            
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(memoryStream, Encoding.UTF8, true))
                {
                    writer.Write(Encoding.ASCII.GetBytes(s));
                }

                memoryStream.Position = 0;

                using (var reader = new BufferedBinaryReader(memoryStream))
                {
                    reader.ReadInt32();
                    observedBufferPosition = reader.BufferPosition;

                    reader.BufferPosition = 2;
                    observedByte = reader.ReadByte();

                    memoryStream.Position = 4;
                    reader.Reset();

                    var bytes = reader.ReadBytes(s.Length - 4);
                    observedValue = Encoding.ASCII.GetString(bytes);
                }
            }

            Assert.Equal(expectedValue, observedValue);
            Assert.Equal((byte)'e', observedByte);
            Assert.Equal(4, observedBufferPosition);
        }

        private static T GetObservedValue<T>(Action<ExtendedBinaryWriter> writeMethod, Func<BufferedBinaryReader, T> readMethod)
        {
            T observedValue;
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(memoryStream, Encoding.UTF8, true))
                {
                    writeMethod(writer);
                }

                memoryStream.Position = 0;

                using (var reader = new BufferedBinaryReader(memoryStream))
                {
                    observedValue = readMethod(reader);
                }
            }

            return observedValue;
        }
    }
}
