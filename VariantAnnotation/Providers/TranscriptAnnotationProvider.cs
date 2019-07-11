using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using Genome;
using Intervals;
using IO;
using OptimizedCore;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Caches;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.TranscriptAnnotation;
using Variants;

namespace VariantAnnotation.Providers
{
    public sealed class TranscriptAnnotationProvider : ITranscriptAnnotationProvider
    {
        private readonly ITranscriptCache _transcriptCache;
        private readonly ISequence _sequence;

        public string Name { get; }
        public GenomeAssembly Assembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }
        public IntervalArray<ITranscript>[] TranscriptIntervalArrays { get; }
        public ushort VepVersion { get; }

        private readonly PredictionCacheReader _siftReader;
        private readonly PredictionCacheReader _polyphenReader;
        private IPredictionCache _siftCache;
        private IPredictionCache _polyphenCache;
        private ushort _currentRefIndex = ushort.MaxValue;

        public TranscriptAnnotationProvider(string pathPrefix, ISequenceProvider sequenceProvider)
        {
            Name = "Transcript annotation provider";
            _sequence = sequenceProvider.Sequence;

            var transcriptStream = PersistentStreamUtils.GetReadStream(CacheConstants.TranscriptPath(pathPrefix));
            (_transcriptCache, TranscriptIntervalArrays, VepVersion) = InitiateCache(transcriptStream, sequenceProvider.RefIndexToChromosome, sequenceProvider.Assembly);

            Assembly = _transcriptCache.Assembly;
            DataSourceVersions = _transcriptCache.DataSourceVersions;


            var siftStream = PersistentStreamUtils.GetReadStream(CacheConstants.SiftPath(pathPrefix));
            _siftReader = new PredictionCacheReader(siftStream, PredictionCacheReader.SiftDescriptions);

            var polyphenStream = PersistentStreamUtils.GetReadStream(CacheConstants.PolyPhenPath(pathPrefix));
            _polyphenReader = new PredictionCacheReader(polyphenStream, PredictionCacheReader.PolyphenDescriptions);
        }

        private static (TranscriptCache Cache, IntervalArray<ITranscript>[] TranscriptIntervalArrays, ushort VepVersion) InitiateCache(Stream stream,
            IDictionary<ushort, IChromosome> refIndexToChromosome, GenomeAssembly refAssembly)
        {
            TranscriptCache cache;
            ushort vepVersion;
            TranscriptCacheData cacheData;

            using (var reader = new TranscriptCacheReader(stream))
            {
                vepVersion = reader.Header.Custom.VepVersion;
                CheckHeaderVersion(reader.Header, refAssembly);
                cacheData = reader.Read(refIndexToChromosome);
                cache = cacheData.GetCache();
            }

            return (cache, cacheData.TranscriptIntervalArrays, vepVersion);
        }

        private static void CheckHeaderVersion(Header header, GenomeAssembly refAssembly)
        {
            if (header.Assembly != refAssembly)
                throw new UserErrorException(GetAssemblyErrorMessage(header.Assembly, refAssembly));

            if (header.SchemaVersion != CacheConstants.SchemaVersion)
                throw new UserErrorException(
                    $"Expected the cache schema version ({CacheConstants.SchemaVersion}) to be identical to the schema version in the cache header ({header.SchemaVersion})");
        }

        private static string GetAssemblyErrorMessage(GenomeAssembly cacheAssembly, GenomeAssembly refAssembly)
        {
            var sb = StringBuilderCache.Acquire();
            sb.AppendLine("Not all of the data sources have the same genome assembly:");
            sb.AppendLine($"- Using {refAssembly}: Reference sequence provider");
            sb.AppendLine($"- Using {cacheAssembly}: Transcript annotation provider");
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            if (annotatedPosition.AnnotatedVariants == null || annotatedPosition.AnnotatedVariants.Length == 0) return;

            ushort refIndex = annotatedPosition.Position.Chromosome.Index;
            LoadPredictionCaches(refIndex);

            AddRegulatoryRegions(annotatedPosition.AnnotatedVariants, _transcriptCache.RegulatoryIntervalForest);
            AddTranscripts(annotatedPosition.AnnotatedVariants, _transcriptCache.TranscriptIntervalForest);
        }

