using System;
using System.IO;
using System.Text;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions.Transcript
{
    public sealed class CompactIdTests
    {
        [Fact]
        public void Convert_ENSG()
        {
            var id = CompactId.Convert("ENSG00000223972");
            Assert.Equal("ENSG00000223972", id.ToString());
        }

        [Fact]
        public void Convert_ENST()
        {
            var id = CompactId.Convert("ENST00000456328", 2);
            Assert.Equal("ENST00000456328.2", id.WithVersion);
        }

        [Fact]
        public void Convert_ENSP()
        {
            var id = CompactId.Convert("ENSP00000334393", 3);
            Assert.Equal("ENSP00000334393.3", id.WithVersion);
        }

        [Fact]
        public void Convert_ENSESTG()
        {
            var id = CompactId.Convert("ENSESTG00000027277");
            Assert.Equal("ENSESTG00000027277", id.WithVersion);
        }

        [Fact]
        public void Convert_ENSESTP()
        {
            var id = CompactId.Convert("ENSESTP00000068714", 1);
            Assert.Equal("ENSESTP00000068714.1", id.WithVersion);
        }

        [Fact]
        public void Convert_ENSR()
        {
            var id = CompactId.Convert("ENSR00001576074", 4);
            Assert.Equal("ENSR00001576074.4", id.WithVersion);
        }

        [Fact]
        public void Convert_CCDS()
        {
            var id = CompactId.Convert("CCDS30555", 1);
            Assert.Equal("CCDS30555.1", id.WithVersion);
        }

        [Fact]
        public void Convert_NR()
        {
            var id = CompactId.Convert("NR_074509", 1);
            Assert.Equal("NR_074509.1", id.WithVersion);
        }

        [Fact]
        public void Convert_NM()
        {
            var id = CompactId.Convert("NM_001029885", 1);
            Assert.Equal("NM_001029885.1", id.WithVersion);
        }

        [Fact]
        public void Convert_NP()
        {
            var id = CompactId.Convert("NP_001025056", 1);
            Assert.Equal("NP_001025056.1", id.WithVersion);
        }

        [Fact]
        public void Convert_XR()
        {
            var id = CompactId.Convert("XR_246629", 1);
            Assert.Equal("XR_246629.1", id.WithVersion);
        }

        [Fact]
        public void Convert_XM()
        {
            var id = CompactId.Convert("XM_005244723", 1);
            Assert.Equal("XM_005244723.1", id.WithVersion);
        }

        [Fact]
        public void Convert_XP()
        {
            var id = CompactId.Convert("XP_005244780", 1);
            Assert.Equal("XP_005244780.1", id.WithVersion);
        }

        [Fact]
        public void Convert_NullInput_ReturnsEmptyId()
        {
            var id = CompactId.Convert(null);
            Assert.True(id.IsEmpty());
            Assert.Null(id.WithVersion);

            id = CompactId.Convert(string.Empty);
            Assert.True(id.IsEmpty());
            Assert.Null(id.WithVersion);
        }

        [Fact]
        public void Convert_Unknown()
        {
            var id = CompactId.Convert("ABC123");
            Assert.True(id.IsEmpty());
            Assert.Null(id.WithVersion);
        }

        [Fact]
        public void Convert_OnlyNumbers()
        {
            var id = CompactId.Convert("268435455");
            Assert.Equal("268435455", id.WithoutVersion);
        }

        [Fact]
        public void Convert_OnlyNumbers_ThrowException_NumberTooLarge()
        {
            Assert.Throws<ArgumentOutOfRangeException>(delegate
            {
                // ReSharper disable once UnusedVariable
                var id = CompactId.Convert("268435456");
            });
        }

        [Fact]
        public void CompactId_IO_EndToEnd()
        {
            const string expectedResults = "ENSP00000334393.3";
            var id = CompactId.Convert("ENSP00000334393", 3);

            ICompactId observedId;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    id.Write(writer);
                }

                ms.Position = 0;

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    observedId = CompactId.Read(reader);
                }
            }

            Assert.NotNull(observedId);
            Assert.Equal(expectedResults, observedId.WithVersion);
        }
    }
}