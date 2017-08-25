using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class ClinVarItem : SupplementaryDataItem
    {
        #region members

        public string ID { get; }
	    private readonly List<string> _alleleOrigins;
		private readonly List<string> _phenotypes;
		private readonly List<string> _medgenIds;
		private readonly List<string> _omimIds;
		private readonly List<string> _orphanetIds;
		private readonly List<long> _pubmedIds;

		public IEnumerable<string> AlleleOrigins => _alleleOrigins;
		public IEnumerable<string> Phenotypes => _phenotypes;
		public string Significance { get;  }
	    private ReviewStatusEnum ReviewStatus { get;}
	    private string IsAlleleSpecific { get; }
		public IEnumerable<string> MedGenIDs => _medgenIds;
		public IEnumerable<string> OmimIDs => _omimIds;
		public IEnumerable<string> OrphanetIDs => _orphanetIds;

		public IEnumerable<long> PubmedIds => _pubmedIds;
		public long LastUpdatedDate { get; }
		
		private static readonly Dictionary<string, ReviewStatusEnum> ReviewStatusNameMapping = new Dictionary<string, ReviewStatusEnum>
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
			[ReviewStatusEnum.multiple_submitters_no_conflict] = "criteria provided, multiple submitters, no conflicts",
			
		};

		#endregion

		#region Equality Overrides

		public override int GetHashCode()
		{
		    // ReSharper disable NonReadonlyMemberInGetHashCode
            var hashCode = Start.GetHashCode();
            if (Chromosome      != null) hashCode ^= Chromosome.GetHashCode();
            if (ID              != null) hashCode ^= ID.GetHashCode();
            if (AlleleOrigins   != null) hashCode ^= AlleleOrigins.GetHashCode();
            if (AlternateAllele != null) hashCode ^= AlternateAllele.GetHashCode();
            if (MedGenIDs       != null) hashCode ^= MedGenIDs.GetHashCode();
            if (OmimIDs         != null) hashCode ^= OmimIDs.GetHashCode();
            if (OrphanetIDs     != null) hashCode ^= OrphanetIDs.GetHashCode();
            if (Phenotypes      != null) hashCode ^= Phenotypes.GetHashCode();
            if (ReferenceAllele != null) hashCode ^= ReferenceAllele.GetHashCode();
            if (Significance    != null) hashCode ^= Significance.GetHashCode();
            // ReSharper restore NonReadonlyMemberInGetHashCode
            return hashCode;
        }

        #endregion

		public ClinVarItem(IChromosome chromosome,
			int position,
			List<string> alleleOrigins,
			string altAllele,
			string id,
			string reviewStatusString,
			List<string> medGenIds,
			List<string> omimIds,
			List<string> orphanetIds,
			List<string> phenotypes,
			string referenceAllele,
			string significance,
			List<long> pubmedIds = null,
			long lastUpdatedDate = long.MinValue
			)
		{
			Chromosome         = chromosome;
			Start              = position;
			_alleleOrigins     = alleleOrigins?.Count==0? null: alleleOrigins;
			AlternateAllele          = altAllele;
		    ID                 = id;
		    _medgenIds         = medGenIds?.Count == 0 ? null : medGenIds ;
			_omimIds           = omimIds?.Count == 0 ? null : omimIds;
			_orphanetIds       = orphanetIds?.Count == 0 ? null : orphanetIds;
			_phenotypes        = phenotypes?.Count == 0 ? null : phenotypes; 
			ReferenceAllele    = referenceAllele;
			Significance       = significance;
			_pubmedIds         = pubmedIds?.Count == 0 ? null : pubmedIds;
			LastUpdatedDate    = lastUpdatedDate;
			IsAlleleSpecific   = null;

			if (reviewStatusString != null)
				if (ReviewStatusNameMapping.ContainsKey(reviewStatusString))
					ReviewStatus = ReviewStatusNameMapping[reviewStatusString];
		}

		public string GetJsonString()
		{
			var sb = new StringBuilder();
			var jsonObject = new JsonObject(sb);

			//converting empty alleles to '-'
			var refAllele = string.IsNullOrEmpty(ReferenceAllele) ? "-" : ReferenceAllele;
			var altAllele = string.IsNullOrEmpty(AlternateAllele) ? "-" : AlternateAllele;

			//the reduced alt allele should never be output
			altAllele = SupplementaryAnnotationUtilities.ReverseSaReducedAllele(altAllele);

			jsonObject.AddStringValue("id", ID);
			jsonObject.AddStringValue("reviewStatus", ReviewStatusStrings[ReviewStatus]);
			jsonObject.AddStringValue("isAlleleSpecific", IsAlleleSpecific, false);
			jsonObject.AddStringValues("alleleOrigins", _alleleOrigins);
			jsonObject.AddStringValue("refAllele", "N" == refAllele ? null : refAllele);
			jsonObject.AddStringValue("altAllele", "N" == altAllele ? null : altAllele);
			jsonObject.AddStringValues("phenotypes", Phenotypes);
			jsonObject.AddStringValues("medGenIds", _medgenIds);
			jsonObject.AddStringValues("omimIds", OmimIDs);
			jsonObject.AddStringValues("orphanetIds", OrphanetIDs);
			jsonObject.AddStringValue("significance", Significance);

			if (LastUpdatedDate != long.MinValue)
				jsonObject.AddStringValue("lastUpdatedDate", new DateTime(LastUpdatedDate).ToString("yyyy-MM-dd"));

			jsonObject.AddStringValues("pubMedIds", PubmedIds?.Select(id => id.ToString()));

			return sb.ToString();
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
