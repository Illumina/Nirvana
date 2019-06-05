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

        public SimpleSequence(string s, int zeroBasedStartOffset = 0)
        {
            _zeroBasedStartOffset = zeroBasedStartOffset;
            _sequence = s;
        }

        public string Substring(int offset, int length)
        {
            if (offset - _zeroBasedStartOffset + length > _sequence.Length
                || offset < _zeroBasedStartOffset)
                return "";
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

        public void PreLoad(IChromosome chromosome, List<int> positions)
        {
            throw new System.NotImplementedException();
        }

        public ISequence Sequence { get; }
        public IDictionary<string, IChromosome> RefNameToChromosome { get; }
        public IDictionary<ushort, IChromosome> RefIndexToChromosome { get; }
        public void LoadChromosome(IChromosome chromosome)
        {
            
        }

        public SimpleSequenceProvider(GenomeAssembly assembly, ISequence sequence,
            IDictionary<string, IChromosome> refNameToChromosome)
        {
            Assembly = assembly;
            Sequence = sequence;
            RefNameToChromosome = refNameToChromosome;
        }

        public void Dispose()
        {
            
        }
    }
}