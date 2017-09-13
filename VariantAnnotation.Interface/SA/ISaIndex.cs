using System;

namespace VariantAnnotation.Interface.SA
{
    public interface ISaIndex
    {
        Tuple<int, string>[] GlobalMajorAlleleForRefMinor { get; }
        long GetOffset(int position);
    }
}