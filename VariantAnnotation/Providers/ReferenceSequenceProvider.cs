using System.Collections.Generic;
using System.IO;
using Genome;
using Intervals;
using VariantAnnotation.AnnotatedPositions;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Sequence;

namespace VariantAnnotation.Providers
{
    public sealed class ReferenceSequenceProvider : ISequenceProvider
    {
        public IDictionary<string, IChromosome> RefNameToChromosome => _sequenceReader.RefNameToChromosome;
        public IDictionary<ushort, IChromosome> RefIndexToChromosome => _sequenceReader.RefIndexToChromosome;
        public GenomeAssembly Assembly => _sequenceReader.Assembly;
        public ISequence Sequence => _sequenceReader.Sequence;

        public string Name { get; } = "Reference sequence provider";
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; } = null;

        private readonly CytogeneticBands _cytogeneticBands;
        private IChromosome _currentChromosome;
        private readonly CompressedSequenceReader _sequenceReader;

        public ReferenceSequenceProvider(Stream stream)
        {
            _currentChromosome = new EmptyChromosome(string.Empty);
            _sequenceReader = new CompressedSequenceReader(stream);
            _cytogeneticBands = new CytogeneticBands(_sequenceReader.CytogeneticBands);
        }

        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            if (annotatedPosition.AnnotatedVariants == null) return;

            annotatedPosition.CytogeneticBand = _cytogeneticBands.GetCytogeneticBand(annotatedPosition.Position.Chromosome, annotatedPosition.Position.Start,
                annotatedPosition.Position.End);

            if (annotatedPosition.Position.Chromosome.UcscName != "chrM") return;

            const string assertionNumber = "NC_012920.1";
            foreach (var annotatedVariant in annotatedPosition.AnnotatedVariants)
            {
                annotatedVariant.HgvsgNotation = HgvsgNotation.GetNotation(assertionNumber, annotatedVariant.Variant, Sequence, new Interval(0, Sequence.Length));
            }
        }

        public void PreLoad(IChromosome chromosome, List<int> positions)
        {
            throw new System.NotImplementedException();
        }

        public void LoadChromosome(IChromosome chromosome)
        {
            if (chromosome.Index == _currentChromosome.Index) return;
            _sequenceReader.GetCompressedSequence(chromosome);
            _currentChromosome = chromosome;
        }

        public void Dispose()
        {
            _sequenceReader?.Dispose();
        }
    }
}