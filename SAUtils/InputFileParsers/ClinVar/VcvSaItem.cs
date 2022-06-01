using System;
using System.Collections.Generic;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.InputFileParsers.ClinVar
{
    public sealed class VcvSaItem: IClinVarSaItem, IEquatable<VcvSaItem>
    {
        public Chromosome Chromosome { get; }
        public int         Position   { get; set; }
        public string      RefAllele  { get; set; }
        public string      AltAllele  { get; set; }

        private readonly string                     _accession;
        private readonly string                     _version;
        private readonly DateTime                   _lastUpdatedDate;
        public           ClinVarCommon.ReviewStatus ReviewStatus  { get; }
        public           IEnumerable<string>        Significances { get; }
        public           string                     Id            => $"{_accession}.{_version}";

        public VcvSaItem(Chromosome chromosome, int position, string refAllele, string altAllele, string accession, string version, DateTime lastUpdatedDate, ClinVarCommon.ReviewStatus reviewStatus, IEnumerable<string> significances)
        {
            Chromosome = chromosome;
            Position   = position;
            RefAllele  = refAllele;
            AltAllele  = altAllele;

            _accession       = accession;
            _version         = version;
            _lastUpdatedDate = lastUpdatedDate;
            ReviewStatus    = reviewStatus;
            Significances   = significances;
        }

        public string GetJsonString()
        {
            var sb= StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("id", $"{_accession}.{_version}");
            jsonObject.AddStringValue("reviewStatus", ClinVarCommon.ReviewStatusStrings[ReviewStatus]);
            jsonObject.AddStringValues("significance", Significances);
            jsonObject.AddStringValue("refAllele", ClinVarCommon.NormalizeAllele(RefAllele));
            jsonObject.AddStringValue("altAllele", ClinVarCommon.NormalizeAllele(AltAllele));
            jsonObject.AddStringValue("lastUpdatedDate", _lastUpdatedDate.ToString("yyyy-MM-dd"));

            return StringBuilderPool.GetStringAndReturn(sb);
        }

        public string InputLine { get; set; }

        public int CompareTo(IClinVarSaItem other)
        {
            return Chromosome.Index != other.Chromosome.Index
                ? Chromosome.Index.CompareTo(other.Chromosome.Index)
                : Position.CompareTo(other.Position);
        }


        public bool Equals(VcvSaItem other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _accession == other._accession && _version == other._version;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(_accession, _version);
        }
    }
}