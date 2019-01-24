using System.Linq;
using SAUtils;
using VariantAnnotation.SA;
using Xunit;

namespace UnitTests.SAUtils
{
    public sealed class SaJsonKeyAnnotationTests
    {
        [Fact]
        public void GetDefinedAnnotations_AsExpected()
        {
            var keyAnnotation =
                new SaJsonKeyAnnotation {Type = JsonDataType.Number, Description = "No category defined"};

            var definedAnnotations = keyAnnotation.GetDefinedAnnotations().ToArray();
            
            Assert.Equal(("type", "number"), definedAnnotations[0]);
            Assert.Equal(("description", "No category defined"), definedAnnotations[1]);
        }

        [Fact]
        public void GetDefinedAnnotations_OnlyTypeDefinedAsString_WhenEveryThingIsNull()
        {
            var keyAnnotation = new SaJsonKeyAnnotation();
            var definedAnnotations = keyAnnotation.GetDefinedAnnotations().ToArray();

            Assert.Single(definedAnnotations);
            Assert.Equal(("type", "string"), definedAnnotations[0]);
        }
    }
}