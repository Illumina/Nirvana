using System;
using IO;

namespace Genome
{
    public sealed class Chromosome : IChromosome, IComparable<IChromosome>
    {
        public string UcscName { get; }
        public string EnsemblName { get; }
        public string RefSeqAccession { get; }
        public string GenBankAccession { get; }
        public int Length { get; }
        public ushort Index { get; }

        public const ushort UnknownReferenceIndex = ushort.MaxValue;

        public Chromosome(string ucscName, string ensemblName, string refSeqAccession, string genBankAccession,
            int length, ushort index)
        {
            UcscName         = ucscName;
            EnsemblName      = ensemblName;
            RefSeqAccession  = refSeqAccession;
            GenBankAccession = genBankAccession;
            Length           = length;
            Index            = index;
        }
        
        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOptAscii(UcscName);
            writer.WriteOptAscii(EnsemblName);
            writer.WriteOptAscii(RefSeqAccession);
            writer.WriteOptAscii(GenBankAccession);
            writer.WriteOpt(Length);
            writer.WriteOpt(Index);
        }

        public static IChromosome Read(ExtendedBinaryReader reader)
        {
            string ucscName         = reader.ReadAsciiString();
            string ensemblName      = reader.ReadAsciiString();
            string refseqAccession  = reader.ReadAsciiString();
            string genBankAccession = reader.ReadAsciiString();
            int    length           = reader.ReadOptInt32();
            ushort refIndex         = reader.ReadOptUInt16();

            return new Chromosome(ucscName, ensemblName, refseqAccession, genBankAccession, length, refIndex);
        }

        public bool Equals(IChromosome other) => Index == other.Index && Length == other.Length;

        public int CompareTo(IChromosome other) => Index == other.Index ? Length.CompareTo(other.Length) : Index.CompareTo(other.Index);
    }
}