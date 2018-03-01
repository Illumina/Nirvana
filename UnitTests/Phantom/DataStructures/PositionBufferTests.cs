using System.Collections.Generic;
using Moq;
using Phantom.DataStructures;
using Phantom.Interfaces;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Sequence;
using Xunit;

namespace UnitTests.Phantom.DataStructures
{
    public sealed class PositionBufferTests
    {
        private readonly Mock<IIntervalForest<IGene>> _geneIntervalForestMock = new Mock<IIntervalForest<IGene>>();
        private readonly Mock<ICodonInfoProvider> _codonInfoProviderMock = new Mock<ICodonInfoProvider>();

        [Fact]
        public void IsRecomposable_NoCorrectGtTag_ReturnFalse()
        {
            Assert.False(PositionBuffer.IsRecomposable(GetMockedIPositionOnChr1(40, 40, "C", "AA")));
            Assert.False(PositionBuffer.IsRecomposable(GetMockedIPositionOnChr1(40, 40, "C", "AA:BB")));
            Assert.False(PositionBuffer.IsRecomposable(GetMockedIPositionOnChr1(40, 40, "C", "")));
            Assert.False(PositionBuffer.IsRecomposable(GetMockedIPositionOnChr1(40, 40, "C", "GTO:BB")));
            Assert.False(PositionBuffer.IsRecomposable(GetMockedIPositionOnChr1(40, 40, "C", "BB:GT")));
        }

        [Fact]
        public void InGeneRegion_NonOverlap_ReturnFalse()
        {
            _geneIntervalForestMock.Setup(x => x.OverlapsAny(0, 40, 40)).Returns(false);
            var positionBuffer = new PositionBuffer(_codonInfoProviderMock.Object, _geneIntervalForestMock.Object);
            Assert.False(positionBuffer.InGeneRegion(GetMockedIPositionOnChr1(40, 40, "C")));
        }

        [Fact]
        public void InGeneRegion_overlap_ReturnTrue()
        {
            _geneIntervalForestMock.Setup(x => x.OverlapsAny(0, 40, 40)).Returns(true);
            var positionBuffer = new PositionBuffer(_codonInfoProviderMock.Object, _geneIntervalForestMock.Object);
            Assert.True(positionBuffer.InGeneRegion(GetMockedIPositionOnChr1(40, 40, "C")));
        }

        [Fact]
        public void PositionWithinRange_Position_InRange_ReturnTrue()
        {
            var position1 = GetMockedIPositionOnChr1(99, 99);
            _geneIntervalForestMock.Setup(x => x.OverlapsAny(0, 100, 100)).Returns(true);
            _codonInfoProviderMock.Setup(x => x.GetFunctionBlockRanges(position1)).Returns(101);
            var positionBuffer = new PositionBuffer(_codonInfoProviderMock.Object, _geneIntervalForestMock.Object);
            positionBuffer.AddPosition(position1);
            var position2 = GetMockedIPositionOnChr1(100, 100, "C");
            Assert.True(positionBuffer.PositionWithinRange(position2));
        }

        [Fact]
        public void PositionWithinRange_Position_OutRange_ReturnFalse()
        {
            var position1 = GetMockedIPositionOnChr1(99, 99);
            _geneIntervalForestMock.Setup(x => x.OverlapsAny(0, 100, 100)).Returns(true);
            _codonInfoProviderMock.Setup(x => x.GetFunctionBlockRanges(position1)).Returns(99);
            var positionBuffer = new PositionBuffer(_codonInfoProviderMock.Object, _geneIntervalForestMock.Object);
            positionBuffer.AddPosition(position1);
            var position2 = GetMockedIPositionOnChr1(100, 100, "C");
            Assert.False(positionBuffer.PositionWithinRange(position2));
        }

        [Fact]
        public void PositionWithinRange_Position_OutGeneRegion_ReturnFalse()
        {
            var position1 = GetMockedIPositionOnChr1(99, 99);
            _geneIntervalForestMock.Setup(x => x.OverlapsAny(0, 100, 100)).Returns(false);
            _codonInfoProviderMock.Setup(x => x.GetFunctionBlockRanges(position1)).Returns(101);
            var positionBuffer = new PositionBuffer(_codonInfoProviderMock.Object, _geneIntervalForestMock.Object);
            positionBuffer.AddPosition(position1);
            var position2 = GetMockedIPositionOnChr1(100, 100, "C");
            Assert.False(positionBuffer.PositionWithinRange(position2));
        }

        [Fact]
        public void AddPosition_ChromosomeUpdated_WhenFirstPositionAdded()
        {
            var positionBuffer = new PositionBuffer(_codonInfoProviderMock.Object, _geneIntervalForestMock.Object);
            var position = GetMockedIPositionOnChr1(99, 99);
            positionBuffer.AddPosition(position);
            Assert.Equal(new Chromosome("chr1", "1", 0), positionBuffer.CurrentChromosome);
        }

