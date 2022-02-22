using System.Collections.Generic;
using System.Linq;
using ErrorHandling.Exceptions;
using Genome;
using OptimizedCore;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using Variants;
using Vcf;

namespace VariantAnnotation
{
    public sealed class Annotator
    {
        private readonly ISaAnnotationProvider     _saProvider;
        private readonly IAnnotationProvider       _taProvider;
        private readonly ReferenceSequenceProvider _sequenceProvider;
        private readonly IAnnotationProvider       _conservationProvider;
        private readonly IGeneAnnotationProvider   _geneAnnotationProvider;
        private readonly HashSet<string>           _affectedGenes;

        private bool           _annotateMito;
        public  GenomeAssembly Assembly { get; }

        public Annotator(IAnnotationProvider taProvider, ReferenceSequenceProvider sequenceProvider,
            ISaAnnotationProvider saProvider, IAnnotationProvider conservationProvider,
            IGeneAnnotationProvider geneAnnotationProvider)
        {
            _saProvider             = saProvider;
            _taProvider             = taProvider;
            _sequenceProvider       = sequenceProvider;
            _conservationProvider   = conservationProvider;
            _geneAnnotationProvider = geneAnnotationProvider;
            _affectedGenes          = new HashSet<string>();
            Assembly                = GetAssembly();
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
            if (assemblies.TryGetValue(provider.Assembly, out var assemblyList)) assemblyList.Add(provider.Name);
            else assemblies[provider.Assembly] = new List<string> {provider.Name};
        }

        private static string GetAssemblyErrorMessage(Dictionary<GenomeAssembly, List<string>> assemblies)
        {
            var sb = StringBuilderCache.Acquire();
            sb.AppendLine("Not all of the data sources have the same genome assembly:");
            foreach (var assembly in assemblies)
                sb.AppendLine($"- Using {assembly.Key}: {string.Join(", ", assembly.Value)}");
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public AnnotatedPosition Annotate(Position position)
        {
            if (position == null) return null;
            AnnotatedVariant[] annotatedVariants = GetAnnotatedVariants(position.Variants);
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            if (annotatedPosition.AnnotatedVariants        == null ||
                annotatedPosition.AnnotatedVariants.Length == 0    ||
                position.Chromosome.UcscName == "chrM" &&
                !_annotateMito) return annotatedPosition;

            _sequenceProvider?.Annotate(annotatedPosition);
            _saProvider?.Annotate(annotatedPosition);
            _conservationProvider?.Annotate(annotatedPosition);
            _taProvider.Annotate(annotatedPosition);

            TrackAffectedGenes(annotatedPosition);
            return annotatedPosition;
        }

        private void TrackAffectedGenes(AnnotatedPosition annotatedPosition)
        {
            if (_geneAnnotationProvider == null) return;

            foreach (var variant in annotatedPosition.AnnotatedVariants)
            {
                AddGenesFromTranscripts(variant.Transcripts);
            }
        }

        private void AddGenesFromTranscripts(List<AnnotatedTranscript> transcripts)
        {
            foreach (var transcript in transcripts)
            {
                if (IsFlankingTranscript(transcript)) continue;
                _affectedGenes.Add(transcript.Transcript.Gene.Symbol);
            }
        }

        private static bool IsFlankingTranscript(AnnotatedTranscript transcript)
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

        internal static AnnotatedVariant[] GetAnnotatedVariants(IVariant[] variants)
        {
            if (variants?[0].Behavior == null) return null;
            
            int numVariants                                            = variants.Length;
            var annotatedVariants                                      = new AnnotatedVariant[numVariants];
            for (var i = 0; i < numVariants; i++) annotatedVariants[i] = new AnnotatedVariant(variants[i]);
            return annotatedVariants;
        }

        public IEnumerable<string> GetGeneAnnotations()
        {
            var geneAnnotations = new List<string>();

            foreach (var gene in _affectedGenes.OrderBy(x => x))
            {
                var annotation = _geneAnnotationProvider.Annotate(gene);
                if (string.IsNullOrEmpty(annotation)) continue;
                geneAnnotations.Add(annotation);
            }

            return geneAnnotations.Count > 0 ? geneAnnotations : null;
        }

        public void EnableMitochondrialAnnotation() => _annotateMito = true;
    }
}