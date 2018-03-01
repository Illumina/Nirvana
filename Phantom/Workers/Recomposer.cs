using System.Collections.Generic;
using Phantom.DataStructures;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Utilities;
using ReadWriteUtilities = Phantom.Utilities.ReadWriteUtilities;

namespace Phantom.Workers
{
    public sealed class Recomposer : IRecomposer
    {
        private readonly PositionProcessor _positionProcessor;
        private readonly ISequenceProvider _sequenceProvider;

        private Recomposer(PositionProcessor positionProcessor, ISequenceProvider sequenceProvider)
        {
            _positionProcessor = positionProcessor;
            _sequenceProvider = sequenceProvider;
        }

        public static IRecomposer Create(ISequenceProvider sequenceProvider,
            string inputCachePrefix)
        {
            var transcriptIntervalArrays = ReadWriteUtilities.ReadCache(FileUtilities.GetReadStream(CacheConstants.TranscriptPath(inputCachePrefix)), sequenceProvider.RefIndexToChromosome);
            var (geneIntervalForest, _) = ReadWriteUtilities.GetIntervalAndTranscriptsForeachGene(transcriptIntervalArrays);
            var codonInfoProvider = CodonInfoProvider.CreateCodonInfoProvider(transcriptIntervalArrays);
            var variantGenerator = new VariantGenerator(sequenceProvider);
            var positionBuffer = new PositionBuffer(codonInfoProvider, geneIntervalForest);
            return new Recomposer(new PositionProcessor(positionBuffer, codonInfoProvider, variantGenerator), sequenceProvider);
        }

        public IEnumerable<ISimplePosition> ProcessSimplePosition(ISimplePosition simplePosition)
        {
            return simplePosition == null ? _positionProcessor.ProcessBufferedPositions() : _positionProcessor.Process(simplePosition);
        }
    }
}