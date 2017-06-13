using VariantAnnotation.DataStructures.ProteinFunction;
using VariantAnnotation.DataStructures.Transcript;

namespace CacheUtils.UpdateMiniCacheFiles.DataStructures
{
    public class TranscriptPacket
    {
        public readonly ushort ReferenceIndex;
        public readonly string Id;

        public Transcript Transcript;

        public Prediction SiftPrediction;
        public Prediction PolyPhenPrediction;

        public int NewSiftIndex     = -1;
        public int NewPolyPhenIndex = -1;

        /// <summary>
        /// constructor
        /// </summary>
        public TranscriptPacket(Transcript transcript)
        {
            ReferenceIndex       = transcript.ReferenceIndex;
            Id                   = transcript.Id.ToString();
            Transcript           = transcript;
        }
    }
}
