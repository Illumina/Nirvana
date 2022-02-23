using System.Collections.Generic;
using System.IO;
using Cache.Data;
using Cache.IO;
using Cache.Utilities;
using Genome;
using Intervals;
using IO;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.TranscriptAnnotation;
using Variants;
using Versioning;

namespace VariantAnnotation.Providers
{
    public sealed class TranscriptAnnotationProvider : IAnnotationProvider
    {
        private readonly List<TranscriptCache> _transcriptCaches = new();
        private readonly ISequence             _sequence;

        public GenomeAssembly Assembly { get; }

        public           string                          Name               => "Transcript annotation provider";
        public           IEnumerable<IDataSourceVersion> DataSourceVersions => _dataSourceVersions;
        private readonly List<IDataSourceVersion>        _dataSourceVersions = new();

        // TODO: take concurrency into account with these data structures
        private readonly List<Transcript>       _transcripts           = new();
        private readonly List<RegulatoryRegion> _regulatoryRegions     = new();
        private readonly List<Transcript>       _geneFusionTranscripts = new();
        private readonly HashSet<Transcript>    _geneFusionCandidates  = new();

        private readonly PsaProvider _psaProvider;

        public TranscriptAnnotationProvider(string cacheDir, ISequenceProvider sequenceProvider,
            PsaProvider psaProvider)
        {
            _sequence    = sequenceProvider.Sequence;
            _psaProvider = psaProvider;
            Assembly     = sequenceProvider.Assembly;

            Dictionary<int, string> hgncIdToSymbol = GetUpdatedGeneSymbols(cacheDir);
            
            string[] cacheFiles = CacheFileUtilities.GetCacheFiles(cacheDir, Assembly);

            foreach (string cacheFile in cacheFiles)
            {
                var fileStream      = FileUtilities.GetReadStream(cacheFile);
                var transcriptCache = TranscriptCache.Read(fileStream, sequenceProvider.Chromosomes, hgncIdToSymbol);
                _dataSourceVersions.Add(transcriptCache.DataSourceVersion);
                _transcriptCaches.Add(transcriptCache);
            }
        }

        private static Dictionary<int, string> GetUpdatedGeneSymbols(string cacheDir)
        {
            string geneSymbolsPath = CacheFileUtilities.GetGenomeSymbolsPath(cacheDir);
            if (!File.Exists(geneSymbolsPath)) return new Dictionary<int, string>();

            using var reader = new GeneSymbolReader(FileUtilities.GetReadStream(geneSymbolsPath));

            HgncGeneSymbol[] hgncEntries = reader.GetHgncGeneSymbols();

            var hgncIdToSymbol = new Dictionary<int, string>(hgncEntries.Length);
            foreach ((int hgncId, string geneSymbol) in hgncEntries) hgncIdToSymbol[hgncId] = geneSymbol;
            return hgncIdToSymbol;
        }

        public void Annotate(AnnotatedPosition annotatedPosition)
        {
            if (annotatedPosition.AnnotatedVariants == null || annotatedPosition.AnnotatedVariants.Length == 0) return;

            AddRegulatoryRegions(annotatedPosition.AnnotatedVariants);
            AddTranscripts(annotatedPosition.AnnotatedVariants);
        }

        private void AddTranscripts(AnnotatedVariant[] annotatedVariants)
        {
            foreach (var annotatedVariant in annotatedVariants)
            {
                var variant = annotatedVariant.Variant;
                if (variant.Chromosome.IsEmpty) continue;

                GetGeneFusionCandidates(variant.BreakEnds);
                GetFlankingTranscripts(variant);
                if (_transcripts.Count == 0) continue;

                List<AnnotatedTranscript> annotatedTranscripts =
                    TranscriptAnnotationFactory.GetAnnotatedTranscripts(variant, _transcripts, _sequence,
                        _geneFusionCandidates);
                if (annotatedTranscripts.Count == 0) continue;

                foreach (AnnotatedTranscript annotatedTranscript in annotatedTranscripts)
                    AddPredictionScores(annotatedTranscript);

                foreach (var annotatedTranscript in annotatedTranscripts)
                    annotatedVariant.Transcripts.Add(annotatedTranscript);
            }
        }

