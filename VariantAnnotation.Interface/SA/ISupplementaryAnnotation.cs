namespace VariantAnnotation.Interface.SA
{
    public interface ISupplementaryAnnotation
    {
        string JsonKey { get; }
        string GetJsonString();

    }
}