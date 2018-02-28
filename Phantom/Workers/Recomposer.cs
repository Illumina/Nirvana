using System.Collections.Generic;
using System.IO;
using Phantom.DataStructures;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Utilities;
using Vcf;
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

        /*
        public IEnumerable<IPosition> GetPositionEnumerator(StreamReader reader)
        {
            string vcfLine;
            while ((vcfLine = reader.ReadLine()) != null)
            {
                var simplePosition = SimplePosition.GetSimplePosition(vcfLine, _sequenceProvider.RefNameToChromosome);
                foreach (var vcfRecord in _positionProcessor.ProcessSimplePosition(simplePosition))
                    yield return string.Join("\t", vcfRecord);
            }
            foreach (var vcfRecord in _positionProcessor.ProcessBufferedPositions())
                yield return string.Join("\t", vcfRecord);
        }*/

        public IEnumerable<ISimplePosition> ProcessSimplePosition(ISimplePosition simplePosition)
        {
            return simplePosition == null ? _positionProcessor.ProcessBufferedPositions() : _positionProcessor.Process(simplePosition);
        }
    }
}