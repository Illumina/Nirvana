using System.Text;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
// ReSharper disable NonReadonlyMemberInGetHashCode

namespace SAUtils.DataStructures
{
    public sealed class GnomadItem : SupplementaryDataItem
    {
        #region members

        private int? AllAlleleCount { get; set; }
        private int? AfrAlleleCount { get; set; }
        private int? AmrAlleleCount { get; set; }
        private int? EasAlleleCount { get; set; }
        private int? FinAlleleCount { get; set; }
        private int? NfeAlleleCount { get; set; }
        private int? OthAlleleCount { get; set; }
        private int? AsjAlleleCount { get; set; }
        private int? SasAlleleCount { get; set; }
        private int? AllAlleleNumber { get; set; }
        private int? AfrAlleleNumber { get; set; }
        private int? AmrAlleleNumber { get; set; }
        private int? EasAlleleNumber { get; set; }
        private int? FinAlleleNumber { get; set; }
        private int? NfeAlleleNumber { get; set; }
        private int? OthAlleleNumber { get; set; }
        private int? AsjAlleleNumber { get; set; }
        private int? SasAlleleNumber { get; set; }

        private int? AllHomCount { get; set; }
        private int? AfrHomCount { get; set; }
        private int? AmrHomCount { get; set; }
        private int? EasHomCount { get; set; }
        private int? FinHomCount { get; set; }
        private int? NfeHomCount { get; set; }
        private int? OthHomCount { get; set; }
        private int? AsjHomCount { get; set; }
        private int? SasHomCount { get; set; }

        private int Coverage { get; }
        private bool HasFailedFilters { get; }

        #endregion

        public GnomadItem(IChromosome chromosome,
            int position,
            string refAllele,
            string alternateAllele,
            int depth,
            int? allAlleleNumber, int? afrAlleleNumber, int? amrAlleleNumber, int? easAlleleNumber,
            int? finAlleleNumber, int? nfeAlleleNumber, int? othAlleleNumber, int? asjAlleleNumber, int? sasAlleleNumber, 
            int? allAlleleCount, int? afrAlleleCount, int? amrAlleleCount, int? easAlleleCount, int? finAlleleCount, int? nfeAlleleCount, int? othAlleleCount, int? asjAlleleCount, int? sasAlleleCount,
            int? allHomCount, int? afrHomCount, int? amrHomCount, int? easHomCount,
            int? finHomCount, int? nfeHomCount, int? othHomCount, int? asjHomCount, int? sasHomCount,
            bool hasFailedFilters)
        {
            Chromosome = chromosome;
            Start = position;
            ReferenceAllele = refAllele;
            AlternateAllele = alternateAllele;

            Coverage = depth/allAlleleNumber.Value;

            AllAlleleNumber = allAlleleNumber;
            AfrAlleleNumber = afrAlleleNumber;
            AmrAlleleNumber = amrAlleleNumber;
            EasAlleleNumber = easAlleleNumber;
            FinAlleleNumber = finAlleleNumber;
            NfeAlleleNumber = nfeAlleleNumber;
            OthAlleleNumber = othAlleleNumber;
            AsjAlleleNumber = asjAlleleNumber;
            SasAlleleNumber = sasAlleleNumber;

            AllAlleleCount = allAlleleCount;
            AfrAlleleCount = afrAlleleCount;
            AmrAlleleCount = amrAlleleCount;
            EasAlleleCount = easAlleleCount;
            FinAlleleCount = finAlleleCount;
            NfeAlleleCount = nfeAlleleCount;
            OthAlleleCount = othAlleleCount;
            AsjAlleleCount = asjAlleleCount;
            SasAlleleCount = sasAlleleCount;

            AllHomCount = allHomCount;
            AfrHomCount = afrHomCount;
            AmrHomCount = amrHomCount;
            EasHomCount = easHomCount;
            FinHomCount = finHomCount;
            NfeHomCount = nfeHomCount;
            OthHomCount = othHomCount;
            AsjHomCount = asjHomCount;
            SasHomCount = sasHomCount;

            HasFailedFilters = hasFailedFilters;

            RemoveAlleleNumberZero();
        }

        private void RemoveAlleleNumberZero()
        {
            if (AllAlleleNumber == null || AllAlleleNumber.Value == 0)
            {
                AllAlleleNumber = null;
                AllAlleleCount = null;
                AllHomCount = null;
            }

            if (AfrAlleleNumber == null || AfrAlleleNumber.Value == 0)
            {
                AfrAlleleNumber = null;
                AfrAlleleCount = null;
                AfrHomCount = null;
            }

            if (AmrAlleleNumber == null || AmrAlleleNumber.Value == 0)
            {
                AmrAlleleNumber = null;
                AmrAlleleCount = null;
                AmrHomCount = null;
            }

            if (EasAlleleNumber == null || EasAlleleNumber.Value == 0)
            {
                EasAlleleNumber = null;
                EasAlleleCount = null;
                EasHomCount = null;
            }

            if (FinAlleleNumber == null || FinAlleleNumber.Value == 0)
            {
                FinAlleleNumber = null;
                FinAlleleCount = null;
                FinHomCount = null;
            }

            if (NfeAlleleNumber == null || NfeAlleleNumber.Value == 0)
            {
                NfeAlleleNumber = null;
                NfeAlleleCount = null;
                NfeHomCount = null;
            }

            if (OthAlleleNumber == null || OthAlleleNumber.Value == 0)
            {
                OthAlleleNumber = null;
                OthAlleleCount = null;
                OthHomCount = null;
            }

            if (AsjAlleleNumber == null || AsjAlleleNumber.Value == 0)
            {
                AsjAlleleNumber = null;
                AsjAlleleCount = null;
                AsjHomCount = null;
            }

            if (SasAlleleNumber == null || SasAlleleNumber.Value == 0)
            {
                SasAlleleNumber = null;
                SasAlleleCount = null;
                SasHomCount = null;
            }
        }