        private void AddTranscripts(IAnnotatedVariant[] annotatedVariants, IIntervalForest<ITranscript> transcriptIntervalForest)
        {
            foreach (var annotatedVariant in annotatedVariants)
            {
                var variant = annotatedVariant.Variant;
                if (variant.Behavior.Equals(AnnotationBehavior.MinimalAnnotationBehavior)) continue;

                ITranscript[] geneFusionCandidates = GetGeneFusionCandidates(variant.BreakEnds, transcriptIntervalForest);
                ITranscript[] transcripts          = transcriptIntervalForest.GetAllFlankingValues(variant);
                if (transcripts == null) continue;

                IList<IAnnotatedTranscript> annotatedTranscripts =
                    TranscriptAnnotationFactory.GetAnnotatedTranscripts(variant, transcripts, _sequence, _siftCache,
                        _polyphenCache, geneFusionCandidates);

                if (annotatedTranscripts.Count == 0) continue;

                foreach (var annotatedTranscript in annotatedTranscripts)
                    annotatedVariant.Transcripts.Add(annotatedTranscript);
            }
        }

        public void PreLoad(IChromosome chromosome, List<int> positions) => throw new System.NotImplementedException();

        private void LoadPredictionCaches(ushort refIndex)
        {
            if (refIndex == _currentRefIndex) return;

            if (refIndex == ushort.MaxValue)
            {
                ClearCache();
                return;
            }

            _siftCache       = _siftReader.Read(refIndex);
            _polyphenCache   = _polyphenReader.Read(refIndex);
            _currentRefIndex = refIndex;
        }

        private void ClearCache()
        {
            _siftCache       = null;
            _polyphenCache   = null;
            _currentRefIndex = ushort.MaxValue;
        }

        private static ITranscript[] GetGeneFusionCandidates(IBreakEnd[] breakEnds, IIntervalForest<ITranscript> transcriptIntervalForest)
        {
            if (breakEnds == null || breakEnds.Length == 0) return null;

            var geneFusionCandidates = new HashSet<ITranscript>();

            foreach (var breakEnd in breakEnds)
            {
                ITranscript[] transcripts = transcriptIntervalForest.GetAllOverlappingValues(
                    breakEnd.Piece2.Chromosome.Index, breakEnd.Piece2.Position, breakEnd.Piece2.Position);
                if (transcripts == null) continue;

                foreach (var transcript in transcripts)
                {
                    if (transcript.Id.IsPredictedTranscript()) continue;
                    geneFusionCandidates.Add(transcript);
                }
            }

            return geneFusionCandidates.ToArray();
        }

        private static void AddRegulatoryRegions(IAnnotatedVariant[] annotatedVariants, IIntervalForest<IRegulatoryRegion> regulatoryIntervalForest)
        {
            if (annotatedVariants[0].Variant.Behavior.Equals(AnnotationBehavior.RohBehavior)) return;
            foreach (var annotatedVariant in annotatedVariants)
            {
                // In case of insertions, the base(s) are assumed to be inserted at the end position
                // if this is an insertion just before the beginning of the regulatory element, this takes care of it
                var variant      = annotatedVariant.Variant;
                int variantBegin = variant.Type == VariantType.insertion ? variant.End : variant.Start;

                if (SkipLargeVariants(variantBegin, variant.End)) continue;

                IRegulatoryRegion[] regulatoryRegions =
                    regulatoryIntervalForest.GetAllOverlappingValues(variant.Chromosome.Index, variantBegin,
                        variant.End);
                if (regulatoryRegions == null) continue;

                foreach (var regulatoryRegion in regulatoryRegions)
                {
                    // if the insertion is at the end, its past the feature and therefore not overlapping
                    if (variant.Type == VariantType.insertion && variant.End == regulatoryRegion.End) continue;

                    annotatedVariant.RegulatoryRegions.Add(RegulatoryRegionAnnotator.Annotate(variant, regulatoryRegion));
                }
            }
        }

        private const int MaxSvLengthForRegulatoryRegionAnnotation = 50000;

        private static bool SkipLargeVariants(int begin, int end) => end - begin + 1 > MaxSvLengthForRegulatoryRegionAnnotation;

        public void Dispose()
        {
            _siftReader?.Dispose();
            _polyphenReader?.Dispose();
        }
    }
}