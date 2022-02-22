using IO;

namespace Genome
{
    public sealed record Chromosome(string UcscName, string EnsemblName, string RefSeqAccession,
        string GenBankAccession, int Length, ushort Index)
    {
        public const ushort UnknownReferenceIndex = ushort.MaxValue;
        public       bool   IsEmpty => Index == UnknownReferenceIndex;

        public static Chromosome GetEmpty(string referenceName) => new(referenceName, referenceName, referenceName,
            referenceName, 1, UnknownReferenceIndex);

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
    }
}