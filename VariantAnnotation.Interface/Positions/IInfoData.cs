namespace VariantAnnotation.Interface.Positions
{
    public interface IInfoData
    {
        int[]          CiEnd                     { get; }
        int[]          CiPos                     { get; }
        int?           End                       { get; }
        double?        RecalibratedQuality       { get; }
        int?           JointSomaticNormalQuality { get; }
        int?           RefRepeatCount            { get; }
        string         RepeatUnit                { get; }
        double?        StrandBias                { get; }
        int?           SvLength                  { get; }
        string         SvType                    { get; }
        double? FisherStrandBias          { get; }
        double? MappingQuality            { get; }
    }
}