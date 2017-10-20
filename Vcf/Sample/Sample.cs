using VariantAnnotation.Interface.Positions;

namespace Vcf.Sample
{
    public sealed class Sample : ISample
    {
        public double? VariantFrequency { get; }
        public int? TotalDepth { get; }
        public int[] AlleleDepths { get; }
        public string Genotype { get; }
        public int? GenotypeQuality { get; }
        public bool FailedFilter { get; }
        public int? CopyNumber { get; }
        public bool IsLossOfHeterozygosity { get; }
        public float? DeNovoQuality { get; }
        public bool IsEmpty { get; }
        public int[] SplitReadCounts { get; }
        public int[] PairEndReadCounts { get; }
	    public string RepeatNumbers { get; }
	    public string RepeatNumberSpans { get; }

	    public Sample(string genotype, int? genotypeQuality, double? variantFreq, int? totalDepth, int[] alleleDepths,
            bool failedFilter, int? copyNumber, bool isLossOfHeterozygosity, float? deNovoQuality, int[] splitReadCounts,
            int[] pairEndReadCounts, string repeatNumbers, string repeatNumberSpan)
        {
            Genotype = genotype;
            GenotypeQuality = genotypeQuality;
            VariantFrequency = variantFreq;
            TotalDepth = totalDepth;
            AlleleDepths = alleleDepths;
            FailedFilter = failedFilter;
            CopyNumber = copyNumber;
            IsLossOfHeterozygosity = isLossOfHeterozygosity;
            DeNovoQuality = deNovoQuality;
            SplitReadCounts = splitReadCounts;
            PairEndReadCounts = pairEndReadCounts;
	        RepeatNumbers = repeatNumbers;
	        RepeatNumberSpans = repeatNumberSpan;
            IsEmpty = IsNull();
        }

        public Sample()
        {
            IsEmpty = true;
        }

        private bool IsNull()
        {
            return string.IsNullOrEmpty(Genotype) && GenotypeQuality == null && VariantFrequency == null &&
                   TotalDepth == null && (AlleleDepths == null || AlleleDepths.Length == 0) && FailedFilter &&
                   CopyNumber == null && IsLossOfHeterozygosity && DeNovoQuality == null &&
                   (SplitReadCounts == null || SplitReadCounts.Length == 0) &&
                   (PairEndReadCounts == null || PairEndReadCounts.Length == 0);
        }


    }
}