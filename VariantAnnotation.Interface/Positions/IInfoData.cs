using System.Collections.Generic;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.Positions
{
    public interface IInfoData
    {
        int[]                 CiEnd                     { get; }
        int[]                 CiPos                     { get; }
        int?                  End                       { get; }
        double?               RecalibratedQuality       { get; }
        int?                  JointSomaticNormalQuality { get; }
        int?                  RefRepeatCount            { get; }
        string                RepeatUnit                { get; }
        double?               StrandBias                { get; }
        int?                  SvLength                  { get; }
        string                SvType                    { get; }
        double?               FisherStrandBias          { get; }
        double?               MappingQuality            { get; }
        string                BreakendEventId           { get; }
        bool                  IsImprecise               { get; }
        ICustomFields CustomKeyValues{ get; }
        // for old version of Manta, but still required by Encore
        bool    IsInv3 { get; }
        bool    IsInv5 { get; }
        double? LogOddsRatio    { get; }
    }
}