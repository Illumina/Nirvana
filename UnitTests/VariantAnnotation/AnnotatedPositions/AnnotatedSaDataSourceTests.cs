using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.VariantAnnotation.AnnotatedPositions
{
    public sealed class AnnotatedSaDataSourceTests
    {
        [Fact]
        public void GetJsonStrings_Positional_AlleleSpecific()
        {
            string[] expectedJsonStrings = {"test1", "test2"};
            var dataSource               = new SaDataSource("bob", "bobVcf", "A", false, false, null, expectedJsonStrings);
            var annotatedSaDataSource    = new AnnotatedSaDataSource(dataSource, "A");

            var jsonStrings = annotatedSaDataSource.GetJsonStrings();

            Assert.NotNull(jsonStrings);
            Assert.Equal(2, jsonStrings.Count);
            Assert.Contains(expectedJsonStrings[0], jsonStrings[0]);
            Assert.Contains(expectedJsonStrings[1], jsonStrings[1]);
        }

        [Fact]
        public void GetJsonStrings_NullJsonStrings()
        {
            var dataSource = new SaDataSource("bob", "bobVcf", "A", false, false, null, null);
            var annotatedSaDataSource = new AnnotatedSaDataSource(dataSource, "A");

            var jsonStrings = annotatedSaDataSource.GetJsonStrings();

            Assert.Null(jsonStrings);
        }
    }
}
