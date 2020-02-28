using Cloud.Messages.Gene;
using ErrorHandling.Exceptions;
using Xunit;

namespace UnitTests.GeneAnnotationLambda
{
    public sealed class GeneConfigTests
    {
        [Fact]
        public void Validate_NoId_ThrowException()
        {
            var input = new GeneConfig {geneSymbols = new[] {"TP53"}};
            Assert.Throws<UserErrorException>(() => input.Validate());
        }

        [Fact]
        public void Validate_NoGeneSymbols_ThrowException()
        {
            var input = new GeneConfig { id = "test" };
            Assert.Throws<UserErrorException>(() => input.Validate());
        }

        [Fact]
        public void Validate_EmptyGeneSymbols_ThrowException()
        {
            var input = new GeneConfig { id = "test", geneSymbols = new string[]{}};
            Assert.Throws<UserErrorException>(() => input.Validate());
        }
    }
}