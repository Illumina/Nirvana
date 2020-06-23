using System.Collections.Generic;
using System.Linq;
using ErrorHandling.Exceptions;
using Genome;
using OptimizedCore;
using RepeatExpansions;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace VariantAnnotation
{
    public sealed class Annotator : IAnnotator
    {
        private readonly IAnnotationProvider _saProvider;
        private readonly IAnnotationProvider _taProvider;
        private readonly IAnnotationProvider _lcrProvider;
        private readonly ISequenceProvider _sequenceProvider;
        private readonly IAnnotationProvider _conservationProvider;        
        private readonly IGeneAnnotationProvider _geneAnnotationProvider;
        private readonly IRepeatExpansionProvider _repeatExpansionProvider;
        private readonly HashSet<string> _affectedGenes;

        private bool _annotateMito;
        public GenomeAssembly Assembly { get; }

        public Annotator(IAnnotationProvider taProvider, ISequenceProvider sequenceProvider,
            IAnnotationProvider saProvider, IAnnotationProvider conservationProvider, IAnnotationProvider lcrProvider,
            IGeneAnnotationProvider geneAnnotationProvider, IRepeatExpansionProvider repeatExpansionProvider)
        {
            _saProvider              = saProvider;
            _taProvider              = taProvider;
            _sequenceProvider        = sequenceProvider;
            _conservationProvider    = conservationProvider;
            _lcrProvider             = lcrProvider;
            _geneAnnotationProvider  = geneAnnotationProvider;
            _repeatExpansionProvider = repeatExpansionProvider;
            _affectedGenes           = new HashSet<string>();
            Assembly                 = GetAssembly();
        }

        private GenomeAssembly GetAssembly()
        {
            var assemblies = new Dictionary<GenomeAssembly, List<string>>();
            AddAssembly(assemblies, _taProvider);
            AddAssembly(assemblies, _saProvider);
            AddAssembly(assemblies, _sequenceProvider);
            AddAssembly(assemblies, _conservationProvider);

            if (assemblies.Count == 0) return GenomeAssembly.Unknown;
            if (assemblies.Count != 1) throw new UserErrorException(GetAssemblyErrorMessage(assemblies));

            return assemblies.First().Key;
        }

        private static void AddAssembly(IDictionary<GenomeAssembly, List<string>> assemblies, IProvider provider)
        {
            if (provider == null) return;
            if (assemblies.TryGetValue(provider.Assembly, out List<string> assemblyList)) assemblyList.Add(provider.Name);
            else assemblies[provider.Assembly] = new List<string> { provider.Name };
        }

        private static string GetAssemblyErrorMessage(Dictionary<GenomeAssembly, List<string>> assemblies)
        {
            var sb = StringBuilderCache.Acquire();
            sb.AppendLine("Not all of the data sources have the same genome assembly:");
            foreach ((GenomeAssembly genomeAssembly, List<string> dataSources) in assemblies) sb.AppendLine($"- Using {genomeAssembly}: {string.Join(", ", dataSources)}");
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public IAnnotatedPosition Annotate(IPosition position)
        {
            if (position == null) return null;
            IAnnotatedVariant[] annotatedVariants = GetAnnotatedVariants(position.Variants);
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            if (annotatedPosition.AnnotatedVariants == null
                || annotatedPosition.AnnotatedVariants.Length == 0
                || position.Chromosome.UcscName == "chrM" && !_annotateMito
                ) return annotatedPosition;

            _sequenceProvider?.Annotate(annotatedPosition);
            _saProvider?.Annotate(annotatedPosition);
            _lcrProvider?.Annotate(annotatedPosition);
            _repeatExpansionProvider?.Annotate(annotatedPosition);
            _conservationProvider?.Annotate(annotatedPosition);
            _taProvider.Annotate(annotatedPosition);

            TrackAffectedGenes(annotatedPosition);
            return annotatedPosition;
        }

        private void TrackAffectedGenes(IAnnotatedPosition annotatedPosition)
        {
            if (_geneAnnotationProvider == null) return;

            foreach (var variant in annotatedPosition.AnnotatedVariants)
            {
                AddGenesFromTranscripts(variant.Transcripts);
            }
        }

        private void AddGenesFromTranscripts(IList<IAnnotatedTranscript> transcripts)
        {
            foreach (var transcript in transcripts)
            {
                if (IsFlankingTranscript(transcript)) continue;
                _affectedGenes.Add(transcript.Transcript.Gene.Symbol);
            }
        }

        private static bool IsFlankingTranscript(IAnnotatedTranscript transcript)
        {
            if (transcript.Consequences == null) return false;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var consequence in transcript.Consequences)
            {
                if (consequence == ConsequenceTag.downstream_gene_variant ||
                    consequence == ConsequenceTag.upstream_gene_variant) return true;
            }

            return false;
        }

        internal static IAnnotatedVariant[] GetAnnotatedVariants(IVariant[] variants)
        {
            if (variants?[0].Behavior == null) return null;
            int numVariants = variants.Length;
            var annotatedVariants = new IAnnotatedVariant[numVariants];
            for (var i = 0; i < numVariants; i++) annotatedVariants[i] = new AnnotatedVariant(variants[i]);
            return annotatedVariants;
        }

        public IEnumerable<string> GetGeneAnnotations()
        {
            var geneAnnotations = new List<string>();

            foreach (string gene in _affectedGenes.OrderBy(x => x))
            {
                string annotation = _geneAnnotationProvider.Annotate(gene);
                if (string.IsNullOrEmpty(annotation)) continue;
                geneAnnotations.Add(annotation);
            }

            return geneAnnotations.Count > 0 ? geneAnnotations : null;
        }

        public void EnableMitochondrialAnnotation() => _annotateMito = true;
    }
}
