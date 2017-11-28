using System.Collections.Generic;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;

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
            IntervalArray<ITranscript>[] transcriptIntervalArrays,
            IntervalArray<IRegulatoryRegion>[] regulatoryRegionIntervalArrays)
        {
	        Name                      = "Transcript annotation provider";
            DataSourceVersions        = dataSourceVersions;
            GenomeAssembly            = genomeAssembly;
            _transcriptIntervalForest = new IntervalForest<ITranscript>(transcriptIntervalArrays);
            _regulatoryIntervalForest = new IntervalForest<IRegulatoryRegion>(regulatoryRegionIntervalArrays);
        }

        public ITranscript[] GetOverlappingTranscripts(IChromosomeInterval interval) =>
            GetOverlappingTranscripts(interval.Chromosome, interval.Start, interval.End);

        public ITranscript[] GetOverlappingTranscripts(IChromosome chromosome, int start, int end,
            int flankingLength = OverlapBehavior.FlankingLength) =>
            _transcriptIntervalForest.GetAllOverlappingValues(chromosome.Index, start - flankingLength,
                end + flankingLength);

        public IRegulatoryRegion[] GetOverlappingRegulatoryRegions(IChromosomeInterval interval) =>
            _regulatoryIntervalForest.GetAllOverlappingValues(interval.Chromosome.Index, interval.Start, interval.End);
    }
}