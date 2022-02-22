using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.Providers;
using Versioning;

namespace UnitTests.TestDataStructures
{
    public sealed class SimpleSequence : ISequence
    {
        private readonly string _sequence;
        private readonly int    _zeroBasedStartOffset;
        public           int    Length           => _zeroBasedStartOffset + _sequence.Length;
        public           Band[] CytogeneticBands => null;

        public SimpleSequence(string s, int zeroBasedStartOffset = 0)
        {
            _zeroBasedStartOffset = zeroBasedStartOffset;
            _sequence             = s;
        }

        public string Sequence => _sequence;

        public string Substring(int offset, int length)
        {
            if (offset - _zeroBasedStartOffset + length > _sequence.Length ||
                offset                                  < _zeroBasedStartOffset) return "";
            return _sequence.Substring(offset - _zeroBasedStartOffset, length);
        }
    }

    public sealed class SimpleSequenceProvider : ISequenceProvider
    {
        public string                          Name               { get; }
        public GenomeAssembly                  Assembly           { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }

        public ISequence                      Sequence            { get; }
        public Dictionary<string, Chromosome> RefNameToChromosome { get; }
        public Chromosome[]                   Chromosomes         { get; }

        public void LoadChromosome(Chromosome chromosome)
        {
        }

        public SimpleSequenceProvider(GenomeAssembly assembly, ISequence sequence,
            Dictionary<string, Chromosome> refNameToChromosome)
        {
            Assembly            = assembly;
            Sequence            = sequence;
            RefNameToChromosome = refNameToChromosome;
        }
    }
}