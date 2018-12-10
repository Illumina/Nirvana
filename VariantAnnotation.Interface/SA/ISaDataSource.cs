namespace VariantAnnotation.Interface.SA
{
    public interface ISaDataSource
    {
        string KeyName { get; }
        string VcfkeyName { get; }
        bool MatchByAllele { get; }
        bool IsArray { get; }
    }
}