using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
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
        private List<string> _diseases;
        private bool? _homoplasmy;
        private bool? _heteroplasmy;
        private string _status;
        private string _clinicalSignificance;
        private string _scorePercentile;
        private int? _intervalEnd;
        private VariantType? _variantType;
        private static readonly Chromosome ChromM = new Chromosome("chrM", "MT", 24);


        public MitoMapItem(int posi, string refAllele, string altAllele, List<string> diseases, bool? homoplasmy, bool? heteroplasmy, string status, string clinicalSignificance, string scorePercentile, bool isInterval, int? intervalEnd, VariantType? variantType)
        {
            Chromosome = ChromM;
            Start = posi;
            ReferenceAllele = refAllele;
            AlternateAllele = altAllele;
            IsInterval = isInterval;
            _diseases = diseases;
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
            if (_diseases.Count > 0) jsonObject.AddStringValue("disease", string.Join(";", _diseases));
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
                InterimSaCommon.MitoMapTag, intValues, doubleValues, freqValues, stringValues, boolValues);
            return suppInterval;
        }

        public static Dictionary<(string, string), MitoMapItem> AggregatedMutationsSomePosition(List<MitoMapItem> mitoMapMutItems)
        {
            var aggregatedMutations = new Dictionary<(string, string), MitoMapItem>();

            foreach (var mitoMapMutItem in mitoMapMutItems)
            {
                var mutation = (mitoMapMutItem.ReferenceAllele, mitoMapMutItem.AlternateAllele);
                if (aggregatedMutations.ContainsKey(mutation))
                    aggregatedMutations[mutation].Update(mitoMapMutItem);
                else aggregatedMutations[mutation] = mitoMapMutItem;
            }
            return aggregatedMutations;
        }

        private void Update(MitoMapItem newMitoMapItem)
        {
            if (HasConflict(Chromosome, newMitoMapItem.Chromosome) || HasConflict(Start, newMitoMapItem.Start) ||
                HasConflict(ReferenceAllele, newMitoMapItem.ReferenceAllele) || HasConflict(AlternateAllele, newMitoMapItem.AlternateAllele) || HasConflict(_homoplasmy, newMitoMapItem._homoplasmy) || HasConflict(_heteroplasmy, newMitoMapItem._heteroplasmy) || HasConflict(_diseases.ToString(), newMitoMapItem._diseases.ToString()) || HasConflict(_status, newMitoMapItem._status) || HasConflict(_clinicalSignificance, newMitoMapItem._clinicalSignificance) || HasConflict(_scorePercentile, newMitoMapItem._scorePercentile) || HasConflict(_intervalEnd, newMitoMapItem._intervalEnd) || HasConflict(_variantType, newMitoMapItem._variantType))
            {
                throw new InvalidDataException($"Conflict found at {Start} when updating MITOMAP record: original record: {GetVariantJsonString()}; new record: {newMitoMapItem.GetVariantJsonString()} ");
            }
            _homoplasmy = _homoplasmy ?? newMitoMapItem._homoplasmy;
            _heteroplasmy = _heteroplasmy ?? newMitoMapItem._heteroplasmy;
            _diseases = _diseases ?? newMitoMapItem._diseases;
            _status = _status ?? newMitoMapItem._status;
            _clinicalSignificance = _clinicalSignificance ?? newMitoMapItem._clinicalSignificance;
            _scorePercentile = _scorePercentile ?? newMitoMapItem._scorePercentile;
            _intervalEnd = _intervalEnd ?? newMitoMapItem._intervalEnd;
            _variantType = _variantType ?? newMitoMapItem._variantType;
        }

        private bool HasConflict<T>(T originalValue, T newValue)
        {
            return !IsNullOrEmpty(originalValue) && !IsNullOrEmpty(newValue) && !originalValue.Equals(newValue);
        }

        private static bool IsNullOrEmpty<T>(T value)
        {
            if (typeof(T) == typeof(string))
                return string.IsNullOrEmpty(value as string);
            return value == null || value.Equals(default(T));
        }
    }
}
