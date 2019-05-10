using System;
using System.IO;
using System.Linq;
using System.Text;
using IO;
using Xunit;

namespace UnitTests.IO
{
    public sealed class MD5StreamTests
    {
        [Fact]
        public void GetFileMetadata_AsExpected()
        {
            FileMetadata observed, cachedObserved;

            using (var memoryStream = new MemoryStream())
            using (var md5Stream    = new MD5Stream(memoryStream))
            {
                using (var writer = new StreamWriter(md5Stream, Encoding.ASCII))
                {
                    writer.Write("The quick brown fox jumps over the lazy dog");
                }

                observed       = md5Stream.GetFileMetadata();
                cachedObserved = md5Stream.GetFileMetadata();
            }

            byte[] expectedMd5 = StringToByteArray("9e107d9d372bb6826bd81d3542a419d6");
            const int expectedLength = 43;

            Assert.Equal(expectedMd5, observed.MD5);
            Assert.Equal(expectedLength, observed.Length);
            Assert.Equal(expectedMd5, cachedObserved.MD5);
            Assert.Equal(expectedLength, cachedObserved.Length);
        }

        [Fact]
        public void StreamTests_AsExpected()
        {
            using (var memoryStream = new MemoryStream())
            using (var md5Stream = new MD5Stream(memoryStream))
            {
                using (var writer = new StreamWriter(md5Stream, Encoding.ASCII))
                {
                    writer.Write("The quick brown fox jumps over the lazy dog");
                    md5Stream.Flush();
                }

                Assert.True(md5Stream.CanRead);
                Assert.True(md5Stream.CanWrite);
                Assert.True(md5Stream.CanSeek);
                Assert.Equal(43, md5Stream.Length);
                Assert.Equal(43, md5Stream.Position);
            }
        }

        [Fact]
        public void StreamTests_Throws_NotSupportedException()
        {
            using (var memoryStream = new MemoryStream())
            using (var md5Stream    = new MD5Stream(memoryStream))
            {
                var buffer = new byte[10];

                ThrowsNotSupportedException(md5Stream, stream => stream.Read(buffer, 0, buffer.Length));
                ThrowsNotSupportedException(md5Stream, stream => stream.Position = 5);
                ThrowsNotSupportedException(md5Stream, stream => stream.Seek(0, SeekOrigin.Begin));
                ThrowsNotSupportedException(md5Stream, stream => stream.SetLength(7));
            }
        }

        private static void ThrowsNotSupportedException<T>(MD5Stream md5Stream, Func<MD5Stream, T> exceptionFunc)
        {
            Assert.Throws<NotSupportedException>(delegate
            {
                // ReSharper disable once UnusedVariable
                exceptionFunc(md5Stream);
            });
        }

        private static void ThrowsNotSupportedException(MD5Stream lengthStream, Action<MD5Stream> exceptionAction)
        {
            Assert.Throws<NotSupportedException>(delegate
            {
                // ReSharper disable once UnusedVariable
                exceptionAction(lengthStream);
            });
        }

        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
    }
}
