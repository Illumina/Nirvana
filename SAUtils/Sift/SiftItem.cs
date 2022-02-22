using VariantAnnotation.Interface.SA;

namespace SAUtils.Sift
{
    public sealed class SiftItem : IProteinSuppDataItem
    {
        public string TranscriptId { get; }
        public string ProteinId    { get; }
        public int    Position     { get; }
        public char   RefAllele    { get; }
        public char   AltAllele    { get; }
        public short  Score        { get; }

        public SiftItem(string transcriptId, int position,
            char refAllele, char altAllele, short score)
        {
            Position     = position;
            TranscriptId = transcriptId;
            RefAllele    = refAllele;
            AltAllele    = altAllele;
            Score        = score;
        }
    }
}