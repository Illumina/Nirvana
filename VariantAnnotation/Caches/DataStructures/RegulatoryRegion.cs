using System.Collections.Generic;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Caches;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class RegulatoryRegion : IRegulatoryRegion
    {
        public int Start { get; }
        public int End { get; }
        public IChromosome Chromosome { get; }
        public ICompactId Id { get; }
        public RegulatoryElementType Type { get; }

        /// <summary>
        /// constructor
        /// </summary>
        internal RegulatoryRegion(IChromosome chromosome, int start, int end, CompactId id, RegulatoryElementType type)
        {
            Id         = id;
            Type       = type;
            Start      = start;
            End        = end;
            Chromosome = chromosome;
        }

        /// <summary>
        /// reads the regulatory element data from the binary reader
        /// </summary>
        public static IRegulatoryRegion Read(IExtendedBinaryReader reader, IDictionary<ushort, IChromosome> chromosomeIndexDictionary)
        {
            var refIndex = reader.ReadUInt16();
            int start    = reader.ReadOptInt32();
            int end      = reader.ReadOptInt32();
            var type     = (RegulatoryElementType)reader.ReadByte();
            var id       = CompactId.Read(reader);

            return new RegulatoryRegion(chromosomeIndexDictionary[refIndex], start, end, id, type);
        }


        /// <summary>
        /// writes the regulatory element data to the binary writer
        /// </summary>
        public void Write(IExtendedBinaryWriter writer)
        {
            writer.Write(Chromosome.Index);
            writer.WriteOpt(Start);
            writer.WriteOpt(End);
            writer.Write((byte)Type);
            // ReSharper disable once ImpureMethodCallOnReadonlyValueField
            Id.Write(writer);
        }
    }
}
