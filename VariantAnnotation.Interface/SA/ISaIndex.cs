namespace VariantAnnotation.Interface.SA
{
    public interface ISaIndex
    {
        int[] RefMinorPositions { get; }
        long GetOffset(int position);
        bool IsRefMinor(int position);
    }
}