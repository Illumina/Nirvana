using System;
using System.Collections.Generic;
using CommonUtilities;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class OneKGenItem : SupplementaryDataItem
    {
        #region members

        private string Id { get; }
        private string AncestralAllele { get; }
        
        private string AfrFreq { get; }
        private string AllFreq { get; }
        private string AmrFreq { get; }
        private string EasFreq { get; }
        private string EurFreq { get; }
        private string SasFreq { get; }

        public int? AllAlleleNumber { get; }
        private int? AfrAlleleNumber { get; }
        private int? AmrAlleleNumber { get; }
        private int? EurAlleleNumber { get; }
        private int? EasAlleleNumber { get; }
        private int? SasAlleleNumber { get; }

        public int? AllAlleleCount { get; }
        private int? AfrAlleleCount { get; }
        private int? AmrAlleleCount { get; }
        private int? EurAlleleCount { get; }
        private int? EasAlleleCount { get; }
        private int? SasAlleleCount { get; }

        private string SvType { get; }
        private int SvEnd { get; }
        private int ObservedGains { get; }
        private int ObservedLosses { get; }

        #endregion

        public OneKGenItem(IChromosome chromosome,
            int position,
            string id,
            string refAllele,
            string alternateAllele,
            string ancestralAllele,
            int? allAlleleCount,
            int? afrAlleleCount,
            int? amrAlleleCount,
            int? eurAlleleCount,
            int? easAlleleCount,
            int? sasAlleleCount,
            int? allAlleleNumber,
            int? afrAlleleNumber,
            int? amrAlleleNumber,
            int? eurAlleleNumber,
            int? easAlleleNumber,
            int? sasAlleleNumber,
            string svType,
            int svEnd
            )
        {
            Chromosome = chromosome;
            Start = position;
            Id = id;
            ReferenceAllele = refAllele;
            AlternateAllele = alternateAllele;
            AncestralAllele = ancestralAllele;


            AllAlleleCount = allAlleleCount;
            AfrAlleleCount = afrAlleleCount;
            AmrAlleleCount = amrAlleleCount;
            EurAlleleCount = eurAlleleCount;
            EasAlleleCount = easAlleleCount;
            SasAlleleCount = sasAlleleCount;

            AllAlleleNumber = allAlleleNumber;
            AfrAlleleNumber = afrAlleleNumber;
            AmrAlleleNumber = amrAlleleNumber;
            EurAlleleNumber = eurAlleleNumber;
            EasAlleleNumber = easAlleleNumber;
            SasAlleleNumber = sasAlleleNumber;

            SvType = svType;
            SvEnd = svEnd;

            IsInterval = svType != null;
        }

        public OneKGenItem(IChromosome chromosome,
            int position,
            string id,
            string refAllele,
            string alternateAllele,
            string ancestralAllele,
            string afrFreq,
            string allFreq,
            string amrFreq,
            string easFreq,
            string eurFreq,
            string sasFreq,
            int? allAlleleCount,
            int? afrAlleleCount,
            int? amrAlleleCount,
            int? eurAlleleCount,
            int? easAlleleCount,
            int? sasAlleleCount,
            int? allAlleleNumber,
            int? afrAlleleNumber,
            int? amrAlleleNumber,
            int? eurAlleleNumber,
            int? easAlleleNumber,
            int? sasAlleleNumber,
            string svType,
            int svEnd, int observedGains, int observedLosses)
            : this(
                chromosome, position, id, refAllele, alternateAllele, ancestralAllele, allAlleleCount, afrAlleleCount, amrAlleleCount, eurAlleleCount, easAlleleCount, sasAlleleCount,
                allAlleleNumber, afrAlleleNumber, amrAlleleNumber, eurAlleleNumber, easAlleleNumber, sasAlleleNumber, svType,
                svEnd)
        {
            ObservedGains = observedGains;
            ObservedLosses = observedLosses;

            AfrFreq = afrFreq;
            AllFreq = allFreq;
            AmrFreq = amrFreq;
            EasFreq = easFreq;
            EurFreq = eurFreq;
            SasFreq = sasFreq;
        }

        public override SupplementaryIntervalItem GetSupplementaryInterval()
        {
            if (!IsInterval) return null;

            var seqAltType = SaParseUtilities.GetSequenceAlteration(SvType, ObservedGains, ObservedLosses);

            var intValues    = new Dictionary<string, int>();
            var doubleValues = new Dictionary<string, double>();
            var freqValues   = new Dictionary<string, double>();
            var stringValues = new Dictionary<string, string>();
            var boolValues   = new List<string>();

            var suppInterval = new SupplementaryIntervalItem(Chromosome,Start, SvEnd, AlternateAllele, seqAltType,
                "1000 Genomes Project", intValues, doubleValues, freqValues, stringValues, boolValues);

            if (Id != null) suppInterval.AddStringValue("id", Id);
            if (AfrFreq != null) suppInterval.AddFrequencyValue("variantFreqAfr", Convert.ToDouble(AfrFreq));
            if (AllFreq != null) suppInterval.AddFrequencyValue("variantFreqAll", Convert.ToDouble(AllFreq));
            if (AmrFreq != null) suppInterval.AddFrequencyValue("variantFreqAmr", Convert.ToDouble(AmrFreq));
            if (EasFreq != null) suppInterval.AddFrequencyValue("variantFreqEas", Convert.ToDouble(EasFreq));
            if (EurFreq != null) suppInterval.AddFrequencyValue("variantFreqEur", Convert.ToDouble(EurFreq));
            if (SasFreq != null) suppInterval.AddFrequencyValue("variantFreqSas", Convert.ToDouble(SasFreq));

            if (AllAlleleNumber != null && AllAlleleNumber.Value > 0) suppInterval.AddIntValue("sampleSize", AllAlleleNumber.Value);
            if (AfrAlleleNumber != null && AfrAlleleNumber.Value > 0) suppInterval.AddIntValue("sampleSizeAfr", AfrAlleleNumber.Value);
            if (AmrAlleleNumber != null && AmrAlleleNumber.Value > 0) suppInterval.AddIntValue("sampleSizeAmr", AmrAlleleNumber.Value);
            if (EasAlleleNumber != null && EasAlleleNumber.Value > 0) suppInterval.AddIntValue("sampleSizeEas", EasAlleleNumber.Value);
            if (EurAlleleNumber != null && EurAlleleNumber.Value > 0) suppInterval.AddIntValue("sampleSizeEur", EurAlleleNumber.Value);
            if (SasAlleleNumber != null && SasAlleleNumber.Value > 0) suppInterval.AddIntValue("sampleSizeSas", SasAlleleNumber.Value);

            if (ObservedGains != 0) suppInterval.AddIntValue("observedGains", ObservedGains);
            if (ObservedLosses != 0) suppInterval.AddIntValue("observedLosses", ObservedLosses);

            return suppInterval;
        }




        public override bool Equals(object other)
        {
            // If parameter is null return false.

            // if other cannot be cast into OneKGenItem, return false
            if (!(other is OneKGenItem otherItem)) return false;

            // Return true if the fields match:
            return Equals(Chromosome, otherItem.Chromosome)
                && Start == otherItem.Start
                && AlternateAllele.Equals(otherItem.AlternateAllele)
                ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (ReferenceAllele?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (AlternateAllele?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

		public string GetVcfString()
		{
			var freq = ComputingUtilities.ComputeFrequency(AllAlleleNumber, AllAlleleCount) ?? "";
			var ancestralAlleleString = AncestralAllele ?? "";
			return freq + ";" + ancestralAlleleString;
		}

		public string GetJsonString()
		{
			var sb = StringBuilderCache.Acquire();
			var jsonObject = new JsonObject(sb);
			jsonObject.AddStringValue("ancestralAllele", AncestralAllele);
			jsonObject.AddStringValue("allAf", ComputingUtilities.ComputeFrequency(AllAlleleNumber, AllAlleleCount), false);
			jsonObject.AddStringValue("afrAf", ComputingUtilities.ComputeFrequency(AfrAlleleNumber, AfrAlleleCount), false);
			jsonObject.AddStringValue("amrAf", ComputingUtilities.ComputeFrequency(AmrAlleleNumber, AmrAlleleCount), false);
			jsonObject.AddStringValue("easAf", ComputingUtilities.ComputeFrequency(EasAlleleNumber, EasAlleleCount), false);
			jsonObject.AddStringValue("eurAf", ComputingUtilities.ComputeFrequency(EurAlleleNumber, EurAlleleCount), false);
			jsonObject.AddStringValue("sasAf", ComputingUtilities.ComputeFrequency(SasAlleleNumber, SasAlleleCount), false);

			if (AllAlleleNumber != null) jsonObject.AddIntValue("allAn", AllAlleleNumber.Value);
			if (AfrAlleleNumber != null) jsonObject.AddIntValue("afrAn", AfrAlleleNumber.Value);
			if (AmrAlleleNumber != null) jsonObject.AddIntValue("amrAn", AmrAlleleNumber.Value);
			if (EasAlleleNumber != null) jsonObject.AddIntValue("easAn", EasAlleleNumber.Value);
			if (EurAlleleNumber != null) jsonObject.AddIntValue("eurAn", EurAlleleNumber.Value);
			if (SasAlleleNumber != null) jsonObject.AddIntValue("sasAn", SasAlleleNumber.Value);

			if (AllAlleleCount != null) jsonObject.AddIntValue("allAc", AllAlleleCount.Value);
			if (AfrAlleleCount != null) jsonObject.AddIntValue("afrAc", AfrAlleleCount.Value);
			if (AmrAlleleCount != null) jsonObject.AddIntValue("amrAc", AmrAlleleCount.Value);
			if (EasAlleleCount != null) jsonObject.AddIntValue("easAc", EasAlleleCount.Value);
			if (EurAlleleCount != null) jsonObject.AddIntValue("eurAc", EurAlleleCount.Value);
			if (SasAlleleCount != null) jsonObject.AddIntValue("sasAc", SasAlleleCount.Value);

		    return StringBuilderCache.GetStringAndRelease(sb);
		}
	}
}
