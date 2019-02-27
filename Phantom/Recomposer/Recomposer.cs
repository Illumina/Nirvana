using System.Collections.Generic;
using Phantom.CodonInformation;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;

namespace Phantom.Recomposer
{
    public sealed class Recomposer : IRecomposer
    {
        private readonly PositionProcessor _positionProcessor;

        private Recomposer(PositionProcessor positionProcessor) => _positionProcessor = positionProcessor;

        public static IRecomposer Create(ISequenceProvider sequenceProvider, ITranscriptAnnotationProvider taProvider)
        {
            var transcriptIntervalArrays = taProvider.TranscriptIntervalArrays;
            var (geneIntervalForest, _)  = GeneInfoGenerator.GetGeneIntervalAndTranscripts(transcriptIntervalArrays);
            var codonInfoProvider        = new CodonInfoProvider(transcriptIntervalArrays);
            var variantGenerator         = new VariantGenerator(sequenceProvider);
            var positionBuffer           = new PositionBuffer(codonInfoProvider, geneIntervalForest);
            return new Recomposer(new PositionProcessor(positionBuffer, variantGenerator));
        }

        public IEnumerable<ISimplePosition> ProcessSimplePosition(ISimplePosition simplePosition) =>
            simplePosition == null
                ? _positionProcessor.ProcessBufferedPositions()
                : _positionProcessor.Process(simplePosition);
    }
}