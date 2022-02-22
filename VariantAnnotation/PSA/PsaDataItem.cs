namespace VariantAnnotation.PSA;

public sealed class PsaDataItem
{
    public string ChromName    { get; }
    public string TranscriptId { get; }
    public int    Position     { get; }
    public char   RefAllele    { get; }
    public char   AltAllele    { get; }
    public ushort Score        { get; }
    public string Prediction   { get; }

    public PsaDataItem(string chromName, string transcriptId, int position,
        char refAllele, char altAllele, ushort score, string prediction)
    {
        ChromName    = chromName;
        Position     = position;
        TranscriptId = transcriptId;
        RefAllele    = refAllele;
        AltAllele    = altAllele;
        Score        = score;
        Prediction   = prediction;
    }
}