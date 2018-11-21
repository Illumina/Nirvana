using System.Collections.Generic;
using IO;
using Phantom.CodonInformation;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO.Caches;
using ReadWriteUtilities = Phantom.Utilities.ReadWriteUtilities;

namespace Phantom.Recomposer
{
    public sealed class Recomposer : IRecomposer
    {
        private readonly PositionProcessor _positionProcessor;

        private Recomposer(PositionProcessor positionProcessor) => _positionProcessor = positionProcessor;

        public static IRecomposer Create(ISequenceProvider sequenceProvider,
            string inputCachePrefix)
        {
            var transcriptIntervalArrays = ReadWriteUtilities.ReadCache(PersistentStreamUtils.GetReadStream(CacheConstants.TranscriptPath(inputCachePrefix)), sequenceProvider.RefIndexToChromosome);
            var (geneIntervalForest, _)  = ReadWriteUtilities.GetIntervalAndTranscriptsForeachGene(transcriptIntervalArrays);
            var codonInfoProvider        = CodonInfoProvider.CreateCodonInfoProvider(transcriptIntervalArrays);
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