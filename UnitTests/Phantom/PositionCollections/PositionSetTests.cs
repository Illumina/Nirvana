using System.Collections.Generic;
using Moq;
using Phantom.PositionCollections;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using Xunit;

namespace UnitTests.Phantom.PositionCollections
{
    public sealed class PositionSetTests
    {
        [Fact]
        public void GetPhaseSetTagIndexes_NoPS_Return_NegativeOne()
        {
            var position = new Mock<ISimplePosition>();
            var vcfFields = new string[VcfCommon.MinNumColumnsSampleGenotypes];
            vcfFields[VcfCommon.FormatIndex] = "GT";
            position.SetupGet(x => x.VcfFields).Returns(vcfFields);
            var functionBlockRanges = new List<int> { 102 };
            var positionSet = new PositionSet(new List<ISimplePosition> { position.Object }, functionBlockRanges);
            Assert.Equal(new[] { new []{-1} }, positionSet.GetSampleTagIndexes(new[] { "PS" }));
        }

        [Fact]
        public void GetPhaseSetTagIndexes_Return_Correct_PSIndex()
        {
            var position1 = new Mock<ISimplePosition>();
            var vcfFields1 = new string[VcfCommon.MinNumColumnsSampleGenotypes];
            vcfFields1[VcfCommon.FormatIndex] = "GT:PS";
            position1.SetupGet(x => x.VcfFields).Returns(vcfFields1);
            var position2 = new Mock<ISimplePosition>();
            var vcfFields2 = new string[VcfCommon.MinNumColumnsSampleGenotypes];
            vcfFields2[VcfCommon.FormatIndex] = "GT:AA:PS";
            position2.SetupGet(x => x.VcfFields).Returns(vcfFields2);
            var position3 = new Mock<ISimplePosition>();
            var vcfFields3 = new string[VcfCommon.MinNumColumnsSampleGenotypes];
            vcfFields3[VcfCommon.FormatIndex] = "GT:AA:BB:PS";
            position3.SetupGet(x => x.VcfFields).Returns(vcfFields3);
            var positionSet = new PositionSet(new List<ISimplePosition> { position1.Object, position2.Object, position3.Object }, new List<int>());
            Assert.Equal(new[] {new[] { 1, 2, 3 }}, positionSet.GetSampleTagIndexes(new[] { "PS" }));
        }

        [Fact]
        public void ExtractSamplePhaseSet_NegativePSIndex_ReturnDot()
        {
            Assert.Equal(".", PositionSet.ExtractSampleValue(-1, new[] { "0|1", "." }));
        }

        [Fact]
        public void ExtractSamplePhaseSet_PSIsDot_ReturnDot()
        {
            Assert.Equal(".", PositionSet.ExtractSampleValue(1, new[] { "0|1", "." }));
        }

        [Fact]
        public void ExtractSamplePhaseSet_PSIsNum_ReturnTheNum()
        {
            Assert.Equal("123", PositionSet.ExtractSampleValue(1, new[] { "0|1", "123" }));
        }
    }
}