        private void AddPredictionScores(AnnotatedTranscript annotatedTranscript)
        {
            if (_psaProvider                       == null) return;
            if (annotatedTranscript.MappedPosition == null) return;

            string refAa = annotatedTranscript.ReferenceAminoAcids;
            string altAa = annotatedTranscript.AlternateAminoAcids;
            if (refAa == null || altAa == null || refAa.Length != 1 || altAa.Length != 1) return;

            _psaProvider.Annotate(annotatedTranscript, annotatedTranscript.MappedPosition.ProteinStart,
                annotatedTranscript.AlternateAminoAcids[0]);
        }

        private void GetGeneFusionCandidates(IBreakEnd[] breakEnds)
        {
            if (breakEnds == null || breakEnds.Length == 0) return;

            _geneFusionCandidates.Clear();

            foreach (var breakEnd in breakEnds)
            {
                if (breakEnd.Piece2.Chromosome.IsEmpty) continue;
                GetGeneFusionTranscripts(breakEnd.Piece2.Chromosome.Index, breakEnd.Piece2.Position);
                if (_geneFusionTranscripts.Count == 0) continue;

                foreach (var transcript in _geneFusionTranscripts) _geneFusionCandidates.Add(transcript);
            }
        }

        private void GetFlankingTranscripts(IVariant variant)
        {
            _transcripts.Clear();

            ushort refIndex = variant.Chromosome.Index;
            (int start, int end) = GetFlankingRegion(variant);

            foreach (var transcriptCache in _transcriptCaches)
            {
                transcriptCache.AddTranscripts(refIndex, start, end, _transcripts);
            }
        }

        private void GetGeneFusionTranscripts(ushort refIndex, int position)
        {
            _geneFusionTranscripts.Clear();

            foreach (var transcriptCache in _transcriptCaches)
            {
                transcriptCache.AddTranscripts(refIndex, position, position, _geneFusionTranscripts);
            }
        }

        private void GetRegulatoryRegions(ushort refIndex, int start, int end)
        {
            _regulatoryRegions.Clear();
            foreach (var transcriptCache in _transcriptCaches)
            {
                transcriptCache.AddRegulatoryRegions(refIndex, start, end, _regulatoryRegions);
            }
        }

        internal static (int Start, int End) GetFlankingRegion(IInterval variant)
        {
            int start = variant.Start - OverlapBehavior.FlankingLength;
            int end   = variant.End   + OverlapBehavior.FlankingLength;

            if (start < 1) start = 1;

            return (start, end);
        }

        private void AddRegulatoryRegions(AnnotatedVariant[] annotatedVariants)
        {
            foreach (var annotatedVariant in annotatedVariants)
            {
                var variant = annotatedVariant.Variant;
                if (variant.Chromosome.IsEmpty) continue;

                // In case of insertions, the base(s) are assumed to be inserted at the end position
                // if this is an insertion just before the beginning of the regulatory element, this takes care of it
                int variantBegin = variant.Type == VariantType.insertion ? variant.End : variant.Start;

                if (SkipLargeVariants(variantBegin, variant.End)) continue;

                GetRegulatoryRegions(variant.Chromosome.Index, variantBegin, variant.End);
                if (_regulatoryRegions.Count == 0) continue;

                foreach (var regulatoryRegion in _regulatoryRegions)
                {
                    // if the insertion is at the end, its past the feature and therefore not overlapping
                    if (variant.Type == VariantType.insertion && variant.End == regulatoryRegion.End) continue;

                    annotatedVariant.RegulatoryRegions.Add(
                        RegulatoryRegionAnnotator.Annotate(variant, regulatoryRegion));
                }
            }
        }

        private const int MaxSvLengthForRegulatoryRegionAnnotation = 50000;

        private static bool SkipLargeVariants(int begin, int end) =>
            end - begin + 1 > MaxSvLengthForRegulatoryRegionAnnotation;
    }
}