using System;
using System.IO;
using System.Text;
using Compression.FileHandling;
using Xunit;

namespace UnitTests.Compression.FileHandling
{
    public sealed class PeekStreamTests
    {
        private readonly PeekStream _peekStream;

        public PeekStreamTests()
        {
            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true)) writer.WriteLine("testing");
            memoryStream.Position = 0;
            _peekStream = new PeekStream(memoryStream);
        }

        [Fact]
        public void Length()
        {

        }

        [Fact]
        public void ReadByte()
        {

        }

        [Fact]
        public void Write()
        {

        }

        [Fact]
        public void WriteByte()
        {

        }

        [Fact]
        public void SetLength()
        {

        }

        [Fact]
        public void Should_ThrowException_Constructor_InvalidBufferSize()
        {
            var memoryStream = new MemoryStream(new byte[50]);

            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var peekStream = new PeekStream(memoryStream, 0);
            });

            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var peekStream = new PeekStream(memoryStream, -1);
            });
        }

        [Fact]
        public void Should_ThrowException_Read_InvalidArguments()
        {
            var buffer = new byte[10];

            Assert.Throws<ArgumentNullException>(delegate
            {
                _peekStream.Read(null, 0, 10);
            });

            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                _peekStream.Read(buffer, -1, 1);
            });

            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                _peekStream.Read(buffer, 0, -1);
            });

            Assert.Throws<ArgumentException>(delegate
            {
                _peekStream.Read(buffer, 8, 5);
            });
        }
    }
}
