using Genome;
using OptimizedCore;
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
            if (SaUtilsCommon.IsNumberNullOrZero(AllAlleleNumber))
            {
                AllAlleleNumber = null;
                AllAlleleCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(AfrAlleleNumber))
            {
                AfrAlleleNumber = null;
                AfrAlleleCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(AmrAlleleNumber))
            {
                AmrAlleleNumber = null;
                AmrAlleleCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(EasAlleleNumber ))
            {
                EasAlleleNumber = null;
                EasAlleleCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(FinAlleleNumber ))
            {
                FinAlleleNumber = null;
                FinAlleleCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(NfeAlleleNumber ))
            {
                NfeAlleleNumber = null;
                NfeAlleleCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(OthAlleleNumber ))
            {
                OthAlleleNumber = null;
                OthAlleleCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(SasAlleleNumber ))
            {
                SasAlleleNumber = null;
                SasAlleleCount = null;
            }
        }




        // note that for an ExacItem, the chromosome, position and alt allele should uniquely identify it. If not, there is an error in the data source.
        public override bool Equals(object obj)
        {
            // If parameter is null return false.

            // if other cannot be cast into OneKGenItem, return false
            if (!(obj is ExacItem otherItem)) return false;

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

			jsonObject.AddIntValue("allAn", AllAlleleNumber);
			jsonObject.AddIntValue("afrAn", AfrAlleleNumber);
			jsonObject.AddIntValue("amrAn", AmrAlleleNumber);
			jsonObject.AddIntValue("easAn", EasAlleleNumber);
			jsonObject.AddIntValue("finAn", FinAlleleNumber);
			jsonObject.AddIntValue("nfeAn", NfeAlleleNumber);
			jsonObject.AddIntValue("sasAn", SasAlleleNumber);
			jsonObject.AddIntValue("othAn", OthAlleleNumber);

			jsonObject.AddIntValue("allAc", AllAlleleCount);
			jsonObject.AddIntValue("afrAc", AfrAlleleCount);
			jsonObject.AddIntValue("amrAc", AmrAlleleCount);
			jsonObject.AddIntValue("easAc", EasAlleleCount);
			jsonObject.AddIntValue("finAc", FinAlleleCount);
			jsonObject.AddIntValue("nfeAc", NfeAlleleCount);
			jsonObject.AddIntValue("sasAc", SasAlleleCount);
			jsonObject.AddIntValue("othAc", OthAlleleCount);

		    return StringBuilderCache.GetStringAndRelease(sb);
		}

        public override SupplementaryIntervalItem GetSupplementaryInterval()
        {
            return null;
        }
    }
}
