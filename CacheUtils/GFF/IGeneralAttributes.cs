namespace CacheUtils.GFF
{
    public interface IGeneralAttributes
    {
        string GeneId { get; }
        string GeneSymbol { get; }
        string TranscriptId { get; }
        string ProteinId { get; }
        string BioType { get; }
        bool IsCanonical { get; }
        int InternalGeneId { get; }
    }
}
