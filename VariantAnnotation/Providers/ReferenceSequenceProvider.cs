using System.Collections.Generic;
using System.IO;
using Genome;
using Intervals;
using ReferenceSequence.IO;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using Versioning;

namespace VariantAnnotation.Providers
{
    public sealed class ReferenceSequenceProvider : ISequenceProvider
    {
        public Dictionary<string, Chromosome> RefNameToChromosome  => _sequenceReader.RefNameToChromosome;
        public Chromosome[]                   Chromosomes          => _sequenceReader.Chromosomes;
        public GenomeAssembly                 Assembly             => _sequenceReader.Assembly;
        public ISequence                      Sequence             { get; }

        public string Name { get; } = "Reference sequence provider";
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; } = null;

        private          Chromosome               _currentChromosome = Chromosome.GetEmpty("bob");
        private readonly CompressedSequenceReader _sequenceReader;

        public ReferenceSequenceProvider(Stream stream)
        {
            _sequenceReader = new CompressedSequenceReader(stream);
            Sequence        = _sequenceReader.Sequence;
        }

        public void Annotate(AnnotatedPosition annotatedPosition)
        {
            if (annotatedPosition.AnnotatedVariants == null) return;

            annotatedPosition.CytogeneticBand = Sequence.CytogeneticBands.Find(annotatedPosition.Position.Chromosome,
                annotatedPosition.Position.Start, annotatedPosition.Position.End);

            string refSeqAccession = annotatedPosition.Position.Chromosome.RefSeqAccession;
            foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                annotatedVariant.HgvsgNotation = HgvsgNotation.GetNotation(refSeqAccession, annotatedVariant.Variant, Sequence, new Interval(0, Sequence.Length));
            }
        }

        public void LoadChromosome(Chromosome chromosome)
        {
            if (chromosome.Index == _currentChromosome.Index) return;
            _sequenceReader.GetCompressedSequence(chromosome);
            _currentChromosome = chromosome;
        }
    }
}