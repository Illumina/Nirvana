using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

// ReSharper disable NonReadonlyMemberInGetHashCode

namespace SAUtils.DataStructures
{
    public enum GnomadDataType : byte
    {
        Unknown,
        Genome,
        Exome
    }
    public sealed class GnomadItem : ISupplementaryDataItem
    {
        #region members
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }

        public int? AllAlleleCount { get; private set; }
        public int? AfrAlleleCount { get; private set; }
        public int? AmrAlleleCount { get; private set; }
        public int? EasAlleleCount { get; private set; }
        public int? FinAlleleCount { get; private set; }
        public int? NfeAlleleCount { get; private set; }
        public int? OthAlleleCount { get; private set; }
        public int? AsjAlleleCount { get; private set; }
        public int? SasAlleleCount { get; private set; }
        public int? AllAlleleNumber { get; private set; }
        public int? AfrAlleleNumber { get; private set; }
        public int? AmrAlleleNumber { get; private set; }
        public int? EasAlleleNumber { get; private set; }
        public int? FinAlleleNumber { get; private set; }
        public int? NfeAlleleNumber { get; private set; }
        public int? OthAlleleNumber { get; private set; }
        public int? AsjAlleleNumber { get; private set; }
        public int? SasAlleleNumber { get; private set; }

        public int? AllHomCount { get; private set; }
        public int? AfrHomCount { get; private set; }
        public int? AmrHomCount { get; private set; }
        public int? EasHomCount { get; private set; }
        public int? FinHomCount { get; private set; }
        public int? NfeHomCount { get; private set; }
        public int? OthHomCount { get; private set; }
        public int? AsjHomCount { get; private set; }
        public int? SasHomCount { get; private set; }

        //male counts
        public int? MaleAlleleCount { get; private set; }
        public int? MaleAlleleNumber { get; private set; }
        public int? MaleHomCount { get; private set; }

        //female counts
        public int? FemaleAlleleCount { get; private set; }
        public int? FemaleAlleleNumber { get; private set; }
        public int? FemaleHomCount { get; private set; }

        //controls
        public int? ControlsAllAlleleCount { get; private set; }
        public int? ControlsAllAlleleNumber { get; private set; }
        
        public int? Depth { get; }
        public int? Coverage { get; }
        public bool HasFailedFilters { get; }
        public bool IsLowComplexityRegion { get; }
        public GnomadDataType DataType { get; }

        #endregion

        public GnomadItem(IChromosome chromosome,
            int position,
            string refAllele,
            string alternateAllele,
            int? depth,
            int? allAlleleNumber, int? afrAlleleNumber, int? amrAlleleNumber, int? easAlleleNumber,
            int? finAlleleNumber, int? nfeAlleleNumber, int? othAlleleNumber, int? asjAlleleNumber, int? sasAlleleNumber, 
            int? maleAlleleNumber, int? femaleAlleleNumber,
            int? allAlleleCount, int? afrAlleleCount, int? amrAlleleCount, int? easAlleleCount, int? finAlleleCount, int? nfeAlleleCount, int? othAlleleCount, int? asjAlleleCount, int? sasAlleleCount,
            int? maleAlleleCount, int? femaleAlleleCount,
            int? allHomCount, int? afrHomCount, int? amrHomCount, int? easHomCount,
            int? finHomCount, int? nfeHomCount, int? othHomCount, int? asjHomCount, int? sasHomCount,
            int? maleHomCount, int? femaleHomCount,
            int? controlsAllAlleleNumber,
            int? controlsAllAlleleCount,
            bool hasFailedFilters,
            bool isLcr,
            GnomadDataType dataType)
        {
            Chromosome = chromosome;
            Position = position;
            RefAllele = refAllele;
            AltAllele = alternateAllele;

            Depth = depth;
            if (depth!=null && allAlleleNumber!=null && allAlleleNumber.Value > 0)
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

            MaleAlleleNumber = maleAlleleNumber;
            FemaleAlleleNumber = femaleAlleleNumber;
            MaleHomCount = maleHomCount;

            AllAlleleCount = allAlleleCount;
            AfrAlleleCount = afrAlleleCount;
            AmrAlleleCount = amrAlleleCount;
            EasAlleleCount = easAlleleCount;
            FinAlleleCount = finAlleleCount;
            NfeAlleleCount = nfeAlleleCount;
            OthAlleleCount = othAlleleCount;
            AsjAlleleCount = asjAlleleCount;
            SasAlleleCount = sasAlleleCount;

            MaleAlleleCount = maleAlleleCount;
            FemaleAlleleCount = femaleAlleleCount;
            FemaleHomCount = femaleHomCount;

            AllHomCount = allHomCount;
            AfrHomCount = afrHomCount;
            AmrHomCount = amrHomCount;
            EasHomCount = easHomCount;
            FinHomCount = finHomCount;
            NfeHomCount = nfeHomCount;
            OthHomCount = othHomCount;
            AsjHomCount = asjHomCount;
            SasHomCount = sasHomCount;

            //controls
            ControlsAllAlleleNumber = controlsAllAlleleNumber;
            ControlsAllAlleleCount = controlsAllAlleleCount;
            
            HasFailedFilters = hasFailedFilters;
            IsLowComplexityRegion = isLcr;
            DataType = dataType;

            RemoveAlleleNumberZero();
        }

