namespace CacheUtils.GFF
{
    public sealed class GeneralAttributes : IGeneralAttributes
    {
        public string GeneId { get; }
        public string GeneSymbol { get; }
        public string TranscriptId { get; }
        public string ProteinId { get; }
        public string BioType { get; }
        public bool IsCanonical { get; }
        public int InternalGeneId { get; }

        public GeneralAttributes(string geneId, string geneSymbol, string transcriptId, string proteinId,
            string bioType, bool isCanonical, int internalGeneId)
        {
            GeneId         = geneId;
            GeneSymbol     = geneSymbol;
            TranscriptId   = transcriptId;
            ProteinId      = proteinId;
            BioType        = bioType;
            IsCanonical    = isCanonical;
            InternalGeneId = internalGeneId;
        }
    }
}
