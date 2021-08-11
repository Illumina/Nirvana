using System;
using System.Collections.Generic;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.InputFileParsers.ClinVar
{
    public sealed class VcvSaItem: ISupplementaryDataItem, IComparable<VcvSaItem>
    {
        public IChromosome Chromosome { get; }
        public int         Position   { get; set; }
        public string      RefAllele  { get; set; }
        public string      AltAllele  { get; set; }

        private readonly string       _accession;
        private readonly string       _version;
        private readonly DateTime     _lastUpdatedDate;
        private readonly ClinVarCommon.ReviewStatus _reviewStatus;
        private readonly IEnumerable<string> _significances;

        public VcvSaItem(IChromosome chromosome, int position, string refAllele, string altAllele, string accession, string version, DateTime lastUpdatedDate, ClinVarCommon.ReviewStatus reviewStatus, IEnumerable<string> significances)
        {
            Chromosome = chromosome;
            Position   = position;
            RefAllele  = refAllele;
            AltAllele  = altAllele;

            _accession       = accession;
            _version         = version;
            _lastUpdatedDate = lastUpdatedDate;
            _reviewStatus    = reviewStatus;
            _significances   = significances;
        }

        public string GetJsonString()
        {
            var sb= StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("id", $"{_accession}.{_version}");
            jsonObject.AddStringValue("reviewStatus", ClinVarCommon.ReviewStatusStrings[_reviewStatus]);
            jsonObject.AddStringValues("significance", _significances);
            jsonObject.AddStringValue("refAllele", ClinVarCommon.NormalizeAllele(RefAllele));
            jsonObject.AddStringValue("altAllele", ClinVarCommon.NormalizeAllele(AltAllele));
            jsonObject.AddStringValue("lastUpdatedDate", _lastUpdatedDate.ToString("yyyy-MM-dd"));

            return StringBuilderPool.GetStringAndReturn(sb);
        }

        public int CompareTo(VcvSaItem other)
        {
            return Chromosome.Index != other.Chromosome.Index
                ? Chromosome.Index.CompareTo(other.Chromosome.Index)
                : Position.CompareTo(other.Position);
        }
    }
}