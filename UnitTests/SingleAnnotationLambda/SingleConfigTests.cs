using System;
using Cloud.Messages.Single;
using ErrorHandling.Exceptions;
using SingleAnnotationLambda;
using Xunit;

namespace UnitTests.SingleAnnotationLambda
{
    public sealed class SingleConfigTests
    {
        [Fact]
        public void Validate_Success()
        {
            SingleConfig config = GetConfig();
            Exception ex = Record.Exception(() => { config.Validate(); });
            Assert.Null(ex);
        }

        [Fact]
        public void Validate_NullId_ThrowException()
        {
            SingleConfig config = GetConfig();
            config.id = null;
            Assert.Throws<UserErrorException>(() => config.Validate());
        }

        [Fact]
        public void Validate_NullGenomeAssembly_ThrowException()
        {
            SingleConfig config = GetConfig();
            config.genomeAssembly = null;
            Assert.Throws<UserErrorException>(() => config.Validate());
        }

        [Fact]
        public void Validate_NullVariant_ThrowException()
        {
            SingleConfig config = GetConfig();
            config.variant = null;
            Assert.Throws<UserErrorException>(() => config.Validate());
        }

        private static SingleConfig GetConfig() => new SingleConfig
        {
            id             = "Test",
            genomeAssembly = "Assembly",
            variant        = new SingleVariant()
            {
                chromosome = "1",
                position   = 100,
                refAllele  = "A",
                altAlleles = new[] { "T", "C"}
            }
        };
    }
}