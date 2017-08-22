namespace VariantAnnotation.Interface.Positions
{
    public interface IInfoData
    {
        int? End { get; }
        double? StrandBias { get; }
        double? RecalibratedQuality { get; }
        int? JointSomaticNormalQuality { get; }
        int? CopyNumber { get;}
	    int? Depth { get; }
        bool ColocalizedWithCnv { get; }
		bool IsInv3 { get; }
	    bool IsInv5 { get; }

		int[] CiPos { get; }
        int[] CiEnd { get; }

        int? SvLength { get; }
		VariantType SvType { get; }

		int? RefRepeatCount { get; }
		string RepeatUnit { get; }

	    string UpdatedInfoField { get; }
    }
}