using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures
{
    public sealed class Translation
    {
        public readonly CdnaCoordinateMap CodingRegion;
        public readonly CompactId ProteinId;
        public readonly byte ProteinVersion;
        public readonly string PeptideSeq;

        /// <summary>
        /// constructor
        /// </summary>
        public Translation(CdnaCoordinateMap codingRegion, CompactId proteinId, byte proteinVersion, string peptideSeq)
        {
            CodingRegion   = codingRegion;
            ProteinId      = proteinId;
            ProteinVersion = proteinVersion;
            PeptideSeq     = peptideSeq;
        }

        /// <summary>
        /// reads the translation from the binary writer
        /// </summary>
        public static Translation Read(ExtendedBinaryReader reader, string[] peptideSeqs)
        {
            var codingRegion   = CdnaCoordinateMap.Read(reader);
            var proteinId      = CompactId.Read(reader);
            var proteinVersion = reader.ReadByte();
            var peptideIndex   = reader.ReadOptInt32();

            return new Translation(codingRegion, proteinId, proteinVersion, peptideSeqs[peptideIndex]);
        }

        /// <summary>
        /// writes the translation to the binary writer
        /// </summary>
        public void Write(ExtendedBinaryWriter writer, int peptideIndex)
        {
            CodingRegion.Write(writer);
            ProteinId.Write(writer);
            writer.Write(ProteinVersion);
            writer.WriteOpt(peptideIndex);
        }
    }
}
