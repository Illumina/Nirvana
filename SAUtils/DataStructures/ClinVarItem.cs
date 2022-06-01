using System;
using System.Collections.Generic;
using System.Linq;
using Genome;
using SAUtils.InputFileParsers.ClinVar;
using SAUtils.Schema;
using VariantAnnotation.Interface.SA;


namespace SAUtils.DataStructures
{
    public sealed class ClinVarItem : IClinVarSaItem
    {
        public Chromosome Chromosome { get; }
        public int         Position   { get; set; }
        public string      RefAllele  { get; set; }
        public string      AltAllele  { get; set; }
        public string      InputLine  { get; }

        public  int                        Stop             { get; }
        public  string                     VariantType      { get; }
        public  string                     Id               { get; }
        public  string                     VariationId      { get; set; }
        public  IEnumerable<string>        AlleleOrigins    { get; }
        public  IEnumerable<string>        Phenotypes       { get; }
        public  IEnumerable<string>        Significances    { get; }
        public  ClinVarCommon.ReviewStatus ReviewStatus     { get; }
        private string                     IsAlleleSpecific { get; }
        public  IEnumerable<string>        MedGenIds        { get; }
        public  IEnumerable<string>        OmimIds          { get; }
        public  IEnumerable<string>        OrphanetIds      { get; }

        public IEnumerable<long> PubmedIds { get; }
        public long LastUpdatedDate { get; }

        public SaJsonSchema JsonSchema { get; }

        public ClinVarItem(Chromosome chromosome,
            int position,
            int stop,
            string refAllele,
            string altAllele,
            SaJsonSchema jsonSchema,
            IEnumerable<string> alleleOrigins,
            string variantType,
            string id,
            string variationId,
            ClinVarCommon.ReviewStatus reviewStatus,
            IEnumerable<string> medGenIds,
            IEnumerable<string> omimIds,
            IEnumerable<string> orphanetIds,
            IEnumerable<string> phenotypes,
            IEnumerable<string> significances,
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
            VariationId      = variationId;
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
                new[] {VariationId},
                new[] {ClinVarCommon.ReviewStatusStrings[ReviewStatus]},
                AlleleOrigins?.ToArray(),
                new[] {ClinVarCommon.NormalizeAllele(RefAllele)},
                new[] {ClinVarCommon.NormalizeAllele(AltAllele)},
                Phenotypes?.ToArray(),
                MedGenIds?.ToArray(),
                OmimIds?.ToArray(),
                OrphanetIds?.ToArray(),
                Significances?.ToArray(),
                new[] {new DateTime(LastUpdatedDate).ToString("yyyy-MM-dd")},
                PubmedIds?.OrderBy(x => x).Select(x => x.ToString()).ToArray()
            };
            
            return values;
        }

        public int CompareTo(IClinVarSaItem other)
        {
            return Chromosome.Index != other.Chromosome.Index
                ? Chromosome.Index.CompareTo(other.Chromosome.Index)
                : Position.CompareTo(other.Position);
        }
    }

    
}
