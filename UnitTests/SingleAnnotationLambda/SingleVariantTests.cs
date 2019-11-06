using System;
using System.Linq;
using Cloud.Messages.Single;
using ErrorHandling.Exceptions;
using Xunit;

namespace UnitTests.SingleAnnotationLambda
{
    public sealed class SingleVariantTests
    {
        [Fact]
        public void GetVcfFields_AsExpected()
        {
            var variant = new SingleVariant
            {
                chromosome   = "1",
                position     = 100,
                refAllele    = "A",
                altAlleles   = new[] { "C", "AC" },
                filters      = new[] { "LowGQX", "NoPassedVariantGTs" },
                infoField    = "SNVHPOL=2;MQ=34",
                formatField  = "GT:GQ:GQX:DP:DPF:AD:ADF:ADR:SB:FT:PL:ME:DQ",
                sampleFields = new[]
                {
                    "0|0:15:15:6:4:6,0:4,0:2,0:0:PASS:0,18,170:0:.", "0|1:13:0:7:6:6,1:3,0:3,1:0:LowGQX:15,0,147:.:.",
                    "0|1:18:0:9:8:8,1:2,0:6,1:0:LowGQX:20,0,156:.:."
                },
                sampleNames  = new[] { "NA12878", "NA12891", "NA12892" }
            };

            string[] vcfFields = variant.GetVcfFields();
            Assert.Equal(12, vcfFields.Length);
            Assert.Equal("1", vcfFields[0]);
            Assert.Equal("100", vcfFields[1]);
            Assert.True(vcfFields.SequenceEqual(new[]
            {
                "1", "100", ".", "A", "C,AC", ".", "LowGQX;NoPassedVariantGTs", "SNVHPOL=2;MQ=34",
                "GT:GQ:GQX:DP:DPF:AD:ADF:ADR:SB:FT:PL:ME:DQ",
                "0|0:15:15:6:4:6,0:4,0:2,0:0:PASS:0,18,170:0:.", "0|1:13:0:7:6:6,1:3,0:3,1:0:LowGQX:15,0,147:.:.",
                "0|1:18:0:9:8:8,1:2,0:6,1:0:LowGQX:20,0,156:.:."
            }));
        }

        [Fact]
        public void Validate_Success()
        {
            SingleVariant variant = GetConfig();
            Exception ex = Record.Exception(() => { variant.Validate(); });
            Assert.Null(ex);
        }

        [Fact]
        public void Validate_NullChromosome_ThrowException()
        {
            SingleVariant variant = GetConfig();
            variant.chromosome = null;
            Assert.Throws<UserErrorException>(() => variant.Validate());
        }

        [Fact]
        public void Validate_NullPosition_ThrowException()
        {
            SingleVariant variant = GetConfig();
            variant.position = null;
            Assert.Throws<UserErrorException>(() => variant.Validate());
        }

        [Fact]
        public void Validate_NullReferenceAllele_ThrowException()
        {
            SingleVariant variant = GetConfig();
            variant.refAllele = null;
            Assert.Throws<UserErrorException>(() => variant.Validate());
        }

        [Fact]
        public void Validate_NullAlternateAlleles_ThrowException()
        {
            SingleVariant variant = GetConfig();
            variant.altAlleles = null;
            Assert.Throws<UserErrorException>(() => variant.Validate());
        }

        [Fact]
        public void Validate_ZeroAlternateAlleles_ThrowException()
        {
            SingleVariant variant = GetConfig();
            variant.altAlleles = new string[0];
            Assert.Throws<UserErrorException>(() => variant.Validate());
        }

        [Fact]
        public void Validate_SampleNamesAndSampleFields_NoFormatField_ThrowException()
        {
            SingleVariant variant = GetConfig();
            variant.sampleNames   = new[] {"Bob"};
            variant.sampleFields  = new[] { "0/1" };
            Assert.Throws<UserErrorException>(() => variant.Validate());
        }

        [Fact]
        public void Validate_FormatField_NoSampleNamesAndSampleFields_ThrowException()
        {
            SingleVariant variant = GetConfig();
            variant.formatField   = "GT";
            Assert.Throws<UserErrorException>(() => variant.Validate());
        }

        private static SingleVariant GetConfig() => new SingleVariant
        {
            chromosome = "1",
            position   = 100,
            refAllele  = "A",
            altAlleles = new[] { "T", "C" }
        };
    }
}