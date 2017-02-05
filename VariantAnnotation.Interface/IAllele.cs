namespace VariantAnnotation.Interface
{
    public interface IAllele
    {
        int Start { get; }
        int End { get; }
        VariantType NirvanaVariantType { get; }
    }
}