        private void RemoveAlleleNumberZero()
        {
            if (SaUtilsCommon.IsNumberNullOrZero(AllAlleleNumber ))
            {
                AllAlleleNumber = null;
                AllAlleleCount = null;
                AllHomCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(MaleAlleleNumber))
            {
                MaleAlleleNumber = null;
                MaleAlleleCount = null;
                MaleHomCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(FemaleAlleleNumber))
            {
                FemaleAlleleNumber = null;
                FemaleAlleleCount = null;
                FemaleHomCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(AfrAlleleNumber ))
            {
                AfrAlleleNumber = null;
                AfrAlleleCount = null;
                AfrHomCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(AmrAlleleNumber))
            {
                AmrAlleleNumber = null;
                AmrAlleleCount = null;
                AmrHomCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(EasAlleleNumber ))
            {
                EasAlleleNumber = null;
                EasAlleleCount = null;
                EasHomCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(FinAlleleNumber ))
            {
                FinAlleleNumber = null;
                FinAlleleCount = null;
                FinHomCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(NfeAlleleNumber ))
            {
                NfeAlleleNumber = null;
                NfeAlleleCount = null;
                NfeHomCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(OthAlleleNumber ))
            {
                OthAlleleNumber = null;
                OthAlleleCount = null;
                OthHomCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(AsjAlleleNumber ))
            {
                AsjAlleleNumber = null;
                AsjAlleleCount = null;
                AsjHomCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(SasAlleleNumber ))
            {
                SasAlleleNumber = null;
                SasAlleleCount = null;
                SasHomCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(MaleAlleleNumber))
            {
                MaleAlleleNumber = null;
                MaleAlleleCount = null;
                MaleHomCount = null;
            }

            if (SaUtilsCommon.IsNumberNullOrZero(FemaleAlleleNumber))
            {
                FemaleAlleleNumber = null;
                FemaleAlleleCount = null;
                FemaleHomCount = null;
            }

            //controls
            if (SaUtilsCommon.IsNumberNullOrZero(ControlsAllAlleleNumber))
            {
                ControlsAllAlleleNumber = null;
                ControlsAllAlleleCount = null;
            }

            
        }


        
		public string GetJsonString()
		{
			var sb = StringBuilderCache.Acquire();
			var jsonObject = new JsonObject(sb);
			jsonObject.AddIntValue("coverage", Coverage);
		    if (HasFailedFilters) jsonObject.AddBoolValue("failedFilter", true);
            if (IsLowComplexityRegion) jsonObject.AddBoolValue("lowComplexityRegion", true);

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

            jsonObject.AddStringValue("maleAf", ComputingUtilities.ComputeFrequency(MaleAlleleNumber, MaleAlleleCount), false);
            jsonObject.AddIntValue("maleAn", MaleAlleleNumber);
            jsonObject.AddIntValue("maleAc", MaleAlleleCount);
            jsonObject.AddIntValue("maleHc", MaleHomCount);

            jsonObject.AddStringValue("femaleAf", ComputingUtilities.ComputeFrequency(FemaleAlleleNumber, FemaleAlleleCount), false);
            jsonObject.AddIntValue("femaleAn", FemaleAlleleNumber);
            jsonObject.AddIntValue("femaleAc", FemaleAlleleCount);
            jsonObject.AddIntValue("femaleHc", FemaleHomCount);

            //controls
            //jsonObject.AddIntValue("controlsCoverage", ControlsCoverage);
            jsonObject.AddStringValue("controlsAllAf", ComputingUtilities.ComputeFrequency(ControlsAllAlleleNumber, ControlsAllAlleleCount), false);
            jsonObject.AddIntValue("controlsAllAn", ControlsAllAlleleNumber);
            jsonObject.AddIntValue("controlsAllAc", ControlsAllAlleleCount);

            return StringBuilderCache.GetStringAndRelease(sb);
		}

        public static int CompareTo(GnomadItem item, GnomadItem other)
        {
            if (other == null) return -1;
            return item.Chromosome.Index == other.Chromosome.Index ? item.Position.CompareTo(other.Position) : item.Chromosome.Index.CompareTo(other.Chromosome.Index);
        }
    }
}
