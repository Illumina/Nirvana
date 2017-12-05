namespace VariantAnnotation.Interface.SA
{
    public interface ISaIndex
    {
        (int Position, string GlobalMajorAllele)[] GlobalMajorAlleleForRefMinor { get; }
        long GetOffset(int position);
    }
}