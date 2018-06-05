using Variants;

namespace VariantAnnotation.Interface.Positions
{
    public interface IPosition : ISimplePosition
    {
        double? Quality { get; }
        string[] Filters { get; }
        IVariant[] Variants { get; }
        ISample[] Samples { get; }
        IInfoData InfoData { get; }
    }
}