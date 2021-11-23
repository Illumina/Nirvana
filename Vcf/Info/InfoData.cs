using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;

namespace Vcf.Info
{
    public sealed record InfoData(string BreakendEventId, int[] CiEnd, int[] CiPos, int? End, double? FisherStrandBias, bool IsImprecise, bool IsInv3,
        bool IsInv5, int? JointSomaticNormalQuality, double? MappingQuality, double? RecalibratedQuality, int? RefRepeatCount, string RepeatUnit,
        double? StrandBias, int? SvLength, string SvType, ICustomInfoData CustomKeyValues) : IInfoData;

    public sealed class InfoDataBuilder
    {
        public string         BreakendEventId;
        public int[]          CiEnd;
        public int[]          CiPos;
        public int?           End;
        public double?        FisherStrandBias;
        public bool           IsImprecise;
        public bool           IsInv3;
        public bool           IsInv5;
        public int?           JointSomaticNormalQuality;
        public double?        MappingQuality;
        public double?        RecalibratedQuality;
        public int?           RefRepeatCount;
        public string         RepeatUnit;
        public double?        StrandBias;
        public int?           SvLength;
        public string         SvType;
        public ICustomInfoData CustomInfoData=new CustomInfoData();

        public InfoData Create() =>
            new(BreakendEventId, CiEnd, CiPos, End, FisherStrandBias, IsImprecise, IsInv3, IsInv5, JointSomaticNormalQuality, MappingQuality,
                RecalibratedQuality, RefRepeatCount, RepeatUnit, StrandBias, SvLength, SvType, CustomInfoData);

        public void Reset()
        {
            BreakendEventId           = null;
            CiEnd                     = null;
            CiPos                     = null;
            End                       = null;
            FisherStrandBias          = null;
            IsImprecise               = false;
            IsInv3                    = false;
            IsInv5                    = false;
            JointSomaticNormalQuality = null;
            MappingQuality            = null;
            RecalibratedQuality       = null;
            RefRepeatCount            = null;
            RepeatUnit                = null;
            StrandBias                = null;
            SvLength                  = null;
            SvType                    = null;
            CustomInfoData.Clear();
        }
    }
}