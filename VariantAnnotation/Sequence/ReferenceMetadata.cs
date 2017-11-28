using VariantAnnotation.IO;

namespace VariantAnnotation.Sequence
{
    public sealed class ReferenceMetadata
    {
        public readonly string UcscName;
        public readonly string EnsemblName;

        public ReferenceMetadata(string ucscName, string ensemblName)
        {
            UcscName    = ucscName;
            EnsemblName = ensemblName;
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOptAscii(UcscName);
            writer.WriteOptAscii(EnsemblName);
            writer.Write(true); // TODO: Remove this when we update the reference file format
        }

        public static ReferenceMetadata Read(ExtendedBinaryReader reader)
        {
            var ucscName    = reader.ReadAsciiString();
            var ensemblName = reader.ReadAsciiString();
            reader.ReadBoolean(); // TODO: Remove this when we update the reference file format

            return new ReferenceMetadata(ucscName, ensemblName);
        }
    }
}