        // note that for an GnomadItem, the chromosome, position and alt allele should uniquely identify it. If not, there is an error in the data source.
        public override bool Equals(object other)
        {
            if (!(other is GnomadItem otherItem)) return false;

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

		private static string ComputeFrequency(int? alleleNumber, int? alleleCount)
		{
			return alleleNumber != null && alleleNumber.Value > 0 && alleleCount != null
				? ((double)alleleCount / alleleNumber.Value).ToString(JsonCommon.FrequencyRoundingFormat)
				: null;
		}

		public string GetJsonString()
		{
			var sb = new StringBuilder();
			var jsonObject = new JsonObject(sb);
			jsonObject.AddIntValue("coverage", Coverage);
		    if (HasFailedFilters) jsonObject.AddBoolValue("hasFailedFilters", true);

            jsonObject.AddStringValue("allAf", ComputeFrequency(AllAlleleNumber, AllAlleleCount), false);
		    if (AllAlleleNumber != null) jsonObject.AddIntValue("allAn", AllAlleleNumber.Value);
		    if (AllAlleleCount != null) jsonObject.AddIntValue("allAc", AllAlleleCount.Value);
		    if (AllHomCount != null) jsonObject.AddIntValue("allHc", AllHomCount.Value);
            
            jsonObject.AddStringValue("afrAf", ComputeFrequency(AfrAlleleNumber, AfrAlleleCount), false);
		    if (AfrAlleleNumber != null) jsonObject.AddIntValue("afrAn", AfrAlleleNumber.Value);
		    if (AfrAlleleCount != null) jsonObject.AddIntValue("afrAc", AfrAlleleCount.Value);
		    if (AfrHomCount != null) jsonObject.AddIntValue("afrHc", AfrHomCount.Value);
            
            jsonObject.AddStringValue("amrAf", ComputeFrequency(AmrAlleleNumber, AmrAlleleCount), false);
		    if (AmrAlleleNumber != null) jsonObject.AddIntValue("amrAn", AmrAlleleNumber.Value);
		    if (AmrAlleleCount != null) jsonObject.AddIntValue("amrAc", AmrAlleleCount.Value);
		    if (AmrHomCount != null) jsonObject.AddIntValue("amrHc", AmrHomCount.Value);

            jsonObject.AddStringValue("easAf", ComputeFrequency(EasAlleleNumber, EasAlleleCount), false);
		    if (EasAlleleNumber != null) jsonObject.AddIntValue("easAn", EasAlleleNumber.Value);
		    if (EasAlleleCount != null) jsonObject.AddIntValue("easAc", EasAlleleCount.Value);
		    if (EasHomCount != null) jsonObject.AddIntValue("easHc", EasHomCount.Value);

            jsonObject.AddStringValue("finAf", ComputeFrequency(FinAlleleNumber, FinAlleleCount), false);
		    if (FinAlleleNumber != null) jsonObject.AddIntValue("finAn", FinAlleleNumber.Value);
		    if (FinAlleleCount != null) jsonObject.AddIntValue("finAc", FinAlleleCount.Value);
		    if (FinHomCount != null) jsonObject.AddIntValue("finHc", FinHomCount.Value);

            jsonObject.AddStringValue("nfeAf", ComputeFrequency(NfeAlleleNumber, NfeAlleleCount), false);
			if (NfeAlleleNumber != null) jsonObject.AddIntValue("nfeAn", NfeAlleleNumber.Value);
		    if (NfeAlleleCount != null) jsonObject.AddIntValue("nfeAc", NfeAlleleCount.Value);
		    if (NfeHomCount != null) jsonObject.AddIntValue("nfeHc", NfeHomCount.Value);

            jsonObject.AddStringValue("asjAf", ComputeFrequency(AsjAlleleNumber, AsjAlleleCount), false);
		    if (AsjAlleleNumber != null) jsonObject.AddIntValue("asjAn", AsjAlleleNumber.Value);
		    if (AsjAlleleCount != null) jsonObject.AddIntValue("asjAc", AsjAlleleCount.Value);
		    if (AsjHomCount != null) jsonObject.AddIntValue("asjHc", AsjHomCount.Value);

            jsonObject.AddStringValue("sasAf", ComputeFrequency(SasAlleleNumber, SasAlleleCount), false);
		    if (SasAlleleNumber != null) jsonObject.AddIntValue("sasAn", SasAlleleNumber.Value);
		    if (SasAlleleCount != null) jsonObject.AddIntValue("sasAc", SasAlleleCount.Value);
		    if (SasHomCount != null) jsonObject.AddIntValue("sasHc", SasHomCount.Value);

            jsonObject.AddStringValue("othAf", ComputeFrequency(OthAlleleNumber, OthAlleleCount), false);
            if (OthAlleleNumber != null) jsonObject.AddIntValue("othAn", OthAlleleNumber.Value);
            if (OthAlleleCount != null) jsonObject.AddIntValue("othAc", OthAlleleCount.Value);
		    if (OthHomCount != null) jsonObject.AddIntValue("othHc", OthHomCount.Value);

            return sb.ToString();
		}

        public override SupplementaryIntervalItem GetSupplementaryInterval()
        {
            return null;
        }
    }
}
