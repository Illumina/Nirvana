using System.Collections.Generic;
using System.IO;
using VariantAnnotation.Caches;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.TranscriptAnnotation;
using VariantAnnotation.Utilities;

namespace Piano
{
    public class PianoAnnotationProvider:IAnnotationProvider
    {
        public string Name { get; }
        public GenomeAssembly GenomeAssembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }
        private readonly ITranscriptCache _transcriptCache;
        private readonly ISequence _sequence;
        private ushort _currentRefIndex = ushort.MaxValue;


        public PianoAnnotationProvider(string pathPrefix, ISequenceProvider sequenceProvider)
        {
            Name = "Transcript annotation provider";
            _sequence = sequenceProvider.Sequence;
            _transcriptCache = InitiateCache(FileUtilities.GetReadStream(CacheConstants.TranscriptPath(pathPrefix)), sequenceProvider.GetChromosomeIndexDictionary(), sequenceProvider.GenomeAssembly, sequenceProvider.NumRefSeqs);
            GenomeAssembly = _transcriptCache.GenomeAssembly;
            DataSourceVersions = _transcriptCache.DataSourceVersions;


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
            _currentRefIndex = refIndex;
        }

        private void ClearCache()
        {
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
                var annotatedTranscripts = new List<IAnnotatedTranscript>();

                PianoAnnotationUtils.GetAnnotatedTranscripts(annotatedVariant.Variant, overlappingTranscripts,
                    _sequence, annotatedTranscripts);

                if (annotatedTranscripts.Count == 0) continue;

                foreach (var annotatedTranscript in annotatedTranscripts)
                {
                    if (annotatedTranscript.Transcript.Source == Source.Ensembl)
                        annotatedVariant.EnsemblTranscripts.Add(annotatedTranscript);
                    else annotatedVariant.RefSeqTranscripts.Add(annotatedTranscript);
                }
            }
        }

    }
}