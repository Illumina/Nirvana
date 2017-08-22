namespace VariantAnnotation.Interface.GeneAnnotation
{
    public interface IGeneAnnotation
    {
        string DataSource { get; }
        string[] JsonStrings { get; }
        bool IsArray { get; }

    }
}