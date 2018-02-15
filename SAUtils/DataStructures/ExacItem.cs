using CommonUtilities;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class ExacItem : SupplementaryDataItem
    {
        #region members

        public int? AllAlleleCount { get; private set; }
        private int? AfrAlleleCount { get; set; }
        private int? AmrAlleleCount { get; set; }
        private int? EasAlleleCount { get; set; }
        private int? FinAlleleCount { get; set; }
        private int? NfeAlleleCount { get; set; }
        private int? OthAlleleCount { get; set; }
        private int? SasAlleleCount { get; set; }
        public int? AllAlleleNumber { get; private set; }
        private int? AfrAlleleNumber { get; set; }
        private int? AmrAlleleNumber { get; set; }
        private int? EasAlleleNumber { get; set; }
        private int? FinAlleleNumber { get; set; }
        private int? NfeAlleleNumber { get; set; }
        private int? OthAlleleNumber { get; set; }
        private int? SasAlleleNumber { get; set; }

        public int Coverage { get; }

        #endregion

        public ExacItem(IChromosome chromosome,
            int position,
            string refAllele,
            string alternateAllele,
            int coverage,
            int? allAlleleNumber, int? afrAlleleNumber, int? amrAlleleNumber, int? easAlleleNumber,
            int? finAlleleNumber, int? nfeAlleleNumber, int? othAlleleNumber, int? sasAlleleNumber, int? allAlleleCount,
            int? afrAlleleCount, int? amrAlleleCount, int? easAlleleCount, int? finAlleleCount, int? nfeAlleleCount,
            int? othAlleleCount, int? sasAlleleCount)
        {
            Chromosome = chromosome;
            Start = position;
            ReferenceAllele = refAllele;
            AlternateAllele = alternateAllele;

            Coverage = coverage;


            AllAlleleNumber = allAlleleNumber;
            AfrAlleleNumber = afrAlleleNumber;
            AmrAlleleNumber = amrAlleleNumber;
            EasAlleleNumber = easAlleleNumber;
            FinAlleleNumber = finAlleleNumber;
            NfeAlleleNumber = nfeAlleleNumber;
            OthAlleleNumber = othAlleleNumber;
            SasAlleleNumber = sasAlleleNumber;

            AllAlleleCount = allAlleleCount;
            AfrAlleleCount = afrAlleleCount;
            AmrAlleleCount = amrAlleleCount;
            EasAlleleCount = easAlleleCount;
            FinAlleleCount = finAlleleCount;
            NfeAlleleCount = nfeAlleleCount;
            OthAlleleCount = othAlleleCount;
            SasAlleleCount = sasAlleleCount;

            RemoveAlleleNumberZero();
        }

        private void RemoveAlleleNumberZero()
        {
            if (AllAlleleNumber == null || AllAlleleNumber.Value == 0)
            {
                AllAlleleNumber = null;
                AllAlleleCount = null;
            }

            if (AfrAlleleNumber == null || AfrAlleleNumber.Value == 0)
            {
                AfrAlleleNumber = null;
                AfrAlleleCount = null;
            }

            if (AmrAlleleNumber == null || AmrAlleleNumber.Value == 0)
            {
                AmrAlleleNumber = null;
                AmrAlleleCount = null;
            }

            if (EasAlleleNumber == null || EasAlleleNumber.Value == 0)
            {
                EasAlleleNumber = null;
                EasAlleleCount = null;
            }

            if (FinAlleleNumber == null || FinAlleleNumber.Value == 0)
            {
                FinAlleleNumber = null;
                FinAlleleCount = null;
            }

            if (NfeAlleleNumber == null || NfeAlleleNumber.Value == 0)
            {
                NfeAlleleNumber = null;
                NfeAlleleCount = null;
            }

            if (OthAlleleNumber == null || OthAlleleNumber.Value == 0)
            {
                OthAlleleNumber = null;
                OthAlleleCount = null;
            }

            if (SasAlleleNumber == null || SasAlleleNumber.Value == 0)
            {
                SasAlleleNumber = null;
                SasAlleleCount = null;
            }
        }




        // note that for an ExacItem, the chromosome, position and alt allele should uniquely identify it. If not, there is an error in the data source.
        public override bool Equals(object other)
        {
            // If parameter is null return false.

            // if other cannot be cast into OneKGenItem, return false
            if (!(other is ExacItem otherItem)) return false;

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
                var hashCode = Start.GetHashCode() ^ Chromosome.GetHashCode();
                hashCode = (hashCode * 397) ^ (AlternateAllele?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

		public string GetJsonString()
		{
			var sb = StringBuilderCache.Acquire();
			var jsonObject = new JsonObject(sb);
			jsonObject.AddIntValue("coverage", Coverage);
			jsonObject.AddStringValue("allAf", ComputingUtilities.ComputeFrequency(AllAlleleNumber, AllAlleleCount), false);
			jsonObject.AddStringValue("afrAf", ComputingUtilities.ComputeFrequency(AfrAlleleNumber, AfrAlleleCount), false);
			jsonObject.AddStringValue("amrAf", ComputingUtilities.ComputeFrequency(AmrAlleleNumber, AmrAlleleCount), false);
			jsonObject.AddStringValue("easAf", ComputingUtilities.ComputeFrequency(EasAlleleNumber, EasAlleleCount), false);
			jsonObject.AddStringValue("finAf", ComputingUtilities.ComputeFrequency(FinAlleleNumber, FinAlleleCount), false);
			jsonObject.AddStringValue("nfeAf", ComputingUtilities.ComputeFrequency(NfeAlleleNumber, NfeAlleleCount), false);
			jsonObject.AddStringValue("sasAf", ComputingUtilities.ComputeFrequency(SasAlleleNumber, SasAlleleCount), false);
			jsonObject.AddStringValue("othAf", ComputingUtilities.ComputeFrequency(OthAlleleNumber, OthAlleleCount), false);

			if (AllAlleleNumber != null) jsonObject.AddIntValue("allAn", AllAlleleNumber.Value);
			if (AfrAlleleNumber != null) jsonObject.AddIntValue("afrAn", AfrAlleleNumber.Value);
			if (AmrAlleleNumber != null) jsonObject.AddIntValue("amrAn", AmrAlleleNumber.Value);
			if (EasAlleleNumber != null) jsonObject.AddIntValue("easAn", EasAlleleNumber.Value);
			if (FinAlleleNumber != null) jsonObject.AddIntValue("finAn", FinAlleleNumber.Value);
			if (NfeAlleleNumber != null) jsonObject.AddIntValue("nfeAn", NfeAlleleNumber.Value);
			if (SasAlleleNumber != null) jsonObject.AddIntValue("sasAn", SasAlleleNumber.Value);
			if (OthAlleleNumber != null) jsonObject.AddIntValue("othAn", OthAlleleNumber.Value);

			if (AllAlleleCount != null) jsonObject.AddIntValue("allAc", AllAlleleCount.Value);
			if (AfrAlleleCount != null) jsonObject.AddIntValue("afrAc", AfrAlleleCount.Value);
			if (AmrAlleleCount != null) jsonObject.AddIntValue("amrAc", AmrAlleleCount.Value);
			if (EasAlleleCount != null) jsonObject.AddIntValue("easAc", EasAlleleCount.Value);
			if (FinAlleleCount != null) jsonObject.AddIntValue("finAc", FinAlleleCount.Value);
			if (NfeAlleleCount != null) jsonObject.AddIntValue("nfeAc", NfeAlleleCount.Value);
			if (SasAlleleCount != null) jsonObject.AddIntValue("sasAc", SasAlleleCount.Value);
			if (OthAlleleCount != null) jsonObject.AddIntValue("othAc", OthAlleleCount.Value);

		    return StringBuilderCache.GetStringAndRelease(sb);
		}

        public override SupplementaryIntervalItem GetSupplementaryInterval()
        {
            return null;
        }
    }
}
