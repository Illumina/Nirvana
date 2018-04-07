using System.Collections.Generic;
using System.Linq;
using Moq;
using Phantom.DataStructures;
using Phantom.Interfaces;
using Phantom.Workers;
using UnitTests.TestDataStructures;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Sequence;
using Vcf;
using Xunit;

namespace UnitTests.Phantom.Workers
{
    public sealed class PositionProcessorTests
    {
        private readonly Mock<IVariantGenerator> _variantGeneratorMock = new Mock<IVariantGenerator>();
        private readonly Mock<IPositionBuffer> _positionBufferMock = new Mock<IPositionBuffer>();
        private readonly Mock<ICodonInfoProvider> _codonInfoProviderMock = new Mock<ICodonInfoProvider>();


[Fact]
        public void GenerateOutput_EmptyBuffer_ReturnEmptyVcfFieldList()
        {

            var positionProcessor = new PositionProcessor(_positionBufferMock.Object, _codonInfoProviderMock.Object, _variantGeneratorMock.Object);
            Assert.Empty(positionProcessor.GenerateOutput(new BufferedPositions(new List<ISimplePosition>(), new List<bool>(), new List<int>())));
        }

        [Fact]
        public void GenerateOutput_NoRecomposable_ReturnOriginalVcfFieldList()
        {
            var positionMock1 = new Mock<IPosition>();
            var positionMock2 = new Mock<IPosition>();
            var positionMock3 = new Mock<IPosition>();
            positionMock1.SetupGet(x => x.VcfFields).Returns(new[]
                {"chr1", "2", ".", "A", ".", ".", "PASS", ".", "GT", "0/0"});
            var position1 = positionMock1.Object;
            positionMock2.SetupGet(x => x.VcfFields).Returns(new[]
                {"chr1", "4", ".", "C", ".", ".", "PASS", ".", "GT", "0/0"});
            var position2 = positionMock2.Object;
            positionMock3.SetupGet(x => x.VcfFields).Returns(new[]
                {"chr1", "6", ".", "G", ".", ".", "PASS", ".", "GT", "0/0"});
            var position3 = positionMock3.Object;

            var positions = new List<ISimplePosition> {position1, position2, position3};
            var recomposable = new List<bool> { false, false, false};
            var functionBlockRanges = new List<int>();

            var bufferedPositions = new BufferedPositions(positions, recomposable, functionBlockRanges);
            var positionProcessor = new PositionProcessor(_positionBufferMock.Object, _codonInfoProviderMock.Object, _variantGeneratorMock.Object);
            var output = positionProcessor.GenerateOutput(bufferedPositions).ToArray();
            for (int i = 0; i < output.Length; i++)
            {
                Assert.True(positions[i].VcfFields.SequenceEqual(output[i].VcfFields));
            }
        }

        [Fact]
        public void GenerateOutput_OneRecomposable_ReturnOriginalVcfFieldList()
        {
            var positionMock1 = new Mock<IPosition>();
            var positionMock2 = new Mock<IPosition>();
            var positionMock3 = new Mock<IPosition>();
            positionMock1.SetupGet(x => x.VcfFields).Returns(new[]
                {"chr1", "2", ".", "A", ".", ".", "PASS", ".", "GT", "0/0"});
            var position1 = positionMock1.Object;
            positionMock2.SetupGet(x => x.VcfFields).Returns(new[]
                {"chr1", "4", ".", "C", "T", ".", "PASS", ".", "GT", "0|0"});
            var position2 = positionMock2.Object;
            positionMock3.SetupGet(x => x.VcfFields).Returns(new[]
                {"chr1", "6", ".", "G", ".", ".", "PASS", ".", "GT", "0/0"});
            var position3 = positionMock3.Object;

            var positions = new List<ISimplePosition> { position1, position2, position3 };
            var recomposable = new List<bool> { false, true, false };
            var functionBlockRanges = new List<int> {4};

            var bufferedPositions = new BufferedPositions(positions, recomposable, functionBlockRanges);
            var positionProcessor = new PositionProcessor(_positionBufferMock.Object, _codonInfoProviderMock.Object, _variantGeneratorMock.Object);
            var output = positionProcessor.GenerateOutput(bufferedPositions).ToArray();
            for (int i = 0; i < output.Length; i++)
            {
                Assert.True(positions[i].VcfFields.SequenceEqual(output[i].VcfFields));
            }
        }

