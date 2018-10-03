using Genome;
using IO;
using VariantAnnotation.Interface.SA;
using Variants;

namespace VariantAnnotation.NSA
{
    public sealed class SuppInterval:ISuppIntervalItem
    {
        public int Start { get; }
        public int End { get; }
        public IChromosome Chromosome { get; }
        public VariantType VariantType { get; }
        private readonly string _jsonString;

        public SuppInterval(ExtendedBinaryReader reader)
        {
            string ensemblName = reader.ReadAsciiString();
            string ucscName = reader.ReadAsciiString();
            ushort chromIndex = reader.ReadOptUInt16();
            Chromosome = new Chromosome(ucscName, ensemblName, chromIndex);

            Start       = reader.ReadOptInt32();
            End         = reader.ReadOptInt32();
            _jsonString = reader.ReadString();
        }

        public string GetJsonString() => _jsonString;
    }
}