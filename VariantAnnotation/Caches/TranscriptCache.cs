using System.Collections.Generic;
using Genome;
using Intervals;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Caches
{
    public sealed class TranscriptCache : ITranscriptCache
    {
        public IIntervalForest<ITranscript> TranscriptIntervalForest { get; }
        public IIntervalForest<IRegulatoryRegion> RegulatoryIntervalForest { get; }
	    public string Name { get; }
	    public GenomeAssembly Assembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        public TranscriptCache(IEnumerable<IDataSourceVersion> dataSourceVersions, GenomeAssembly genomeAssembly,
            IntervalArray<ITranscript>[] transcriptIntervalArrays,
            IntervalArray<IRegulatoryRegion>[] regulatoryRegionIntervalArrays)
        {
            Name                     = "Transcript annotation provider";
            DataSourceVersions       = dataSourceVersions;
            Assembly                 = genomeAssembly;
            TranscriptIntervalForest = new IntervalForest<ITranscript>(transcriptIntervalArrays);
            RegulatoryIntervalForest = new IntervalForest<IRegulatoryRegion>(regulatoryRegionIntervalArrays);
        }
    }
}