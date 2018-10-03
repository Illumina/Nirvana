namespace VariantAnnotation.Interface.SA
{
    public interface ISuppGeneItem
    {
        string GeneSymbol { get; }
        string GetJsonString();
    }
}