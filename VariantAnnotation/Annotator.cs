using System.Collections.Generic;
using System.Linq;
using CommonUtilities;
using ErrorHandling.Exceptions;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.GeneAnnotation;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Plugins;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation
{
    public sealed class Annotator : IAnnotator
    {
        private readonly IAnnotationProvider _saProviders;
        private readonly IAnnotationProvider _taProvider;
        private readonly ISequenceProvider _sequenceProvider;
        private readonly IAnnotationProvider _conservationProvider;        
        private readonly IGeneAnnotationProvider _geneAnnotationProvider;
        private readonly IEnumerable<IPlugin> _plugins;
        private readonly HashSet<string> _affectedGenes;

        private bool _annotateMito;
        public GenomeAssembly GenomeAssembly { get; }        

        public Annotator(IAnnotationProvider taProvider, ISequenceProvider sequenceProvider,
            IAnnotationProvider saProviders, IAnnotationProvider conservationProvider,
            IGeneAnnotationProvider geneAnnotationProvider, IEnumerable<IPlugin> plugins = null)
        {
            _saProviders            = saProviders;
            _taProvider             = taProvider;
            _sequenceProvider       = sequenceProvider;
            _conservationProvider   = conservationProvider;
            _geneAnnotationProvider = geneAnnotationProvider;
            _affectedGenes          = new HashSet<string>();
            _plugins                = plugins;
            GenomeAssembly          = GetGenomeAssembly();
        }

        private GenomeAssembly GetGenomeAssembly()
        {
            var assemblies = new Dictionary<GenomeAssembly, List<string>>();
            AddGenomeAssembly(assemblies, _taProvider);
            AddGenomeAssembly(assemblies, _saProviders);
            AddGenomeAssembly(assemblies, _sequenceProvider);
            AddGenomeAssembly(assemblies, _conservationProvider);

            if (assemblies.Count == 0) return GenomeAssembly.Unknown;
            if (assemblies.Count != 1) throw new UserErrorException(GetGenomeAssemblyErrorMessage(assemblies));

            CheckPluginGenomeAssemblyConsistency(assemblies.First().Key);
            return assemblies.First().Key;
        }

        private static void AddGenomeAssembly(Dictionary<GenomeAssembly, List<string>> assemblies, IProvider provider)
        {
            if (provider == null) return;
            if (assemblies.TryGetValue(provider.GenomeAssembly, out var assemblyList)) assemblyList.Add(provider.Name);
            else assemblies[provider.GenomeAssembly] = new List<string> { provider.Name };
        }

        private static string GetGenomeAssemblyErrorMessage(Dictionary<GenomeAssembly, List<string>> assemblies)
        {
            var sb = StringBuilderCache.Acquire();
            sb.AppendLine("Not all of the data sources have the same genome assembly:");
            foreach (var assembly in assemblies) sb.AppendLine($"- Using {assembly.Key}: {string.Join(", ", assembly.Value)}");
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        private void CheckPluginGenomeAssemblyConsistency(GenomeAssembly systemGenomeAssembly)
        {
            if (_plugins == null || !_plugins.Any()) return;

            foreach (var plugin in _plugins)
            {
                if (plugin.GenomeAssembly == systemGenomeAssembly || plugin.GenomeAssembly == GenomeAssembly.Unknown) continue;
                throw new UserErrorException($"At least one plugin does not have the same genome assembly ({plugin.GenomeAssembly}) as the system genome assembly ({systemGenomeAssembly})");
            }
        }

        public IAnnotatedPosition Annotate(IPosition position)
        {
            if (position == null) return null;

            var annotatedVariants = GetAnnotatedVariants(position.Variants);
            var annotatedPosition = new AnnotatedPosition(position, annotatedVariants);

            if (annotatedPosition.AnnotatedVariants == null
                || annotatedPosition.AnnotatedVariants.Length == 0
                || position.Chromosome.UcscName == "chrM" && !_annotateMito
                ) return annotatedPosition;

            _sequenceProvider?.Annotate(annotatedPosition);
            _saProviders?.Annotate(annotatedPosition);
            _conservationProvider?.Annotate(annotatedPosition);
            _taProvider.Annotate(annotatedPosition);
            _plugins?.Annotate(annotatedPosition, _sequenceProvider?.Sequence);

            TrackAffectedGenes(annotatedPosition);
            return annotatedPosition;
        }

        internal void TrackAffectedGenes(IAnnotatedPosition annotatedPosition)
        {
            if (_geneAnnotationProvider == null) return;

            foreach (var variant in annotatedPosition.AnnotatedVariants)
            {
                if (variant.OverlappingGenes != null)
                {
                    foreach (var gene in variant.OverlappingGenes)
                    {
                        _affectedGenes.Add(gene);
                    }
                }

                foreach (var ensemblTranscript in variant.EnsemblTranscripts)
                {
                    if (!ensemblTranscript.Consequences.Contains(ConsequenceTag.downstream_gene_variant) &&
                        !ensemblTranscript.Consequences.Contains(ConsequenceTag.upstream_gene_variant))
                        _affectedGenes.Add(ensemblTranscript.Transcript.Gene.Symbol);
                }

                foreach (var refSeqTranscript in variant.RefSeqTranscripts)
                {
                    if (!refSeqTranscript.Consequences.Contains(ConsequenceTag.downstream_gene_variant) &&
                        !refSeqTranscript.Consequences.Contains(ConsequenceTag.upstream_gene_variant))
                        _affectedGenes.Add(refSeqTranscript.Transcript.Gene.Symbol);
                }
            }
        }

        internal static IAnnotatedVariant[] GetAnnotatedVariants(IVariant[] variants)
        {
            if (variants?[0].Behavior == null) return null;
            var numVariants = variants.Length;
            var annotatedVariants = new IAnnotatedVariant[numVariants];
            for (var i = 0; i < numVariants; i++) annotatedVariants[i] = new AnnotatedVariant(variants[i]);
            return annotatedVariants;
        }

        public IList<IAnnotatedGene> GetAnnotatedGenes() => GeneAnnotator.Annotate(_affectedGenes, _geneAnnotationProvider);

        public void EnableMitochondrialAnnotation() => _annotateMito = true;
    }

    internal static class PluginExtensions
    {
        public static void Annotate(this IEnumerable<IPlugin> plugins, IAnnotatedPosition annotatedPosition,
            ISequence sequence)
        {
            if (sequence == null) return;
            foreach (var plugin in plugins) plugin.Annotate(annotatedPosition, sequence);
        }
    }
}
