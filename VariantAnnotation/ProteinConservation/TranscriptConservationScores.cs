using System;
using IO;
using OptimizedCore;

namespace VariantAnnotation.ProteinConservation
{
    public sealed class TranscriptConservationScores
    {
        public readonly string TranscriptId;
        public readonly byte[] ConservationScores;

        public TranscriptConservationScores(string id, byte[] scores)
        {
            //removing versions for ensembl only
            TranscriptId = id;
            ConservationScores = scores;
        }

        public static TranscriptConservationScores Read(ExtendedBinaryReader reader)
        {
            var id = reader.ReadAsciiString();
            var count = reader.ReadOptInt32();
            var scores = reader.ReadBytes(count);
            var item = new TranscriptConservationScores(id, scores);
            return item.IsEmpty() ? null : item;
        }

        public void Write(ExtendedBinaryWriter writer)
        {
            writer.WriteOptAscii(TranscriptId);
            writer.WriteOpt(ConservationScores.Length);
            writer.Write(ConservationScores);
        }

        public static TranscriptConservationScores GetEmptyItem() => new TranscriptConservationScores("", Array.Empty<byte>());
        
        public bool IsEmpty() => string.IsNullOrEmpty(TranscriptId) && ConservationScores.Length == 0;
        
    }
}