using System;
using System.Collections.Generic;
using System.Linq;
using CommonUtilities;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class ClinVarItem : SupplementaryDataItem
    {
        #region members

        public int Stop { get; }
        public string VariantType { get; }
        public string Id { get; }
        public IEnumerable<string> AlleleOrigins { get; }
        public IEnumerable<string> Phenotypes { get; }
        public string Significance { get;  }
	    public ReviewStatusEnum ReviewStatus { get;}
	    private string IsAlleleSpecific { get; }
		public IEnumerable<string> MedGenIDs { get; }
        public IEnumerable<string> OmimIDs { get; }
        public IEnumerable<string> OrphanetIDs { get; }

        public IEnumerable<long> PubmedIds { get; }
        public long LastUpdatedDate { get; }
		
		public static readonly Dictionary<string, ReviewStatusEnum> ReviewStatusNameMapping = new Dictionary<string, ReviewStatusEnum>
		{
			["no_assertion"]                                         = ReviewStatusEnum.no_assertion,
			["no_criteria"]                                          = ReviewStatusEnum.no_criteria,
			["guideline"]                                            = ReviewStatusEnum.practice_guideline,
			["single"]                                               = ReviewStatusEnum.single_submitter,
			["mult"]                                                 = ReviewStatusEnum.multiple_submitters,
			["conf"]                                                 = ReviewStatusEnum.conflicting_interpretations,
			["exp"]                                                  = ReviewStatusEnum.expert_panel,
			//the following are the long forms found in XML
			["no assertion provided"]                                = ReviewStatusEnum.no_assertion,
			["no assertion criteria provided"]                       = ReviewStatusEnum.no_criteria,
			["practice guideline"]                                   = ReviewStatusEnum.practice_guideline,
			["criteria provided, conflicting interpretations"]       = ReviewStatusEnum.conflicting_interpretations,
			["reviewed by expert panel"]                             = ReviewStatusEnum.expert_panel,
			["classified by multiple submitters"]                    = ReviewStatusEnum.multiple_submitters,
			["criteria provided, multiple submitters, no conflicts"] = ReviewStatusEnum.multiple_submitters_no_conflict,
			["criteria provided, single submitter"]                  = ReviewStatusEnum.single_submitter
		};

		private static readonly Dictionary< ReviewStatusEnum, string> ReviewStatusStrings = new Dictionary<ReviewStatusEnum, string>
		{			
			[ReviewStatusEnum.no_criteria]                     = "no assertion criteria provided",
			[ReviewStatusEnum.no_assertion]                    = "no assertion provided",
			[ReviewStatusEnum.expert_panel]                    = "reviewed by expert panel",
			[ReviewStatusEnum.single_submitter]                = "criteria provided, single submitter",
			[ReviewStatusEnum.practice_guideline]              = "practice guideline",
			[ReviewStatusEnum.multiple_submitters]             = "classified by multiple submitters",
			[ReviewStatusEnum.conflicting_interpretations]     = "criteria provided, conflicting interpretations",
			[ReviewStatusEnum.multiple_submitters_no_conflict] = "criteria provided, multiple submitters, no conflicts"
			
		};

		#endregion

		#region Equality Overrides

		public override int GetHashCode()
		{
		    // ReSharper disable NonReadonlyMemberInGetHashCode
            var hashCode = Start.GetHashCode();
            if (Chromosome      != null) hashCode ^= Chromosome.GetHashCode();
            if (Id              != null) hashCode ^= Id.GetHashCode();
            if (AlternateAllele != null) hashCode ^= AlternateAllele.GetHashCode();
            if (ReferenceAllele != null) hashCode ^= ReferenceAllele.GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ClinVarItem item)) return false;
            return Chromosome == item.Chromosome
                   && Start == item.Start
                   && Id.Equals(item.Id)
                   && ReferenceAllele.Equals(item.ReferenceAllele)
                   && AlternateAllele.Equals(item.AlternateAllele);
        }

        #endregion

		public ClinVarItem(IChromosome chromosome,
			int position,
            int stop,
			IEnumerable<string> alleleOrigins,
			string altAllele,
            string variantType,
			string id,
			ReviewStatusEnum reviewStatus,
		    IEnumerable<string> medGenIds,
		    IEnumerable<string> omimIds,
		    IEnumerable<string> orphanetIds,
		    IEnumerable<string> phenotypes,
			string referenceAllele,
			string significance,
		    IEnumerable<long> pubmedIds = null,
			long lastUpdatedDate = long.MinValue
			)
		{
			Chromosome         = chromosome;
			Start              = position;
		    Stop               = stop;
			AlleleOrigins      = alleleOrigins;
			AlternateAllele    = altAllele;
		    VariantType        = variantType;
		    Id                 = id;
		    MedGenIDs          = medGenIds ;
			OmimIDs            = omimIds;
			OrphanetIDs        = orphanetIds;
			Phenotypes         = phenotypes; 
			ReferenceAllele    = referenceAllele;
			Significance       = significance;
			PubmedIds          = pubmedIds;
			LastUpdatedDate    = lastUpdatedDate;
			IsAlleleSpecific   = null;
		    ReviewStatus = reviewStatus;

		}

		public string GetJsonString()
		{
			var sb = StringBuilderCache.Acquire();
			var jsonObject = new JsonObject(sb);

			//converting empty alleles to '-'
			var refAllele = string.IsNullOrEmpty(ReferenceAllele) ? "-" : ReferenceAllele;
			var altAllele = string.IsNullOrEmpty(AlternateAllele) ? "-" : AlternateAllele;

			//the reduced alt allele should never be output
			altAllele = SupplementaryAnnotationUtilities.ReverseSaReducedAllele(altAllele);

			jsonObject.AddStringValue("id", Id);
			jsonObject.AddStringValue("reviewStatus", ReviewStatusStrings[ReviewStatus]);
			jsonObject.AddStringValue("isAlleleSpecific", IsAlleleSpecific, false);
			jsonObject.AddStringValues("alleleOrigins", AlleleOrigins);
			jsonObject.AddStringValue("refAllele", "N" == refAllele ? null : refAllele);
			jsonObject.AddStringValue("altAllele", "N" == altAllele ? null : altAllele);
			jsonObject.AddStringValues("phenotypes", Phenotypes);
			jsonObject.AddStringValues("medGenIds", MedGenIDs);
			jsonObject.AddStringValues("omimIds", OmimIDs);
			jsonObject.AddStringValues("orphanetIds", OrphanetIDs);
			jsonObject.AddStringValue("significance", Significance);

			if (LastUpdatedDate != long.MinValue)
				jsonObject.AddStringValue("lastUpdatedDate", new DateTime(LastUpdatedDate).ToString("yyyy-MM-dd"));

			jsonObject.AddStringValues("pubMedIds", PubmedIds?.Select(id => id.ToString()));

		    return StringBuilderCache.GetStringAndRelease(sb);
		}

		public override SupplementaryIntervalItem GetSupplementaryInterval()
		{
			throw new NotImplementedException();
		}





	}

	public enum ReviewStatusEnum
	{
		// ReSharper disable InconsistentNaming
		no_assertion,
		no_criteria,
		single_submitter,
		multiple_submitters,
		multiple_submitters_no_conflict,
		conflicting_interpretations,
		expert_panel,
		practice_guideline
		// ReSharper restore InconsistentNaming

	}
}
