using System.Collections.Generic;
using System.Linq;
using Intervals;
using Phantom.CodonInformation;
using Xunit;

namespace UnitTests.Phantom.CodonInformation
{
    public sealed class CodonInfoProviderTests
    {
        [Fact]
        public void GetCodonRange_AsExpected()
        {
            const int position = 73115941;
            var codingBlock = new CodingBlock(73115838, 73116000, 1);

            int range = CodonInfoProvider.GetCodonRange(position, codingBlock);

            Assert.Equal(73115941, range);
        }

        [Fact]
        public void GetTranscriptToCodingBlocks_AsExpected()
        {
            var intervalArray1 = new IInterval[]
            {
                new Interval(1, 10),
                new Interval(21, 30),
                new Interval(41, 50),
                new Interval(65, 70)
            };

            var intervalArray2 = new IInterval[]
            {
                new Interval(1, 10),
                new Interval(41, 50),
                new Interval(61, 70)
            };

            var intervalArray3 = new IInterval[]
            {
                new Interval(1, 10),
                new Interval(21, 26),
                new Interval(41, 50),
                new Interval(61, 70)
            };

            var phasedIntervalArrays = new List<PhasedIntervalArray>
            {
                new PhasedIntervalArray(0, intervalArray1),
                new PhasedIntervalArray(0, intervalArray2),
                new PhasedIntervalArray(0, intervalArray3)
            };

            var codingBlocklists = CodonInfoProvider.GetTranscriptToCodingBlocks(phasedIntervalArrays, false);

            var expectedBlockList1 = new List<CodingBlock>
            {
                new CodingBlock(1, 10, 0),
                new CodingBlock(21, 26, 1),
                new CodingBlock(27, 30, 1),
                new CodingBlock(41, 50, 2),
                new CodingBlock(65, 70, 0)
            };

            var expectedBlockList2 = new List<CodingBlock>
            {
                new CodingBlock(1, 10, 0),
                new CodingBlock(41, 50, 1),
                new CodingBlock(61, 64, 2),
                new CodingBlock(65, 70, 0)
            };

            var expectedBlockList3 = new List<CodingBlock>
            {
                new CodingBlock(1, 10, 0),
                new CodingBlock(21, 26, 1),
                new CodingBlock(41, 50, 1),
                new CodingBlock(61, 64, 2),
                new CodingBlock(65, 70, 0)
            };

            var expectedBlockLists = new[] { expectedBlockList1, expectedBlockList2, expectedBlockList3 };

            for (var i = 0; i < 3; i++)
            {
                Assert.True(expectedBlockLists[i].SequenceEqual(codingBlocklists[i]));
            }
        }

        [Fact]
        public void GetTranscriptToCodingBlocks_SingleBaseBlock()
        {
            var intervalArray = new IInterval[]
            {
                new Interval(1, 10),
                new Interval(21, 21),
                new Interval(41, 50),
                new Interval(65, 70)
            };

            var phasedIntervalArrays = new List<PhasedIntervalArray> { new PhasedIntervalArray(0, intervalArray) };

            var codingBlocklists = CodonInfoProvider.GetTranscriptToCodingBlocks(phasedIntervalArrays, false);

            var expectedBlockList = new List<CodingBlock>
            {
                new CodingBlock(1, 10, 0),
                new CodingBlock(21, 21, 1),
                new CodingBlock(41, 50, 2),
                new CodingBlock(65, 70, 0)
            };

            Assert.True(expectedBlockList.SequenceEqual(codingBlocklists[0]));
        }

        [Fact]
        public void GetTranscriptToCodingBlocks_AlternativeTranslationStartInCodingExon()
        {

            var intervalArray1 = new IInterval[]
            {
                new Interval(1, 50),
                new Interval(61, 70)
            };

            var intervalArray2 = new IInterval[]
            {
                new Interval(40, 50),
                new Interval(61, 70)
            };

            var phasedIntervalArrays = new List<PhasedIntervalArray>
            {
                new PhasedIntervalArray(0, intervalArray1),
                new PhasedIntervalArray(0, intervalArray2)
            };

            var codingBlocklists = CodonInfoProvider.GetTranscriptToCodingBlocks(phasedIntervalArrays, false);

            var expectedBlockList1 = new List<CodingBlock>
            {
                new CodingBlock(1, 39, 0),
                new CodingBlock(40, 50, 0),
                new CodingBlock(61, 70, 2)
            };

            var expectedBlockList2 = new List<CodingBlock>
            {
                new CodingBlock(40, 50, 0),
                new CodingBlock(61, 70, 2)
            };

            var expectedBlockLists = new[] { expectedBlockList1, expectedBlockList2};

            for (var i = 0; i < 2; i++)
            {
                Assert.True(expectedBlockLists[i].SequenceEqual(codingBlocklists[i]));
            }
        }

        [Fact]
        public void CreateCodinInfoProvider_AsExpected()
        {
        }
    }
}