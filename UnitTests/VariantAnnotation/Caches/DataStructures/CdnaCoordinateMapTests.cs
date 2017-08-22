using System.IO;
using System.Text;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.IO;
using Xunit;

namespace UnitTests.VariantAnnotation.Caches.DataStructures
{
    public sealed class CdnaCoordinateMapTests
    {
        [Fact]
        public void CdnaCoordinateMap_EndToEnd()
        {
            int expectedStart     = 100;
            int expectedEnd       = 200;
            int expectedCdnaStart = 300;
            int expectedCdnaEnd   = 400;

            ICdnaCoordinateMap expectedCdnaCoordinateMap =
                new CdnaCoordinateMap(expectedStart, expectedEnd, expectedCdnaStart, expectedCdnaEnd);

            ICdnaCoordinateMap observedCdnaCoordinateMap;

            using (var ms = new MemoryStream())
            {
                using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
                {
                    expectedCdnaCoordinateMap.Write(writer);
                }

                ms.Position = 0;

                using (var reader = new ExtendedBinaryReader(ms))
                {
                    observedCdnaCoordinateMap = CdnaCoordinateMap.Read(reader);
                }
            }

            Assert.NotNull(observedCdnaCoordinateMap);
            Assert.Equal(expectedCdnaCoordinateMap.Start,     observedCdnaCoordinateMap.Start);
            Assert.Equal(expectedCdnaCoordinateMap.End,       observedCdnaCoordinateMap.End);
            Assert.Equal(expectedCdnaCoordinateMap.CdnaStart, observedCdnaCoordinateMap.CdnaStart);
            Assert.Equal(expectedCdnaCoordinateMap.CdnaEnd,   observedCdnaCoordinateMap.CdnaEnd);
        }

        [Fact]
        public void IsNull_True_WhenNull()
        {
            Assert.True(CdnaCoordinateMap.Null().IsNull);
        }

        [Fact]
        public void IsNull_False_WhenNotNull()
        {
            ICdnaCoordinateMap cdnaCoordinateMap = new CdnaCoordinateMap(1, 2, 3, 4);
            Assert.False(cdnaCoordinateMap.IsNull);
        }
    }
}
