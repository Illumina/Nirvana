using System;
using IO;

namespace Genome
{
    
    public sealed class Chromosome : IComparable<Chromosome>
    {
        public string UcscName         { get; }
        public string EnsemblName      { get; }
        public string RefSeqAccession  { get; }
        public string GenBankAccession { get; }
        public int    FlankingLength   { get; private set; }
        public int    Length           { get; }
        public ushort Index            { get; }

        public const ushort UnknownReferenceIndex = ushort.MaxValue;
        public const int ShortFlankingLength = 100;

        public static Chromosome GetEmptyChromosome(string name)
        {
             return  new Chromosome(name, name, name, name, 0, ushort.MaxValue)
            {
                FlankingLength = ShortFlankingLength
            };
        }

        
        public Chromosome(string ucscName, string ensemblName, string refSeqAccession, string genBankAccession,
            int length, ushort index)
        {
            UcscName         = ucscName;
            EnsemblName      = ensemblName;
            RefSeqAccession  = refSeqAccession;
            GenBankAccession = genBankAccession;
            Length           = length;
            Index            = index;

            // for short references (< 30 kbp), let's use a shorter flanking length
            const int longFlankingLength      = 5_000;
            const int shortReferenceThreshold = 30_000_000;

            FlankingLength = length < shortReferenceThreshold ? ShortFlankingLength : longFlankingLength;
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

        public static Chromosome Read(ExtendedBinaryReader reader)
        {
            string ucscName         = reader.ReadAsciiString();
            string ensemblName      = reader.ReadAsciiString();
            string refseqAccession  = reader.ReadAsciiString();
            string genBankAccession = reader.ReadAsciiString();
            int    length           = reader.ReadOptInt32();
            ushort refIndex         = reader.ReadOptUInt16();

            return new Chromosome(ucscName, ensemblName, refseqAccession, genBankAccession, length, refIndex);
        }

        public bool Equals(Chromosome other) => Index == other.Index && Length == other.Length;

        public int CompareTo(Chromosome other) => Index == other.Index ? Length.CompareTo(other.Length) : Index.CompareTo(other.Index);

        public override int GetHashCode()
        {
            return UcscName.GetHashCode() ^ Length ^ Index;
        }
    }
}