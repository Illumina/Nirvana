using System.IO;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.DataStructures
{
	public class ExonTests
	{
		[Fact]
		public void ExonReadWriteTests()
		{
			var randomPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            var exon1 = new CdnaCoordinateMap(100, 200, 10, 20);
            var exon2 = new CdnaCoordinateMap(100, 200, 11, 21);
            var exon3 = new CdnaCoordinateMap(105, 201, 12, 15);

            using (var writer = new ExtendedBinaryWriter(FileUtilities.GetCreateStream(randomPath)))
			{
				exon1.Write(writer);
				exon2.Write(writer);
				exon3.Write(writer);
			}

			using (var reader = new ExtendedBinaryReader(FileUtilities.GetReadStream(randomPath)))
			{
				Assert.Equal(exon1, CdnaCoordinateMap.Read(reader));
				Assert.Equal(exon2, CdnaCoordinateMap.Read(reader));
				Assert.Equal(exon3, CdnaCoordinateMap.Read(reader));
			}

			File.Delete(randomPath);
		}

		[Fact]
		public void ExonEqualityTests()
		{
            var exon1 = new CdnaCoordinateMap(100, 200, 1, 2);
            var exon2 = new CdnaCoordinateMap(100, 200, 1, 2);
            Assert.Equal(exon1,exon2);
		}

		[Fact]
		public void ExonToStringTests()
		{
            var exon1 = new CdnaCoordinateMap(100, 200, 1, 2);
            Assert.NotNull(exon1.ToString());
        }
    }
}
