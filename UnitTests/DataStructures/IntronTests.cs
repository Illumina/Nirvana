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
            var originalIntron = new Intron(100, 200);

            const int expectedHashCode = 172;
            var observedHashCode = originalIntron.GetHashCode();

            Assert.Equal(expectedHashCode, observedHashCode);
        }

        [Fact]
        public void SerializationAndEquality()
        {
            string intronPath = Path.GetTempPath() + Guid.NewGuid() + ".dat";
            var originalIntron = new Intron(100, 200);

            // serialize the intron
            using (var writer = new BinaryWriter(new FileStream(intronPath, FileMode.Create)))
            {
                var extWriter = new ExtendedBinaryWriter(writer);
                originalIntron.Write(extWriter);
            }

            // deserialize the intron
            Intron newIntron;
            using (
                var reader = new BinaryReader(FileUtilities.GetFileStream(intronPath))
                )
            {
                var extReader = new ExtendedBinaryReader(reader);
                newIntron = Intron.Read(extReader);
            }

            Assert.Equal(originalIntron, newIntron);

            // test the equality operations
            Assert.True(originalIntron.Equals(newIntron));
            Assert.True(originalIntron == newIntron);
            Assert.False(originalIntron != newIntron);

            Intron nullIntron = null;
            Intron nullIntron2 = null;

            Assert.True(nullIntron == nullIntron2);
            Assert.False(originalIntron == nullIntron);

            object nullObject = null;
            Assert.False(originalIntron.Equals(nullObject));
        }
    }
}