using IO;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Caches.DataStructures
{
    public sealed class Translation : ITranslation
    {
        public ICodingRegion CodingRegion { get; }
        public ICompactId ProteinId { get; }
        public string PeptideSeq { get; }

        public Translation(ICodingRegion codingRegion, CompactId proteinId, string peptideSeq)
        {
            CodingRegion = codingRegion;
            ProteinId    = proteinId;
            PeptideSeq   = peptideSeq;
        }

        public static ITranslation Read(BufferedBinaryReader reader, string[] peptideSeqs)
        {
            var codingRegion = DataStructures.CodingRegion.Read(reader);
            var proteinId    = CompactId.Read(reader);
            var peptideIndex = reader.ReadOptInt32();
            var peptideSeq   = peptideIndex == -1 ? null : peptideSeqs[peptideIndex];

            return new Translation(codingRegion, proteinId, peptideSeq);
        }

        public void Write(IExtendedBinaryWriter writer, int peptideIndex)
        {
            CodingRegion.Write(writer);
            ProteinId.Write(writer);
            writer.WriteOpt(peptideIndex);
        }
    }
}