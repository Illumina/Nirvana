using VariantAnnotation.IO;

namespace VariantAnnotation.Sequence
{
    public sealed class ReferenceMetadata
    {
        public readonly string UcscName;
        public readonly string EnsemblName;
        public readonly bool InVep;

        public ReferenceMetadata(string ucscName, string ensemblName, bool inVep)
        {
            UcscName    = ucscName;
            EnsemblName = ensemblName;
            InVep       = inVep;
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOptAscii(UcscName);
            writer.WriteOptAscii(EnsemblName);
            writer.Write(InVep);
        }

        public static ReferenceMetadata Read(ExtendedBinaryReader reader)
        {
            var ucscName    = reader.ReadAsciiString();
            var ensemblName = reader.ReadAsciiString();
            var inVep       = reader.ReadBoolean();

            return new ReferenceMetadata(ucscName, ensemblName, inVep);
        }
    }
}