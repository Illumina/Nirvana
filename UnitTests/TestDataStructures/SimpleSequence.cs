using System.Collections.Generic;
using Genome;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;

namespace UnitTests.TestDataStructures
{
    public sealed class SimpleSequence : ISequence
    {
        private readonly string _sequence;
        private readonly int _zeroBasedStartOffset;
        public int Length => _zeroBasedStartOffset + _sequence.Length;
        public Band[] CytogeneticBands => null;

        public SimpleSequence(string s, int zeroBasedStartOffset = 0)
        {
            _zeroBasedStartOffset = zeroBasedStartOffset;
            _sequence             = s;
        }

        public string Substring(int offset, int length)
        {
            if (offset - _zeroBasedStartOffset + length > _sequence.Length || 
                offset < _zeroBasedStartOffset) return "";
            return _sequence.Substring(offset - _zeroBasedStartOffset, length);
        }
    }

    public sealed class SimpleSequenceProvider : ISequenceProvider
    {
        public string Name { get; }
        public GenomeAssembly Assembly { get; }
        public IEnumerable<IDataSourceVersion> DataSourceVersions { get; }
        public void Annotate(IAnnotatedPosition annotatedPosition)
        {
            throw new System.NotImplementedException();
        }

        public void PreLoad(Chromosome chromosome, List<int> positions)
        {
            throw new System.NotImplementedException();
        }

        public ISequence Sequence { get; }
        public IDictionary<string, Chromosome> RefNameToChromosome { get; }
        public IDictionary<ushort, Chromosome> RefIndexToChromosome { get; }
        public void LoadChromosome(Chromosome chromosome) { }

        public SimpleSequenceProvider(GenomeAssembly assembly, ISequence sequence,
            IDictionary<string, Chromosome> refNameToChromosome)
        {
            Assembly            = assembly;
            Sequence            = sequence;
            RefNameToChromosome = refNameToChromosome;
        }

        public void Dispose() { }
    }
}