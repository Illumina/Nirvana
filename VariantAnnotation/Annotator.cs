using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
            var assemblies = new Dictionary<GenomeAssembly, string>();
            if (_taProvider           != null) assemblies[_taProvider.GenomeAssembly]           = _taProvider.Name;
            if (_saProviders          != null) assemblies[_saProviders.GenomeAssembly]          = _saProviders.Name;
            if (_sequenceProvider     != null) assemblies[_sequenceProvider.GenomeAssembly]     = _sequenceProvider.Name;
            if (_conservationProvider != null) assemblies[_conservationProvider.GenomeAssembly] = _conservationProvider.Name;

            if (assemblies.Count == 0) return GenomeAssembly.Unknown;
            if (assemblies.Count != 1) throw new InvalidDataException(GetGenomeAssemblyErrorMessage(assemblies));

            CheckPluginGenomeAssemblyConsistency(assemblies.First().Key);
            return assemblies.First().Key;
        }

        private static string GetGenomeAssemblyErrorMessage(Dictionary<GenomeAssembly, string> assemblies)
        {
            var sb = new StringBuilder();
            foreach (var assembly in assemblies) sb.AppendLine($"{assembly.Value} has genome assembly {assembly.Key}");
            return sb.ToString();
        }

        private void CheckPluginGenomeAssemblyConsistency(GenomeAssembly systemGenomeAssembly)
        {
            if (_plugins == null || !_plugins.Any()) return;

            foreach (var plugin in _plugins)
            {
                if (plugin.GenomeAssembly == systemGenomeAssembly || plugin.GenomeAssembly == GenomeAssembly.Unknown) continue;
                throw new InvalidDataException($"At least one plugin does not have the same genome assembly ({plugin.GenomeAssembly}) as the system genome assembly ({systemGenomeAssembly})");
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
