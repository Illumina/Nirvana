using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        // ReSharper disable once UnusedMember.Global
        public const int End = 576;
    }

    public static class MitomapParsingParameters
    {
        public const int LargeDeletionCutoff = 100;
        public const string MitomapDiseaseAnnotationFile = "MitomapDisease";
    }

    public sealed class MitoMapItem : SupplementaryDataItem
    {
        private readonly List<string> _diseases;
        private bool? _homoplasmy;
        private bool? _heteroplasmy;
        private readonly string _status;
        private readonly string _clinicalSignificance;
        private readonly string _scorePercentile;
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
            if (_diseases != null && _diseases.Count > 0) jsonObject.AddStringValues("diseases", _diseases.Distinct().ToList());
            if (_homoplasmy.HasValue) jsonObject.AddStringValue("hasHomoplasmy", _homoplasmy.ToString(), false); // output homoplasmy like a bool
            if (_heteroplasmy.HasValue) jsonObject.AddStringValue("hasHeteroplasmy", _heteroplasmy.ToString(), false);  // output heteroplasmy like a bool
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
                    aggregatedMutations[mutation] =  Merge(aggregatedMutations[mutation], mitoMapMutItem);
                else aggregatedMutations[mutation] = mitoMapMutItem;
            }
            return aggregatedMutations;
        }

        private static MitoMapItem Merge(MitoMapItem mitoMapItem1, MitoMapItem mitoMapItem2)
        {
            if (HasConflict(mitoMapItem1.Chromosome, mitoMapItem2.Chromosome) || HasConflict(mitoMapItem1.Start, mitoMapItem2.Start) ||
                HasConflict(mitoMapItem1.ReferenceAllele, mitoMapItem2.ReferenceAllele) || HasConflict(mitoMapItem1.AlternateAllele, mitoMapItem2.AlternateAllele) || HasConflict(mitoMapItem1._homoplasmy, mitoMapItem2._homoplasmy) || HasConflict(mitoMapItem1._heteroplasmy, mitoMapItem2._heteroplasmy) || HasConflict(mitoMapItem1._status, mitoMapItem2._status) || HasConflict(mitoMapItem1._clinicalSignificance, mitoMapItem2._clinicalSignificance) || HasConflict(mitoMapItem1._scorePercentile, mitoMapItem2._scorePercentile) || HasConflict(mitoMapItem1.IsInterval, mitoMapItem2.IsInterval) || HasConflict(mitoMapItem1._intervalEnd, mitoMapItem2._intervalEnd) || HasConflict(mitoMapItem1._variantType, mitoMapItem2._variantType))
            {
                throw new InvalidDataException($"Conflict found at {mitoMapItem1.Start} when updating MITOMAP record: first record: {mitoMapItem1.GetVariantJsonString()}; second record: {mitoMapItem2.GetVariantJsonString()} ");
            }
            var homoplasmy = mitoMapItem1._homoplasmy ?? mitoMapItem2._homoplasmy;
            var heteroplasmy = mitoMapItem1._heteroplasmy ?? mitoMapItem2._heteroplasmy;
            List<string> diseases;
            if (mitoMapItem1._diseases != null && mitoMapItem2._diseases != null)
            {
                Console.WriteLine($"Merge diseases at {mitoMapItem1.Start}, {mitoMapItem1.ReferenceAllele}-{mitoMapItem1.AlternateAllele}: {string.Join(",", mitoMapItem1._diseases)} and {string.Join(",",mitoMapItem2._diseases)}");
                diseases = mitoMapItem1._diseases.Concat(mitoMapItem2._diseases).Distinct().ToList();
            }
            else
            {
                diseases = (mitoMapItem1._diseases?.Count > 0) ? mitoMapItem1._diseases : mitoMapItem2._diseases;
            }
            var status = mitoMapItem1._status ?? mitoMapItem2._status;
            var clinicalSignificance = mitoMapItem1._clinicalSignificance ?? mitoMapItem2._clinicalSignificance;
            var scorePercentile = mitoMapItem1._scorePercentile ?? mitoMapItem2._scorePercentile;
            var isInterval = mitoMapItem1.IsInterval;
            var intervalEnd = mitoMapItem1._intervalEnd ?? mitoMapItem2._intervalEnd;
            var variantType = mitoMapItem1._variantType ?? mitoMapItem2._variantType;
            return new MitoMapItem(mitoMapItem1.Start, mitoMapItem1.ReferenceAllele, mitoMapItem1.AlternateAllele,
                diseases, homoplasmy, heteroplasmy, status, clinicalSignificance, scorePercentile, isInterval,
                intervalEnd, variantType);
        }

        private static bool HasConflict<T>(T originalValue, T newValue)
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