        [Fact]
        public void GenerateOutput_NothingRecomposed_ReturnOriginalVcfFieldList()
        {
            var mockSequenceProvider = new Mock<ISequenceProvider>();
            mockSequenceProvider.SetupGet(x => x.RefNameToChromosome)
                .Returns(new Dictionary<string, IChromosome> { { "chr1", new Chromosome("chr1", "1", 0) }
                });
            mockSequenceProvider.SetupGet(x => x.Sequence).Returns(new SimpleSequence("CAGCTGAA"));
            var sequenceProvider = mockSequenceProvider.Object;
            var variantGenerator = new VariantGenerator(sequenceProvider);

            var position1 = SimplePosition.GetSimplePosition("chr1	2	.	A	T	.	PASS	.	GT:PS	0|1:123", sequenceProvider.RefNameToChromosome);
            var position2 = SimplePosition.GetSimplePosition("chr1	4	.	C	G	.	PASS	.	GT	0/1", sequenceProvider.RefNameToChromosome);
            var position3 = SimplePosition.GetSimplePosition("chr1	6	.	G	C	.	PASS	.	GT	0|1", sequenceProvider.RefNameToChromosome);

            var positions = new List<ISimplePosition> { position1, position2, position3 };
            var recomposable = new List<bool> { true, true, true };
            var functionBlockRanges = new List<int> {4, 6, 8};
            var bufferedPositions = new BufferedPositions(positions, recomposable, functionBlockRanges);
            var positionProcessor = new PositionProcessor(_positionBufferMock.Object, _codonInfoProviderMock.Object, variantGenerator);
            var output = positionProcessor.GenerateOutput(bufferedPositions).ToArray();

            for (int i = 0; i < output.Length; i++)
            {
                Assert.True(positions[i].VcfFields.SequenceEqual(output[i].VcfFields));
            }
        }

        [Fact]
        public void GenerateOutput_Return_OriginalAndRecomposed_VcfFieldList()
        {
            var mockSequenceProvider = new Mock<ISequenceProvider>();
            mockSequenceProvider.SetupGet(x => x.RefNameToChromosome)
                .Returns(new Dictionary<string, IChromosome> { { "chr1", new Chromosome("chr1", "1", 0) }
                });
            mockSequenceProvider.SetupGet(x => x.Sequence).Returns(new SimpleSequence("CAGCTGAA"));
            var sequenceProvider = mockSequenceProvider.Object;
            var variantGenerator = new VariantGenerator(sequenceProvider);

            var position1 = SimplePosition.GetSimplePosition("chr1	2	.	A	T	.	PASS	.	GT:PS	0|1:.", sequenceProvider.RefNameToChromosome);
            var position2 = SimplePosition.GetSimplePosition("chr1	4	.	C	G	.	PASS	.	GT	1/1", sequenceProvider.RefNameToChromosome);
            var position3 = SimplePosition.GetSimplePosition("chr1	6	.	G	C	.	PASS	.	GT	0|1", sequenceProvider.RefNameToChromosome);

            var positions = new List<ISimplePosition> { position1, position2, position3 };
            var recomposable = new List<bool> { true, true, true };
            var functionBlockRanges = new List<int> { 4, 6, 8 };
            var bufferedPositions = new BufferedPositions(positions, recomposable, functionBlockRanges);
            var positionProcessor = new PositionProcessor(_positionBufferMock.Object, _codonInfoProviderMock.Object, variantGenerator);
            var output = positionProcessor.GenerateOutput(bufferedPositions).ToArray();

            var expectedOutput = new string[4][];
            expectedOutput[0] = position1.VcfFields;
            expectedOutput[1] = new[]
                {"chr1", "2", ".", "AGCTG", "AGGTG,TGGTC", ".", "PASS", "RECOMPOSED", "GT", "1|2"};
            expectedOutput[2] = position2.VcfFields;
            expectedOutput[3] = position3.VcfFields;

            for (int i = 0; i < output.Length; i++)
            {
                Assert.True(expectedOutput[i].SequenceEqual(output[i].VcfFields));
            }
        }
    }
}