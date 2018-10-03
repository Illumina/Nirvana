using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class ClinVarItem : ISupplementaryDataItem, IComparable<ClinVarItem>
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }

        public int Stop { get; }
        public string VariantType { get; }
        public string Id { get; }
        public IEnumerable<string> AlleleOrigins { get; }
        public IEnumerable<string> Phenotypes { get; }
        public string Significance { get;  }
	    public ReviewStatus ReviewStatus { get;}
	    private string IsAlleleSpecific { get; }
		public IEnumerable<string> MedGenIDs { get; }
        public IEnumerable<string> OmimIDs { get; }
        public IEnumerable<string> OrphanetIDs { get; }

        public IEnumerable<long> PubmedIds { get; }
        public long LastUpdatedDate { get; }
		
		public static readonly ImmutableDictionary<string, ReviewStatus> ReviewStatusNameMapping = new Dictionary<string, ReviewStatus>
		{
			["no_assertion"]                                         = ReviewStatus.no_assertion,
			["no_criteria"]                                          = ReviewStatus.no_criteria,
			["guideline"]                                            = ReviewStatus.practice_guideline,
			["single"]                                               = ReviewStatus.single_submitter,
			["mult"]                                                 = ReviewStatus.multiple_submitters,
			["conf"]                                                 = ReviewStatus.conflicting_interpretations,
			["exp"]                                                  = ReviewStatus.expert_panel,
			// the following are the long forms found in XML
			["no assertion provided"]                                = ReviewStatus.no_assertion,
			["no assertion criteria provided"]                       = ReviewStatus.no_criteria,
			["practice guideline"]                                   = ReviewStatus.practice_guideline,
			["criteria provided, conflicting interpretations"]       = ReviewStatus.conflicting_interpretations,
			["reviewed by expert panel"]                             = ReviewStatus.expert_panel,
			["classified by multiple submitters"]                    = ReviewStatus.multiple_submitters,
			["criteria provided, multiple submitters, no conflicts"] = ReviewStatus.multiple_submitters_no_conflict,
			["criteria provided, single submitter"]                  = ReviewStatus.single_submitter
		}.ToImmutableDictionary();

		private static readonly Dictionary< ReviewStatus, string> ReviewStatusStrings = new Dictionary<ReviewStatus, string>
		{			
			[ReviewStatus.no_criteria]                     = "no assertion criteria provided",
			[ReviewStatus.no_assertion]                    = "no assertion provided",
			[ReviewStatus.expert_panel]                    = "reviewed by expert panel",
			[ReviewStatus.single_submitter]                = "criteria provided, single submitter",
			[ReviewStatus.practice_guideline]              = "practice guideline",
			[ReviewStatus.multiple_submitters]             = "classified by multiple submitters",
			[ReviewStatus.conflicting_interpretations]     = "criteria provided, conflicting interpretations",
			[ReviewStatus.multiple_submitters_no_conflict] = "criteria provided, multiple submitters, no conflicts"
			
		};

		public ClinVarItem(IChromosome chromosome,
			int position,
            int stop,
		    string referenceAllele,
		    string altAllele, 
		    IEnumerable<string> alleleOrigins,
			string variantType,
			string id,
			ReviewStatus reviewStatus,
		    IEnumerable<string> medGenIds,
		    IEnumerable<string> omimIds,
		    IEnumerable<string> orphanetIds,
		    IEnumerable<string> phenotypes,
			string significance,
		    IEnumerable<long> pubmedIds = null,
			long lastUpdatedDate = long.MinValue
			)
		{
            Chromosome       = chromosome;
            Position         = position;
            Stop             = stop;
            AlleleOrigins    = alleleOrigins;
            AltAllele  = altAllele;
            VariantType      = variantType;
            Id               = id;
            MedGenIDs        = medGenIds;
            OmimIDs          = omimIds;
            OrphanetIDs      = orphanetIds;
            Phenotypes       = phenotypes;
            RefAllele  = referenceAllele;
            Significance     = significance;
            PubmedIds        = pubmedIds;
            LastUpdatedDate  = lastUpdatedDate;
            IsAlleleSpecific = null;
            ReviewStatus     = reviewStatus;
		}

        

        public string GetJsonString()
		{
			var sb = StringBuilderCache.Acquire();
			var jsonObject = new JsonObject(sb);

			//converting empty alleles to '-'
			var refAllele = string.IsNullOrEmpty(RefAllele) ? "-" : RefAllele;
			var altAllele = string.IsNullOrEmpty(AltAllele) ? "-" : AltAllele;

			//the reduced alt allele should never be output
			altAllele = SaUtilsCommon.ReverseSaReducedAllele(altAllele);

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

        public int CompareTo(ClinVarItem other)
        {
            return Chromosome.Index != other.Chromosome.Index
                ? Chromosome.Index.CompareTo(other.Chromosome.Index)
                : Position.CompareTo(other.Position);
        }
    }

	public enum ReviewStatus
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
