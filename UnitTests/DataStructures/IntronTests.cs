using System;
using System.IO;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
    public sealed class IntronTests
    {
        [Fact]
        public void HashCode()
        {
            var originalIntron = new SimpleInterval(100, 200);

            const int expectedHashCode = 172;
            var observedHashCode = originalIntron.GetHashCode();

            Assert.Equal(expectedHashCode, observedHashCode);
        }

        [Fact]
        public void SerializationAndEquality()
        {
            var intronPath = Path.GetTempPath() + Guid.NewGuid() + ".dat";
            var originalIntron = new SimpleInterval(100, 200);

            // serialize the intron
            using (var writer = new ExtendedBinaryWriter(FileUtilities.GetCreateStream(intronPath)))
            {
                originalIntron.Write(writer);
            }

            // deserialize the intron
            SimpleInterval newIntron;
            using (var reader = new ExtendedBinaryReader(FileUtilities.GetReadStream(intronPath)))
            {
                newIntron = SimpleInterval.Read(reader);
            }

            Assert.Equal(originalIntron, newIntron);

            // test the equality operations
            Assert.True(originalIntron.Equals(newIntron));

            SimpleInterval nullIntron = null;

            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.False(originalIntron.Equals(nullIntron));

            object nullObject = null;
            // ReSharper disable once ExpressionIsAlwaysNull
            Assert.False(originalIntron.Equals(nullObject));
        }
    }
}