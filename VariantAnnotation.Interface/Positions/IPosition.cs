using VariantAnnotation.Interface.Intervals;

namespace VariantAnnotation.Interface.Positions
{
    public interface IPosition : IChromosomeInterval
    {
        string RefAllele { get; }
        string[] AltAlleles { get; }
        double? Quality { get; }
        string[] Filters { get; }
        IVariant[] Variants { get; }
        ISample[] Samples { get; }
        IInfoData InfoData { get; }
        string[] VcfFields { get; }
    }
}