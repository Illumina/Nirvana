namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IGeneFusionPair
    {
        ulong    GeneKey     { get; }
        string[] GeneSymbols { get; }
    }
}