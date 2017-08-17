using System.IO;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.VariantAnnotation.SA
{
    public sealed class SaPositionTests
    {
        [Fact]
        public void ReaderAndWriterTests()
        {
            var saDataSources = new ISaDataSource[4];
            saDataSources[0] = new SaDataSource("data1", "data1", "A", false, true, "acd", new[] { "\"id\":\"123\"" });
            saDataSources[1] = new SaDataSource("data2", "data2", "T", false, true, "acd", new[] { "\"id\":\"123\"" });
            saDataSources[2] = new SaDataSource("data3", "data3", "A", false, false, "acd", new[] { "\"id\":\"123\"" });
            saDataSources[3] = new SaDataSource("data4", "data4", "T", false, false, "acd", new[] { "\"id\":\"123\"" });

            var saPos = new SaPosition(saDataSources, "A");

            var ms = new MemoryStream();
            var writer = new ExtendedBinaryWriter(ms);
            saPos.Write(writer);
            ms.Position = 0;

            var reader = new ExtendedBinaryReader(ms);
            var observedSa = SaPosition.Read(reader);

            Assert.Equal(saPos.GlobalMajorAllele,observedSa.GlobalMajorAllele);
            Assert.Equal(saPos.DataSources.Length,observedSa.DataSources.Length);

            Assert.Equal(saPos.DataSources[3].KeyName, observedSa.DataSources[3].KeyName);

            Assert.Equal(saPos.DataSources[2].JsonStrings, observedSa.DataSources[2].JsonStrings);
            ms.Dispose();
        }
    }
}