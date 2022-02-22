namespace VariantAnnotation.Interface.SA
{
    public interface IProteinSuppDataItem
    {
        string TranscriptId { get; }
        string ProteinId { get; }
        int    Position     { get; }
        char   RefAllele    { get; }
        char   AltAllele    { get; }
        short  Score        { get; }
    }
    
    
}