using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using VariantAnnotation.Algorithms;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Caches;
using VariantAnnotation.Caches.DataStructures;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.TranscriptAnnotation;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.Providers
{
    public sealed class TranscriptAnnotationProvider : IAnnotationProvider
    {
        private const int MaxSvLengthForRegulatoryRegionAnnotation = 50000;

        private static readonly string[] _siftDescriptions =
        {
            "tolerated", "deleterious", "tolerated - low confidence",
            "deleterious - low confidence"
        };

        private static readonly string[] _polyphenDescriptions =
            {"probably damaging", "possibly damaging", "benign", "unknown"};
        private readonly ITranscriptCache _transcriptCache;
        private readonly ISequence _sequence;

	    public string Name { get; }
	    public GenomeAssembly GenomeAssembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        private readonly PredictionCacheReader _siftReader;
        private readonly PredictionCacheReader _polyphenReader;
        private IPredictionCache _siftCache;
        private IPredictionCache _polyphenCache;
        private ushort _currentRefIndex = ushort.MaxValue;

        public TranscriptAnnotationProvider(string pathPrefix,  ISequenceProvider sequenceProvider)
        {
	        Name = "Transcript annotation provider";
            _sequence          = sequenceProvider.Sequence;
            _transcriptCache   = InitiateCache(FileUtilities.GetReadStream(CacheConstants.TranscriptPath(pathPrefix)), sequenceProvider.GetChromosomeIndexDictionary(), sequenceProvider.GenomeAssembly, sequenceProvider.NumRefSeqs);
            GenomeAssembly     = _transcriptCache.GenomeAssembly;
            DataSourceVersions = _transcriptCache.DataSourceVersions;

	        _siftReader     = new PredictionCacheReader(FileUtilities.GetReadStream(CacheConstants.SiftPath(pathPrefix)),_siftDescriptions);
            _polyphenReader = new PredictionCacheReader(FileUtilities.GetReadStream(CacheConstants.PolyPhenPath(pathPrefix)),_polyphenDescriptions);
        }

        private static TranscriptCache InitiateCache(Stream stream,
            IDictionary<ushort, IChromosome> chromosomeIndexDictionary, GenomeAssembly genomeAssembly, ushort numRefSeq)
        {
            TranscriptCache cache;
            using (var reader = new TranscriptCacheReader(stream, genomeAssembly, numRefSeq)) cache = reader.Read(chromosomeIndexDictionary);
            return cache;
        }

        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            if (annotatedPosition.AnnotatedVariants == null || annotatedPosition.AnnotatedVariants.Length == 0) return;

            var refIndex = annotatedPosition.Position.Chromosome.Index;
            LoadPredictionCaches(refIndex);

            AddRegulatoryRegions(annotatedPosition);
            AddTranscripts(annotatedPosition);
        }




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
            _siftCache = null;
            _polyphenCache = null;
            _currentRefIndex = ushort.MaxValue; 
        }

        private void AddTranscripts(IAnnotatedPosition annotatedPosition)
        {
            var overlappingTranscripts = _transcriptCache.GetOverlappingFlankingTranscripts(annotatedPosition.Position);

            if (overlappingTranscripts == null)
            {
                // todo: handle intergenic variants
                return;
            }

            foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                var geneFusionCandidates = GetGeneFusionCandiates(annotatedVariant.Variant.BreakEnds);
                var annotatedTranscripts = new List<IAnnotatedTranscript>();

                TranscriptAnnotationFactory.GetAnnotatedTranscripts(annotatedVariant.Variant, overlappingTranscripts,
                    _sequence, annotatedTranscripts, annotatedVariant.OverlappingGenes,
                    annotatedVariant.OverlappingTranscripts,_siftCache,_polyphenCache, geneFusionCandidates);

                if (annotatedTranscripts.Count == 0) continue;

                foreach (var annotatedTranscript in annotatedTranscripts)
                {
                    if (annotatedTranscript.Transcript.Source == Source.Ensembl)
                        annotatedVariant.EnsemblTranscripts.Add(annotatedTranscript);
                    else annotatedVariant.RefSeqTranscripts.Add(annotatedTranscript);
                }
            }
        }

        private IEnumerable<ITranscript> GetGeneFusionCandiates(IBreakEnd[] breakEnds)
        {
            if (breakEnds == null || breakEnds.Length == 0) return null;

            var geneFusionCandidates = new List<ITranscript>();
            foreach (var breakEnd in breakEnds)
            {
                var candiates = _transcriptCache.GetOverlappingTranscripts(breakEnd.Chromosome2,
                    breakEnd.Position2, breakEnd.Position2);
                if (candiates != null) geneFusionCandidates.AddRange(candiates);
            }

            return geneFusionCandidates;
        }

        private void AddRegulatoryRegions(IAnnotatedPosition annotatedPosition)
        {
            var overlappingRegulatoryRegions = _transcriptCache.GetOverlappingRegulatoryRegions(annotatedPosition.Position);

            if (overlappingRegulatoryRegions == null) return;

            foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                // In case of insertions, the base(s) are assumed to be inserted at the end position

                // if this is an insertion just before the beginning of the regulatory element, this takes care of it
                var variant      = annotatedVariant.Variant;
                var variantEnd   = variant.End;
                var variantBegin = variant.Type == VariantType.insertion ? variant.End : variant.Start;

                // disable regulatory region for SV larger than 50kb
                if (variantEnd - variantBegin + 1 > MaxSvLengthForRegulatoryRegionAnnotation) continue;

                foreach (var regulatoryRegion in overlappingRegulatoryRegions)
                {
                    if (!variant.Overlaps(regulatoryRegion)) continue;

                    // if the insertion is at the end, its past the feature and therefore not overlapping
                    if (variant.Type == VariantType.insertion && variantEnd == regulatoryRegion.End) continue;

                    annotatedVariant.RegulatoryRegions.Add(RegulatoryRegionAnnotator.Annotate(variant, regulatoryRegion));
                }
            }
        }
    }
}