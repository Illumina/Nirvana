namespace VariantAnnotation.Interface
{
    public interface ISaIndex
    {
        long GetOffset(int position);
        bool IsRefMinor(int position);
    }
}
