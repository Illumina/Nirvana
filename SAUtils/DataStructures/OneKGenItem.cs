using System;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class OneKGenItem : ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }

        private string Id { get; }
        private string AncestralAllele { get; }

        private string AfrFreq { get; }
        private string AllFreq { get; }
        private string AmrFreq { get; }
        private string EasFreq { get; }
        private string EurFreq { get; }
        private string SasFreq { get; }

        private int? AllAlleleNumber { get; }
        private int? AfrAlleleNumber { get; }
        private int? AmrAlleleNumber { get; }
        private int? EurAlleleNumber { get; }
        private int? EasAlleleNumber { get; }
        private int? SasAlleleNumber { get; }

        private int? AllAlleleCount { get; }
        private int? AfrAlleleCount { get; }
        private int? AmrAlleleCount { get; }
        private int? EurAlleleCount { get; }
        private int? EasAlleleCount { get; }
        private int? SasAlleleCount { get; }

        private string SvType { get; }
        private int SvEnd { get; }
        private int ObservedGains { get; }
        private int ObservedLosses { get; }

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
            Position = position;
            Id = id;
            RefAllele = refAllele;
            AltAllele = alternateAllele;
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

			jsonObject.AddIntValue("allAn", AllAlleleNumber);
			jsonObject.AddIntValue("afrAn", AfrAlleleNumber);
			jsonObject.AddIntValue("amrAn", AmrAlleleNumber);
			jsonObject.AddIntValue("easAn", EasAlleleNumber);
			jsonObject.AddIntValue("eurAn", EurAlleleNumber);
			jsonObject.AddIntValue("sasAn", SasAlleleNumber);

			jsonObject.AddIntValue("allAc", AllAlleleCount);
			jsonObject.AddIntValue("afrAc", AfrAlleleCount);
			jsonObject.AddIntValue("amrAc", AmrAlleleCount);
			jsonObject.AddIntValue("easAc", EasAlleleCount);
			jsonObject.AddIntValue("eurAc", EurAlleleCount);
			jsonObject.AddIntValue("sasAc", SasAlleleCount);

		    return StringBuilderCache.GetStringAndRelease(sb);
		}
        
    }
}
