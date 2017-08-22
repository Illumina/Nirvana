using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class Translation : ITranslation
    {
        public ICdnaCoordinateMap CodingRegion { get; }
        public ICompactId ProteinId { get; }
        public byte ProteinVersion { get; }
        public string PeptideSeq { get; }

        internal Translation(ICdnaCoordinateMap codingRegion, CompactId proteinId, byte proteinVersion, string peptideSeq)
        {
            CodingRegion   = codingRegion;
            ProteinId      = proteinId;
            ProteinVersion = proteinVersion;
            PeptideSeq     = peptideSeq;
        }

        public static ITranslation Read(ExtendedBinaryReader reader, string[] peptideSeqs)
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
        public void Write(IExtendedBinaryWriter writer, int peptideIndex)
        {
            // ReSharper disable ImpureMethodCallOnReadonlyValueField
            CodingRegion.Write(writer);
            ProteinId.Write(writer);
            // ReSharper restore ImpureMethodCallOnReadonlyValueField
            writer.Write(ProteinVersion);
            writer.WriteOpt(peptideIndex);
        }
    }
}