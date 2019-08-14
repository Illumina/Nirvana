using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Genome;
using SAUtils.Schema;
using VariantAnnotation.Interface.SA;

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
        public int? VariationId { get; }
        public IEnumerable<string> AlleleOrigins { get; }
        public IEnumerable<string> Phenotypes { get; }
        public string[] Significances { get; }
        public ReviewStatus ReviewStatus { get; }
        private string IsAlleleSpecific { get; }
        public IEnumerable<string> MedGenIds { get; }
        public IEnumerable<string> OmimIds { get; }
        public IEnumerable<string> OrphanetIds { get; }

        public IEnumerable<long> PubmedIds { get; }
        public long LastUpdatedDate { get; }

        public SaJsonSchema JsonSchema { get; }

        public static readonly ImmutableDictionary<string, ReviewStatus> ReviewStatusNameMapping = new Dictionary<string, ReviewStatus>
        {
            ["no_assertion"] = ReviewStatus.no_assertion,
            ["no_criteria"] = ReviewStatus.no_criteria,
            ["guideline"] = ReviewStatus.practice_guideline,
            ["single"] = ReviewStatus.single_submitter,
            ["mult"] = ReviewStatus.multiple_submitters,
            ["conf"] = ReviewStatus.conflicting_interpretations,
            ["exp"] = ReviewStatus.expert_panel,
            // the following are the long forms found in XML
            ["no assertion provided"] = ReviewStatus.no_assertion,
            ["no assertion criteria provided"] = ReviewStatus.no_criteria,
            ["practice guideline"] = ReviewStatus.practice_guideline,
            ["criteria provided, conflicting interpretations"] = ReviewStatus.conflicting_interpretations,
            ["reviewed by expert panel"] = ReviewStatus.expert_panel,
            ["classified by multiple submitters"] = ReviewStatus.multiple_submitters,
            ["criteria provided, multiple submitters, no conflicts"] = ReviewStatus.multiple_submitters_no_conflict,
            ["criteria provided, single submitter"] = ReviewStatus.single_submitter
        }.ToImmutableDictionary();

        private static readonly Dictionary<ReviewStatus, string> ReviewStatusStrings = new Dictionary<ReviewStatus, string>
        {
            [ReviewStatus.no_criteria] = "no assertion criteria provided",
            [ReviewStatus.no_assertion] = "no assertion provided",
            [ReviewStatus.expert_panel] = "reviewed by expert panel",
            [ReviewStatus.single_submitter] = "criteria provided, single submitter",
            [ReviewStatus.practice_guideline] = "practice guideline",
            [ReviewStatus.multiple_submitters] = "classified by multiple submitters",
            [ReviewStatus.conflicting_interpretations] = "criteria provided, conflicting interpretations",
            [ReviewStatus.multiple_submitters_no_conflict] = "criteria provided, multiple submitters, no conflicts"

        };

        public ClinVarItem(IChromosome chromosome,
            int position,
            int stop,
            string refAllele,
            string altAllele,
            SaJsonSchema jsonSchema,
            IEnumerable<string> alleleOrigins,
            string variantType,
            string id,
            int? variationId,
            ReviewStatus reviewStatus,
            IEnumerable<string> medGenIds,
            IEnumerable<string> omimIds,
            IEnumerable<string> orphanetIds,
            IEnumerable<string> phenotypes,
            string[] significances,
            IEnumerable<long> pubmedIds = null,
            long lastUpdatedDate = long.MinValue
            )
        {
            Chromosome       = chromosome;
            Position         = position;
            Stop             = stop;
            AlleleOrigins    = alleleOrigins;
            AltAllele        = altAllele;
            JsonSchema       = jsonSchema;
            VariantType      = variantType;
            Id               = id;
            VariationId        = variationId;
            MedGenIds        = medGenIds;
            OmimIds          = omimIds;
            OrphanetIds      = orphanetIds;
            Phenotypes       = phenotypes;
            RefAllele        = refAllele;
            Significances    = significances;
            PubmedIds        = pubmedIds;
            LastUpdatedDate  = lastUpdatedDate;
            IsAlleleSpecific = null;
            ReviewStatus     = reviewStatus;

        }

        public string GetJsonString()
        {
            return JsonSchema.GetJsonString(GetValues());
        }

        private List<string[]> GetValues()
        {
            var values = new List<string[]>
            {
                //the exact order of adding values has to be preserved. the order is dictated by the json schema
                new[] {Id},
                new[] {VariationId?.ToString()},
                new[] {ReviewStatusStrings[ReviewStatus]},
                AlleleOrigins?.ToArray(),
                new[] {NormalizeAllele(RefAllele)},
                new[] {NormalizeAllele(AltAllele)},
                Phenotypes?.ToArray(),
                MedGenIds?.ToArray(),
                OmimIds?.ToArray(),
                OrphanetIds?.ToArray(),
                Significances,
                new[] {new DateTime(LastUpdatedDate).ToString("yyyy-MM-dd")},
                PubmedIds?.OrderBy(x => x).Select(x => x.ToString()).ToArray()
            };
            
            return values;
        }

        private string NormalizeAllele(string allele)
        {
            if (string.IsNullOrEmpty(allele)) return "-";
            return allele == "N" ? null : allele;
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
