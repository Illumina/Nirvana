using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly HashSet<string> _affectedGenes;
        private readonly IGeneAnnotationProvider _geneAnnotationProvider;
        private bool _annotateMito;
        public GenomeAssembly GenomeAssembly { get; }
        private readonly IEnumerable<IPlugin> _plugins;

        public Annotator(IAnnotationProvider taProvider, ISequenceProvider sequenceProvider, IAnnotationProvider saProviders, IAnnotationProvider conservationProvider, IGeneAnnotationProvider geneAnnotationProvider, IEnumerable<IPlugin> plugins = null)
        {
            _saProviders = saProviders;
            _taProvider = taProvider;
            _sequenceProvider = sequenceProvider;
            _conservationProvider = conservationProvider;
            _geneAnnotationProvider = geneAnnotationProvider;
            _affectedGenes = new HashSet<string>();
            _plugins = plugins;
            GenomeAssembly = GetGenomeAssembly();            
        }

        private GenomeAssembly GetGenomeAssembly()
        {
            var assemblies = new Dictionary<GenomeAssembly, string>();
            if (_taProvider != null) assemblies[_taProvider.GenomeAssembly] = _taProvider.Name;
            if (_saProviders != null) assemblies[_saProviders.GenomeAssembly] = _saProviders.Name;
            if (_sequenceProvider != null) assemblies[_sequenceProvider.GenomeAssembly] = _sequenceProvider.Name;
            if (_conservationProvider != null) assemblies[_conservationProvider.GenomeAssembly] = _conservationProvider.Name;
            

            if (assemblies.Count == 0) return GenomeAssembly.Unknown;
            if (assemblies.Count == 1)
            {
                CheckPluginAssemblyConsistent(assemblies.First().Key);
                return assemblies.First().Key;
            }
            foreach (var assembly in assemblies)
            {
                Console.WriteLine($"{assembly.Value} has genome assembly {assembly.Key}");
            }
            throw new UserErrorException("Found more than one genome assembly represented in the selected data sources.");

        }

        private void CheckPluginAssemblyConsistent(GenomeAssembly assembly)
        {
            if (_plugins == null || !_plugins.Any()) return ;
            foreach (var plugin in _plugins)
            {
                var pluginAssembly = plugin.GetGenomeAssembly();
                if (pluginAssembly != null && pluginAssembly != assembly)
                {
                    throw new UserErrorException("Found more than one genome assembly represented in the selected data sources.");
                }  
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

            if (_plugins != null)
            {
                foreach (var plugin in _plugins)
                {
                    if (_sequenceProvider != null) plugin.Annotate(annotatedPosition, _sequenceProvider.Sequence);
                }
            }
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
        public void EnableMitochondrialAnnotation()
        {
            _annotateMito = true;
        }
    }
}
