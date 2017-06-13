using System.Linq;
using Moq;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling.SA;
using VariantAnnotation.Interface;
using Xunit;

namespace UnitTests.VariantAnnotationTests.DataStructures.SupplementaryAnnotation
{
    public class SaPositionTests
    {
        [Fact]
        public void CorrectlyAddIsAlleleSpecificForMatchByPosData()
        {
            var saDataSources = new ISaDataSource[4];
            saDataSources[0] = new SaDataSource("data1", "data1", "A", false, true, "acd", new[] { "\"id\":\"123\"" });
            saDataSources[1] = new SaDataSource("data2", "data2", "T", false, true, "acd", new[] { "\"id\":\"123\"" });
            saDataSources[2] = new SaDataSource("data3", "data3", "A", false, false, "acd", new[] { "\"id\":\"123\"" });
            saDataSources[3] = new SaDataSource("data4", "data4", "T", false, false, "acd", new[] { "\"id\":\"123\"" });

            var saPos = new SaPosition(saDataSources, "A");

            var altAllele = new Mock<IAllele>();
            altAllele.Setup(x => x.SuppAltAllele).Returns("T");

            var variantFeature = new Mock<IVariantFeature>();

            var variant = new JsonVariant(altAllele.Object, variantFeature.Object);
            variant.AddSaDataSources(saPos.DataSources.ToList());

            Assert.Null(variant.SuppAnnotations[0].IsAlleleSpecific);
            Assert.True(variant.SuppAnnotations[1].IsAlleleSpecific);
            Assert.Null(variant.SuppAnnotations[2].IsAlleleSpecific);
            Assert.True(variant.SuppAnnotations[3].IsAlleleSpecific);
        }

        [Fact]
        public void NoIsAlleleSpecificForMatchByAlleleData()
        {
            var saDataSources = new ISaDataSource[2];
            saDataSources[0] = new SaDataSource("data1", "data1", "A", true, true, "acd", new[] { "\"id\":\"123\"" });
            saDataSources[1] = new SaDataSource("data2", "data2", "T", true, false, "acd", new[] { "\"id\":\"123\"" });

            var saPos = new SaPosition(saDataSources, "A");

            var altAllele = new Mock<IAllele>();
            altAllele.Setup(x => x.SuppAltAllele).Returns("T");

            var variantFeature = new Mock<IVariantFeature>();

            var variant = new JsonVariant(altAllele.Object, variantFeature.Object);

            variant.AddSaDataSources(saPos.DataSources.ToList());

            Assert.Equal(1, variant.SuppAnnotations.Count);
            Assert.Equal("data2", variant.SuppAnnotations[0].KeyName);
            Assert.Null(variant.SuppAnnotations[0].IsAlleleSpecific);
        }
    }
}