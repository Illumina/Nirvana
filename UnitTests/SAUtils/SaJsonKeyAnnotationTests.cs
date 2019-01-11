using System.Linq;
using SAUtils;
using Xunit;

namespace UnitTests.SAUtils
{
    public sealed class SaJsonKeyAnnotationTests
    {
        [Fact]
        public void GetDefinedAnnotations_AsExpected()
        {
            var keyAnnotation =
                new SaJsonKeyAnnotation {Type = "number", Description = "No category defined"};

            var definedAnnotations = keyAnnotation.GetDefinedAnnotations().ToArray();
            
            Assert.Equal(("type", "number"), definedAnnotations[0]);
            Assert.Equal(("description", "No category defined"), definedAnnotations[1]);
        }

        [Fact]
        public void GetDefinedAnnotations_EmptyOutput_WhenEveryThingIsNull()
        {
            var keyAnnotation = new SaJsonKeyAnnotation();
            var definedAnnotations = keyAnnotation.GetDefinedAnnotations();

            Assert.Empty(definedAnnotations);
        }

    }
}