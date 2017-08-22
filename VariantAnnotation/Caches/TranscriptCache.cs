using System.Collections.Generic;
using VariantAnnotation.Algorithms;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.TranscriptAnnotation;

namespace VariantAnnotation.Caches
{
    public sealed class TranscriptCache : ITranscriptCache
    {
        private readonly IIntervalForest<ITranscript> _transcriptIntervalForest;
        private readonly IIntervalForest<IRegulatoryRegion> _regulatoryIntervalForest;

	    public string Name { get; }
	    public GenomeAssembly GenomeAssembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        public TranscriptCache(IEnumerable<IDataSourceVersion> dataSourceVersions, GenomeAssembly genomeAssembly,
            ITranscript[] transcripts, IRegulatoryRegion[] regulatoryRegions, ushort numRefSequences)
        {
	        Name = "Transcript annotation provider";
            DataSourceVersions        = dataSourceVersions;
            GenomeAssembly            = genomeAssembly;
            _transcriptIntervalForest = IntervalArrayFactory.CreateIntervalForest(transcripts, numRefSequences);
            _regulatoryIntervalForest = IntervalArrayFactory.CreateIntervalForest(regulatoryRegions, numRefSequences);
        }

        public ITranscript[] GetOverlappingFlankingTranscripts(IChromosomeInterval interval) =>
            _transcriptIntervalForest.GetAllOverlappingValues(interval.Chromosome.Index,
                interval.Start - TranscriptAnnotationFactory.FlankingLength, interval.End + TranscriptAnnotationFactory.FlankingLength);
        public ITranscript[] GetOverlappingTranscripts(IChromosome chromosome,int start,int end) =>
            _transcriptIntervalForest.GetAllOverlappingValues(chromosome.Index,
                start, end);

        public IRegulatoryRegion[] GetOverlappingRegulatoryRegions(IChromosomeInterval interval) =>
            _regulatoryIntervalForest.GetAllOverlappingValues(interval.Chromosome.Index, interval.Start, interval.End);
    }
}