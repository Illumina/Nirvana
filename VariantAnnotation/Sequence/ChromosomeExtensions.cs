using Genome;
using IO;

namespace VariantAnnotation.Sequence
{
    public static class ChromosomeExtensions
    {
        public static void Write(this IChromosome chromosome, ExtendedBinaryWriter writer)
        {
            writer.WriteOptAscii(chromosome.UcscName);
            writer.WriteOptAscii(chromosome.EnsemblName);
            writer.WriteOptAscii(chromosome.RefSeqAccession);
            writer.WriteOptAscii(chromosome.GenBankAccession);
            writer.WriteOpt(chromosome.Length);
            writer.WriteOpt(chromosome.Index);
        }

        public static IChromosome Read(ExtendedBinaryReader reader)
        {
            string ucscName         = reader.ReadAsciiString();
            string ensemblName      = reader.ReadAsciiString();
            string refseqAccession  = reader.ReadAsciiString();
            string genBankAccession = reader.ReadAsciiString();
            int length              = reader.ReadOptInt32();
            ushort refIndex         = reader.ReadOptUInt16();

            return new Chromosome(ucscName, ensemblName, refseqAccession, genBankAccession, length, refIndex);
        }
    }
}