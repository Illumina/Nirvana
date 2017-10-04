using System;
using System.Collections.Generic;
using System.Linq;
using ErrorHandling.Exceptions;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;

namespace Piano
{
    public class PianoAnnotator:IAnnotator
    {

        private readonly IAnnotationProvider _taProvider;
        private readonly ISequenceProvider _sequenceProvider;
        private bool _annotateMito;
        public GenomeAssembly GenomeAssembly { get; }

        public PianoAnnotator(IAnnotationProvider taProvider, ISequenceProvider sequenceProvider)
        {
            _taProvider = taProvider;
            _sequenceProvider = sequenceProvider;

            GenomeAssembly = GetGenomeAssembly();

        }

        private GenomeAssembly GetGenomeAssembly()
        {
            var assemblies = new Dictionary<GenomeAssembly, string>();
            if (_taProvider != null) assemblies[_taProvider.GenomeAssembly] = _taProvider.Name;
            if (_sequenceProvider != null) assemblies[_sequenceProvider.GenomeAssembly] = _sequenceProvider.Name;

            if (assemblies.Count == 0) return GenomeAssembly.Unknown;
            if (assemblies.Count == 1) return assemblies.First().Key;
            foreach (var assembly in assemblies)
            {
                Console.WriteLine($"{assembly.Value} has genome assembly {assembly.Key}");
            }
            throw new InconsistantGenomeAssemblyException();
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

            _taProvider.Annotate(annotatedPosition);

            return annotatedPosition;
        }
        private static IAnnotatedVariant[] GetAnnotatedVariants(IVariant[] variants)
        {
            if (variants?[0].Behavior == null) return null;

            var numVariants = variants.Length;
            var annotatedVariants = new IAnnotatedVariant[numVariants];
            for (var i = 0; i < numVariants; i++) annotatedVariants[i] = new AnnotatedVariant(variants[i]);

            return annotatedVariants;
        }
        public IList<IAnnotatedGene> GetAnnotatedGenes()
        {
            return null;
        }

        public void EnableMitochondrialAnnotation()
        {
            _annotateMito = true;
        }
    }
}