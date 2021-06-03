namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IGeneFusionPair
    {
        ulong  FusionKey        { get; }
        string FirstGeneSymbol  { get; }
        uint   FirstGeneKey     { get; }
        string SecondGeneSymbol { get; }
        uint   SecondGeneKey    { get; }
    }
}