using System.Collections.Generic;
using System.Text;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.IO;
using VariantAnnotation.Sequence;

namespace SAUtils.DataStructures
{
    public static class MitoMapDataTypes
    {
        public const string MitoMapMutationsCodingControl = "MutationsCodingControl";
        public const string MitoMapMutationsRNA = "MutationsRNA";
        public const string MitoMapPolymorphismsCoding = "PolymorphismsCoding";
        public const string MitoMapPolymorphismsControl = "PolymorphismsControl";
        public const string MitoMapDeletionsSingle = "DeletionsSingle";
        public const string MitoMapInsertionsSimple = "InsertionsSimple";
    }

    public static class MitoDLoop
    {
        public const int Start = 16024;
        public const int End = 576;
    }

    public sealed class MitoMapItem : SupplementaryDataItem
    {
        private readonly string _disease;
        private bool? _homoplasmy;
        private bool? _heteroplasmy;
        private readonly string _status;
        private readonly string _clinicalSignificance;
        private readonly string _scorePercentile;
        private readonly int? _intervalEnd;
        private readonly VariantType? _variantType;
        private static readonly Chromosome ChromM= new Chromosome("chrM", "MT", 24);


        public MitoMapItem(int posi, string refAllele, string altAllele, string disease, bool? homoplasmy, bool? heteroplasmy, string status, string clinicalSignificance, string scorePercentile, bool isInterval, int? intervalEnd, VariantType? variantType)
        {
            Chromosome = ChromM;
            Start = posi;
            ReferenceAllele = refAllele;
            AlternateAllele = altAllele;
            IsInterval = isInterval;
            _disease = disease;
            _homoplasmy = homoplasmy;
            _heteroplasmy = heteroplasmy;
            _status = status;
            _clinicalSignificance = clinicalSignificance;
            _scorePercentile = scorePercentile;
            _intervalEnd = intervalEnd;
            _variantType = variantType;
        }

        public string GetVariantJsonString()
        {
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            //converting empty alleles to '-'
            var refAllele = string.IsNullOrEmpty(ReferenceAllele) ? "-" : ReferenceAllele;
            var altAllele = string.IsNullOrEmpty(AlternateAllele) ? "-" : AlternateAllele;
 
            jsonObject.AddStringValue("refAllele", refAllele);
            jsonObject.AddStringValue("altAllele", altAllele);
            if (!string.IsNullOrEmpty(_disease)) jsonObject.AddStringValue("disease", _disease);
            if (_homoplasmy.HasValue) jsonObject.AddStringValue("hasHomoplasmy", _homoplasmy.ToString());
            if (_heteroplasmy.HasValue) jsonObject.AddStringValue("hasHeteroplasmy", _heteroplasmy.ToString());
            if (!string.IsNullOrEmpty(_status)) jsonObject.AddStringValue("status", _status);
            if (!string.IsNullOrEmpty(_clinicalSignificance)) jsonObject.AddStringValue("clinicalSignificance", _clinicalSignificance);
            if (!string.IsNullOrEmpty(_scorePercentile)) jsonObject.AddStringValue("scorePercentile", _scorePercentile);
            return sb.ToString();
        }

        public override SupplementaryIntervalItem GetSupplementaryInterval()
        {
            if (!IsInterval || !_intervalEnd.HasValue || !_variantType.HasValue) return null;

            var intValues = new Dictionary<string, int>();
            var doubleValues = new Dictionary<string, double>();
            var freqValues = new Dictionary<string, double>();
            var stringValues = new Dictionary<string, string>();
            var boolValues = new List<string>();

            var suppInterval = new SupplementaryIntervalItem(Chromosome, Start, _intervalEnd.Value, null, _variantType.Value,
                InterimSaCommon.MitoMapSvTag, intValues, doubleValues, freqValues, stringValues, boolValues);
            return suppInterval;
        }


        public Dictionary<(string, string), List<MitoMapItem>> GroupByAltAllele(List<MitoMapItem> mitoMapMutItems)
        {
            var groups = new Dictionary<(string, string), List<MitoMapItem>>();

            foreach (var mitoMapMutItem in mitoMapMutItems)
            {
                var alleleTuple = (mitoMapMutItem.ReferenceAllele, mitoMapMutItem.AlternateAllele);
                if (groups.ContainsKey(alleleTuple))
                    groups[alleleTuple].Add(mitoMapMutItem);
                else groups[alleleTuple] = new List<MitoMapItem> { mitoMapMutItem };
            }
            return groups;
        }
    }
}
