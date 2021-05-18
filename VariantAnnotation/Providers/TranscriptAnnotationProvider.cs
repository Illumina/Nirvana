using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ErrorHandling.Exceptions;
using Genome;
using Intervals;
using IO;
using OptimizedCore;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Caches;
using VariantAnnotation.GeneFusions.Calling;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.TranscriptAnnotation;
using Variants;

namespace VariantAnnotation.Providers
{
    public sealed class TranscriptAnnotationProvider : ITranscriptAnnotationProvider
    {
        private readonly ITranscriptCache _transcriptCache;
        private readonly ISequence        _sequence;

        public string                          Name                     { get; }
        public GenomeAssembly                  Assembly                 { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions       { get; }
        public IntervalArray<ITranscript>[]    TranscriptIntervalArrays { get; }
        public ushort                          VepVersion               { get; }

        private readonly Stream                _siftStream;
        private readonly Stream                _polyphenStream;
        private readonly PredictionCacheReader _siftReader;
        private readonly PredictionCacheReader _polyphenReader;
        private          IPredictionCache      _siftCache;
        private          IPredictionCache      _polyphenCache;
        private          ushort                _currentRefIndex = ushort.MaxValue;

        private readonly ProteinConservationProvider _conservationProvider;
        private readonly GeneFusionCaller            _fusionCaller;

        public TranscriptAnnotationProvider(string pathPrefix, ISequenceProvider sequenceProvider, ProteinConservationProvider conservationProvider)
        {
            Name                  = "Transcript annotation provider";
            _sequence             = sequenceProvider.Sequence;
            _conservationProvider = conservationProvider;

            using (var stream = PersistentStreamUtils.GetReadStream(CacheConstants.TranscriptPath(pathPrefix)))
            {
                (_transcriptCache, TranscriptIntervalArrays, VepVersion) =
                    InitiateCache(stream, sequenceProvider.RefIndexToChromosome, sequenceProvider.Assembly);
            }

            _fusionCaller      = new GeneFusionCaller(sequenceProvider.RefNameToChromosome, _transcriptCache.TranscriptIntervalForest);
            Assembly           = _transcriptCache.Assembly;
            DataSourceVersions = _transcriptCache.DataSourceVersions;

            // TODO: this is not great. We should not be using IEnumerables if we have to resort to strange stuff like this
            if (conservationProvider != null)
                DataSourceVersions = DataSourceVersions.Concat(new[] {conservationProvider.Version});

            _siftStream = PersistentStreamUtils.GetReadStream(CacheConstants.SiftPath(pathPrefix));
            _siftReader = new PredictionCacheReader(_siftStream, PredictionCacheReader.SiftDescriptions);

            _polyphenStream = PersistentStreamUtils.GetReadStream(CacheConstants.PolyPhenPath(pathPrefix));
            _polyphenReader = new PredictionCacheReader(_polyphenStream, PredictionCacheReader.PolyphenDescriptions);
        }

        private static (TranscriptCache Cache, IntervalArray<ITranscript>[] TranscriptIntervalArrays, ushort VepVersion) InitiateCache(Stream stream,
            IDictionary<ushort, IChromosome> refIndexToChromosome, GenomeAssembly refAssembly)
        {
            using var reader     = new TranscriptCacheReader(stream);
            ushort    vepVersion = reader.Header.Custom.VepVersion;
            CheckHeaderVersion(reader.Header, refAssembly);
            TranscriptCacheData cacheData = reader.Read(refIndexToChromosome);
            TranscriptCache     cache     = cacheData.GetCache();

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
            StringBuilder sb = StringBuilderCache.Acquire();
            sb.AppendLine("Not all of the data sources have the same genome assembly:");
            sb.AppendLine($"- Using {refAssembly}: Reference sequence provider");
            sb.AppendLine($"- Using {cacheAssembly}: Transcript annotation provider");
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            if (annotatedPosition.AnnotatedVariants == null || annotatedPosition.AnnotatedVariants.Length == 0) return;

            IPosition position = annotatedPosition.Position;
            ushort    refIndex = position.Chromosome.Index;
            LoadPredictionCaches(refIndex);

            AddRegulatoryRegions(annotatedPosition.AnnotatedVariants, _transcriptCache.RegulatoryIntervalForest);
            AddTranscripts(annotatedPosition.AnnotatedVariants);

            if (position.HasStructuralVariant)
                _fusionCaller.AddGeneFusions(annotatedPosition.AnnotatedVariants, annotatedPosition.Position.InfoData.IsImprecise,
                    position.InfoData.IsInv3, position.InfoData.IsInv5);
        }

        private void AddTranscripts(IAnnotatedVariant[] annotatedVariants)
        {
            foreach (var annotatedVariant in annotatedVariants)
            {
                var variant = annotatedVariant.Variant;
                if (variant.Behavior.MinimalTranscriptAnnotation) continue;

                ITranscript[] transcripts = _transcriptCache.TranscriptIntervalForest.GetAllFlankingValues(variant);
                if (transcripts == null) continue;

                IList<IAnnotatedTranscript> annotatedTranscripts =
                    TranscriptAnnotationFactory.GetAnnotatedTranscripts(variant, transcripts, _sequence, _siftCache,
                        _polyphenCache);

                if (annotatedTranscripts.Count == 0) continue;

                foreach (IAnnotatedTranscript annotatedTranscript in annotatedTranscripts)
                {
                    AddConservationScore(annotatedTranscript);
                }

                foreach (IAnnotatedTranscript annotatedTranscript in annotatedTranscripts)
                    annotatedVariant.Transcripts.Add(annotatedTranscript);
            }
        }

        private void AddConservationScore(IAnnotatedTranscript annotatedTranscript)
        {
            if (_conservationProvider              == null) return;
            if (annotatedTranscript.MappedPosition == null) return;

            var scores = new List<double>();
            int start  = annotatedTranscript.MappedPosition.ProteinStart;
            int end    = annotatedTranscript.MappedPosition.ProteinEnd;

            if (start == -1 || end == -1) return;
            for (int aaPos = start; aaPos <= end; aaPos++)
            {
                string transcriptId = annotatedTranscript.Transcript.Id.WithVersion;
                int    score        = _conservationProvider.GetConservationScore(transcriptId, aaPos);
                if (score == -1) return; //don't add conservation scores
                scores.Add(1.0 * score / 100);
            }

            annotatedTranscript.ConservationScores = scores;
        }

        public void PreLoad(IChromosome chromosome, List<int> positions) => throw new NotImplementedException();

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

        private static void AddRegulatoryRegions(IAnnotatedVariant[] annotatedVariants, IIntervalForest<IRegulatoryRegion> regulatoryIntervalForest)
        {
            foreach (IAnnotatedVariant annotatedVariant in annotatedVariants)
            {
                if (!annotatedVariant.Variant.Behavior.NeedRegulatoryRegions) continue;

                // In case of insertions, the base(s) are assumed to be inserted at the end position
                // if this is an insertion just before the beginning of the regulatory element, this takes care of it
                IVariant variant      = annotatedVariant.Variant;
                int      variantBegin = variant.Type == VariantType.insertion ? variant.End : variant.Start;

                if (SkipLargeVariants(variantBegin, variant.End)) continue;

                IRegulatoryRegion[] regulatoryRegions =
                    regulatoryIntervalForest.GetAllOverlappingValues(variant.Chromosome.Index, variantBegin,
                        variant.End);
                if (regulatoryRegions == null) continue;

                foreach (IRegulatoryRegion regulatoryRegion in regulatoryRegions)
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
            _siftStream?.Dispose();
            _polyphenStream?.Dispose();
        }
    }
}