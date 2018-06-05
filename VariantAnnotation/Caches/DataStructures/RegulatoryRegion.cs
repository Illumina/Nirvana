using System.Collections.Generic;
using Genome;
using IO;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class RegulatoryRegion : IRegulatoryRegion
    {
        public int Start { get; }
        public int End { get; }
        public IChromosome Chromosome { get; }
        public ICompactId Id { get; }
        public RegulatoryRegionType Type { get; }

        public RegulatoryRegion(IChromosome chromosome, int start, int end, CompactId id, RegulatoryRegionType type)
        {
            Id         = id;
            Type       = type;
            Start      = start;
            End        = end;
            Chromosome = chromosome;
        }

        public static IRegulatoryRegion Read(IBufferedBinaryReader reader, IDictionary<ushort, IChromosome> chromosomeIndexDictionary)
        {
            var refIndex = reader.ReadOptUInt16();
            int start    = reader.ReadOptInt32();
            int end      = reader.ReadOptInt32();
            var type     = (RegulatoryRegionType)reader.ReadByte();
            var id       = CompactId.Read(reader);

            return new RegulatoryRegion(chromosomeIndexDictionary[refIndex], start, end, id, type);
        }

        public void Write(IExtendedBinaryWriter writer)
        {
            writer.WriteOpt(Chromosome.Index);
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            writer.Write((byte)Type);
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            Id.Write(writer);
        }
    }
}
