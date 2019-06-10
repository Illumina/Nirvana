using Variants;

namespace VariantAnnotation.Interface.Positions
{
    public interface IInfoData
    {
        int[] CiEnd { get; }
        int[] CiPos { get; }
        int? End { get; }
        int? JointSomaticNormalQuality { get; }
        int? RefRepeatCount { get; }
        string RepeatUnit { get; }
        double? StrandBias { get; }
        int? SvLength { get; }
        VariantType SvType { get; }
    }
}