using CommonUtilities;
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

        private int? Coverage { get; }
        private bool HasFailedFilters { get; }

        #endregion

        public GnomadItem(IChromosome chromosome,
            int position,
            string refAllele,
            string alternateAllele,
            int? depth,
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

            if (depth!=null && allAlleleNumber!=null)
                Coverage = ComputingUtilities.GetCoverage(depth.Value, allAlleleNumber.Value);

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

		public string GetJsonString()
		{
			var sb = StringBuilderCache.Acquire();
			var jsonObject = new JsonObject(sb);
			jsonObject.AddIntValue("coverage", Coverage);
		    if (HasFailedFilters) jsonObject.AddBoolValue("failedFilter", true);

            jsonObject.AddStringValue("allAf", ComputingUtilities.ComputeFrequency(AllAlleleNumber, AllAlleleCount), false);
		    jsonObject.AddIntValue("allAn", AllAlleleNumber);
		    jsonObject.AddIntValue("allAc", AllAlleleCount);
		    jsonObject.AddIntValue("allHc", AllHomCount);
            
            jsonObject.AddStringValue("afrAf", ComputingUtilities.ComputeFrequency(AfrAlleleNumber, AfrAlleleCount), false);
		    jsonObject.AddIntValue("afrAn", AfrAlleleNumber);
		    jsonObject.AddIntValue("afrAc", AfrAlleleCount);
		    jsonObject.AddIntValue("afrHc", AfrHomCount);
            
            jsonObject.AddStringValue("amrAf", ComputingUtilities.ComputeFrequency(AmrAlleleNumber, AmrAlleleCount), false);
		    jsonObject.AddIntValue("amrAn", AmrAlleleNumber);
		    jsonObject.AddIntValue("amrAc", AmrAlleleCount);
		    jsonObject.AddIntValue("amrHc", AmrHomCount);

            jsonObject.AddStringValue("easAf", ComputingUtilities.ComputeFrequency(EasAlleleNumber, EasAlleleCount), false);
		    jsonObject.AddIntValue("easAn", EasAlleleNumber);
		    jsonObject.AddIntValue("easAc", EasAlleleCount);
		    jsonObject.AddIntValue("easHc", EasHomCount);

            jsonObject.AddStringValue("finAf", ComputingUtilities.ComputeFrequency(FinAlleleNumber, FinAlleleCount), false);
		    jsonObject.AddIntValue("finAn", FinAlleleNumber);
		    jsonObject.AddIntValue("finAc", FinAlleleCount);
		    jsonObject.AddIntValue("finHc", FinHomCount);

            jsonObject.AddStringValue("nfeAf", ComputingUtilities.ComputeFrequency(NfeAlleleNumber, NfeAlleleCount), false);
			jsonObject.AddIntValue("nfeAn", NfeAlleleNumber);
		    jsonObject.AddIntValue("nfeAc", NfeAlleleCount);
		    jsonObject.AddIntValue("nfeHc", NfeHomCount);

            jsonObject.AddStringValue("asjAf", ComputingUtilities.ComputeFrequency(AsjAlleleNumber, AsjAlleleCount), false);
		    jsonObject.AddIntValue("asjAn", AsjAlleleNumber);
		    jsonObject.AddIntValue("asjAc", AsjAlleleCount);
		    jsonObject.AddIntValue("asjHc", AsjHomCount);

            jsonObject.AddStringValue("sasAf", ComputingUtilities.ComputeFrequency(SasAlleleNumber, SasAlleleCount), false);
		    jsonObject.AddIntValue("sasAn", SasAlleleNumber);
		    jsonObject.AddIntValue("sasAc", SasAlleleCount);
		    jsonObject.AddIntValue("sasHc", SasHomCount);

            jsonObject.AddStringValue("othAf", ComputingUtilities.ComputeFrequency(OthAlleleNumber, OthAlleleCount), false);
            jsonObject.AddIntValue("othAn", OthAlleleNumber);
            jsonObject.AddIntValue("othAc", OthAlleleCount);
		    jsonObject.AddIntValue("othHc", OthHomCount);

		    return StringBuilderCache.GetStringAndRelease(sb);
		}

        public override SupplementaryIntervalItem GetSupplementaryInterval()
        {
            return null;
        }
    }
}