        [Fact]
        public void AddPosition_NewPositionInRange_EmptyBufferReturned()
        {
            var position1 = GetMockedIPositionOnChr1(99, 99);
            _geneIntervalForestMock.Setup(x => x.OverlapsAny(0, 100, 100)).Returns(true);
            _codonInfoProviderMock.Setup(x => x.GetFunctionBlockRanges(position1)).Returns(101);
            var positionBuffer = new PositionBuffer(_codonInfoProviderMock.Object, _geneIntervalForestMock.Object);
            positionBuffer.AddPosition(position1);
            var position2 = GetMockedIPositionOnChr1(100, 100, "C");
            var bufferedPositions = positionBuffer.AddPosition(position2);
            Assert.IsType<BufferedPositions>(bufferedPositions);
            Assert.Empty(bufferedPositions.SimplePositions);
            Assert.Empty(bufferedPositions.Recomposable);
        }

        [Fact]
        public void AddPosition_NewPositionOutRange_BufferedPositionsReturned()
        {
            var position1 = GetMockedIPositionOnChr1(99, 99);
            var position2 = GetMockedIPositionOnChr1(100, 100, ".");
            var position3 = GetMockedIPositionOnChr1(110, 110, "C");
            _geneIntervalForestMock.Setup(x => x.OverlapsAny(0, 100, 100)).Returns(true);
            _codonInfoProviderMock.Setup(x => x.GetFunctionBlockRanges(position1)).Returns(101);
            _codonInfoProviderMock.Setup(x => x.GetFunctionBlockRanges(position2)).Returns(101);
            var positionBuffer = new PositionBuffer(_codonInfoProviderMock.Object, _geneIntervalForestMock.Object);
            positionBuffer.AddPosition(position1);
            positionBuffer.AddPosition(position2);
            var bufferedPositions = positionBuffer.AddPosition(position3);
            Assert.IsType<BufferedPositions>(bufferedPositions);
            Assert.Equal(2, bufferedPositions.SimplePositions.Count);
            Assert.Equal(2, bufferedPositions.Recomposable.Count);
            Assert.True(bufferedPositions.SimplePositions[0].Equals(position1));
            Assert.True(bufferedPositions.SimplePositions[1].Equals(position2));
            Assert.True(bufferedPositions.Recomposable[0]);
            Assert.False(bufferedPositions.Recomposable[1]);
        }

        [Fact]
        public void Purge_BufferedPositionsPurged()
        {
            var position1 = GetMockedIPositionOnChr1(99, 99);
            var position2 = GetMockedIPositionOnChr1(100, 100, ".");
            _geneIntervalForestMock.Setup(x => x.OverlapsAny(0, 100, 100)).Returns(true);
            _codonInfoProviderMock.Setup(x => x.GetFunctionBlockRanges(position1)).Returns(101);
            _codonInfoProviderMock.Setup(x => x.GetFunctionBlockRanges(position2)).Returns(101);
            var positionBuffer = new PositionBuffer(_codonInfoProviderMock.Object, _geneIntervalForestMock.Object);
            positionBuffer.AddPosition(position1);
            positionBuffer.AddPosition(position2);
            var bufferedPositions = positionBuffer.Purge();
            Assert.IsType<BufferedPositions>(bufferedPositions);
            Assert.Equal(2, bufferedPositions.SimplePositions.Count);
            Assert.Equal(2, bufferedPositions.Recomposable.Count);
            Assert.True(bufferedPositions.SimplePositions[0].Equals(position1));
            Assert.True(bufferedPositions.SimplePositions[1].Equals(position2));
            Assert.True(bufferedPositions.Recomposable[0]);
            Assert.False(bufferedPositions.Recomposable[1]);
        }


        [Fact]
        public void GetRecomposablePositions_OnlyRecomposablePositions_Returned()
        {
            var positions = new List<ISimplePosition>
            {
                GetMockedIPositionOnChr1(99, 99),
                GetMockedIPositionOnChr1(101, 101, "."),
                GetMockedIPositionOnChr1(105, 105, "T"),
                GetMockedIPositionOnChr1(110, 110, ".")
            };
            var recomposable = new List<bool> {true, false, true, false};
            var functionBlockRanges = new List<int> {102, 110};
            var recomposablePositions = new BufferedPositions(positions, recomposable, functionBlockRanges).GetRecomposablePositions();
            Assert.Equal(2, recomposablePositions.Count);
            Assert.True(positions[0].Equals(recomposablePositions[0]));
            Assert.True(positions[2].Equals(recomposablePositions[1]));
        }


        internal static IPosition GetMockedIPositionOnChr1(int start, int end, string altAllele="A", string formatCol = "GT")
        {
            var positionMock = new Mock<IPosition>();
            var chromosome = new Chromosome("chr1", "1", 0);
            positionMock.SetupGet(x => x.Chromosome).Returns(chromosome);
            positionMock.SetupGet(x => x.Start).Returns(start);
            positionMock.SetupGet(x => x.End).Returns(end);
            var vcfFields = new string[VcfCommon.MinNumColumnsSampleGenotypes];
            vcfFields[VcfCommon.AltIndex] = altAllele;
            vcfFields[VcfCommon.FormatIndex] = formatCol;
            positionMock.SetupGet(x => x.VcfFields).Returns(vcfFields);
            return positionMock.Object;
        }

    }
}