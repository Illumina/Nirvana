using System;
using System.IO;
using System.Text;
using IO;
using Xunit;

namespace UnitTests.IO
{
    public sealed class LengthStreamTests
    {
        [Fact]
        public void Length_AsExpected()
        {
            long trueLength, modifiedLength;

            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream, Encoding.ASCII, 1024, true))
                {
                    writer.Write("The quick brown fox jumps over the lazy dog");
                }

                trueLength = memoryStream.Length;


                using (var lengthStream = new LengthStream(memoryStream, 3))
                {
                    modifiedLength = lengthStream.Length;
                }
            }

            Assert.Equal(43, trueLength);
            Assert.Equal(3, modifiedLength);
        }

        [Fact]
        public void StreamTests_AsExpected()
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new StreamWriter(memoryStream, Encoding.ASCII, 1024, true))
                {
                    writer.Write("The quick brown fox jumps over the lazy dog");
                }

                long expectedPosition = memoryStream.Position;
                memoryStream.Position = 0;

                using (var lengthStream = new LengthStream(memoryStream, 3))
                using (var reader = new StreamReader(lengthStream))
                {
                    reader.ReadToEnd();
                    Assert.True(lengthStream.CanRead);
                    Assert.True(lengthStream.CanWrite);
                    Assert.True(lengthStream.CanSeek);
                    Assert.Equal(3, lengthStream.Length);
                    Assert.True(lengthStream.Position >= expectedPosition);
                }
            }
        }

        [Fact]
        public void StreamTests_Throws_NotSupportedException()
        {
            using (var memoryStream = new MemoryStream())
            using (var lengthStream = new LengthStream(memoryStream, 3))
            {
                var buffer = new byte[10];

                ThrowsNotSupportedException(lengthStream, stream => stream.Position = 5);
                ThrowsNotSupportedException(lengthStream, stream => stream.Seek(0, SeekOrigin.Begin));
                ThrowsNotSupportedException(lengthStream, stream => stream.Write(buffer, 0, buffer.Length));
                ThrowsNotSupportedException(lengthStream, stream => stream.SetLength(7));
                ThrowsNotSupportedException(lengthStream, stream => stream.Flush());
            }
        }

        private static void ThrowsNotSupportedException<T>(LengthStream lengthStream, Func<LengthStream, T> exceptionFunc)
        {
            Assert.Throws<NotSupportedException>(delegate
            {
                // ReSharper disable once UnusedVariable
                exceptionFunc(lengthStream);
            });
        }

        private static void ThrowsNotSupportedException(LengthStream lengthStream, Action<LengthStream> exceptionAction)
        {
            Assert.Throws<NotSupportedException>(delegate
            {
                // ReSharper disable once UnusedVariable
                exceptionAction(lengthStream);
            });
        }
    }
}
