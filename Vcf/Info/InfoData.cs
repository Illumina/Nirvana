using VariantAnnotation.Interface.Positions;

namespace Vcf.Info
{
    public sealed class InfoData : IInfoData
    {
        public int? End { get; }
        public double? StrandBias { get; }
        public double? RecalibratedQuality { get; }
        public int? JointSomaticNormalQuality { get; }
        public int? CopyNumber { get; }
        public int? Depth { get; }
        public bool ColocalizedWithCnv { get; }
		public bool IsInv3 { get; }
	    public bool IsInv5 { get; }
		public int[] CiPos { get; }
        public int[] CiEnd { get; }
        public int? SvLength { get; }
	    public VariantType SvType { get; }

		public int? RefRepeatCount { get; }
		public string RepeatUnit { get; }
        public string UpdatedInfoField { get; }

        public InfoData(int? end, int? svLength, VariantType svType, double? strandBias, double? recalibratedQuality,
            int? jointSomaticNormalQuality, int? copyNumber, int? depth, bool colocalizedWithCnv, int[] ciPos,
            int[] ciEnd, bool isInv3, bool isInv5, string updatedInfoField, string repeatUnit, int? refRepeatCount)
        {
            End                       = end;
            SvLength                  = svLength;
	        SvType                    = svType;
            StrandBias                = strandBias;
            RecalibratedQuality       = recalibratedQuality;
            JointSomaticNormalQuality = jointSomaticNormalQuality;
            CopyNumber                = copyNumber;
            Depth                     = depth;
            ColocalizedWithCnv        = colocalizedWithCnv;
	        IsInv3                    = isInv3;
	        IsInv5                    = isInv5;
            CiPos                     = ciPos;
            CiEnd                     = ciEnd;
	        RepeatUnit                = repeatUnit;
	        RefRepeatCount            = refRepeatCount;
            UpdatedInfoField          = updatedInfoField;
        }
    }
}