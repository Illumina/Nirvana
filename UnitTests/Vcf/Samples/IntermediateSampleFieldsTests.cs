using VariantAnnotation.Interface.IO;
using Vcf.Sample;
using Xunit;

namespace UnitTests.Vcf.Samples
{
    public sealed class IntermediateSampleFieldsTests
    {
        [Fact]
        public void GetBool_ReturnTrue()
        {
            var observedResult = IntermediateSampleFields.GetBool("test", "test");
            Assert.True(observedResult);
        }

        [Fact]
        public void GetBool_ReturnFalse()
        {
            var observedResult = IntermediateSampleFields.GetBool("bob", "test");
            Assert.False(observedResult);

            observedResult = IntermediateSampleFields.GetBool(null, "test");
            Assert.False(observedResult);
        }

        [Fact]
        public void GetFloat_ReturnNull()
        {
            var observedResult = IntermediateSampleFields.GetFloat("test");
            Assert.Null(observedResult);

            observedResult = IntermediateSampleFields.GetFloat(null);
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetFloat_ReturnFloat()
        {
            var observedResult = IntermediateSampleFields.GetFloat("1.23");
            Assert.Equal(1.23f, observedResult);
        }

        [Fact]
        public void GetInteger_ReturnNull()
        {
            var observedResult = IntermediateSampleFields.GetInteger("test");
            Assert.Null(observedResult);

            observedResult = IntermediateSampleFields.GetInteger(null);
            Assert.Null(observedResult);
        }

        [Fact]
        public void GetInteger_ReturnInteger()
        {
            var observedResult = IntermediateSampleFields.GetInteger("1234");
            Assert.Equal(1234, observedResult);
        }

        [Fact]
        public void IntermediateSampleFields_AlleleCounts()
        {
            const string vcfLine = "chr1\t5592503\t.\tC\tT\t900.00\tPASS\t.\tGT:AU:CU:GU:TU:DP:DPF:AD\t1/1:10,11:20,21:30,31:40,41:70:3:0,70";
            var cols = vcfLine.Split('\t');

            var formatIndices = FormatIndices.Extract(cols[VcfCommon.FormatIndex]);
            var sampleCols    = cols[VcfCommon.GenotypeIndex].Split(':');
            var sampleFields  = new IntermediateSampleFields(cols, formatIndices, sampleCols);

            Assert.Equal(10, sampleFields.ACount);
            Assert.Equal(20, sampleFields.CCount);
            Assert.Equal(30, sampleFields.GCount);
            Assert.Equal(40, sampleFields.TCount);
        }
    }
}
