namespace VariantAnnotation.Interface
{
    public interface IGeneAnnotation
    {
        string GeneName { get; }
        string DataSource { get; }
        bool IsArray { get;}
    }
}