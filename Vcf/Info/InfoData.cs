using VariantAnnotation.Interface.Positions;

namespace Vcf.Info
{
    public sealed class InfoData : IInfoData
    {
        public int[] CiEnd { get; }
        public int[] CiPos { get; }
        public int? End { get; }
        public double? RecalibratedQuality { get; }
        public int? JointSomaticNormalQuality { get; }
        public int? RefRepeatCount { get; }
        public string RepeatUnit { get; }
        public double? StrandBias { get; }
        public int? SvLength { get; }
        public string SvType { get; }

        public InfoData(int[] ciEnd, int[] ciPos, int? end, double? recalibratedQuality, int? jointSomaticNormalQuality,
            int? refRepeatCount, string repeatUnit, double? strandBias, int? svLength, string svType)
        {
            CiEnd                     = ciEnd;
            CiPos                     = ciPos;
            End                       = end;
            RecalibratedQuality       = recalibratedQuality;
            JointSomaticNormalQuality = jointSomaticNormalQuality;
            RefRepeatCount            = refRepeatCount;
            RepeatUnit                = repeatUnit;
            StrandBias                = strandBias;
            SvLength                  = svLength;
            SvType                    = svType;
        }
    }
}