using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using Variants;

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
    }

    public sealed class MitoMapItem : ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }
        public bool IsInterval;

        private readonly List<string> _diseases;
        private readonly bool? _homoplasmy;
        private readonly bool? _heteroplasmy;
        private readonly string _status;
        private readonly string _clinicalSignificance;
        private readonly string _scorePercentile;
        private readonly int? _intervalEnd;
        private readonly VariantType? _variantType;

        public MitoMapItem(IChromosome chromosome, int posi, string refAllele, string altAllele, List<string> diseases, bool? homoplasmy, bool? heteroplasmy, string status, string clinicalSignificance, string scorePercentile, bool isInterval, int? intervalEnd, VariantType? variantType, ISequenceProvider sequenceProvider)
        {
            Chromosome = chromosome;
            Position = posi;
            if (sequenceProvider == null)
            {
                RefAllele = refAllele;
                AltAllele = altAllele;
            }
            else
            {
                (Position, RefAllele, AltAllele) = TryAddPaddingBase(refAllele, altAllele, Position, sequenceProvider);
            }
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

        private static (int, string, string) TryAddPaddingBase(string refAllele, string altAllele, int position, ISequenceProvider sequenceProvider)
        {
            // insertion
            if (IsEmptyOrDash(refAllele)) return AddPaddingBase(altAllele, true, position, sequenceProvider);
            // deletion
            return IsEmptyOrDash(altAllele) ? AddPaddingBase(refAllele, false, position, sequenceProvider) : (position, refAllele, altAllele);
        }

        private static (int, string, string) AddPaddingBase(string allele, bool isInsertion, int position, ISequenceProvider sequenceProvider)
        {
            string paddingBase = sequenceProvider.Sequence.Substring(position - 2, 1);
            return isInsertion ? (position - 1, paddingBase, paddingBase + allele) : (position - 1, paddingBase + allele, paddingBase);
        }

        private static bool IsEmptyOrDash(string allele) => string.IsNullOrEmpty(allele) || allele == "-";

        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            if (string.IsNullOrEmpty(RefAllele)) RefAllele = "-";
            if (string.IsNullOrEmpty(AltAllele)) AltAllele = "-";

            jsonObject.AddStringValue("refAllele", RefAllele);
            jsonObject.AddStringValue("altAllele", AltAllele);
            if (_diseases != null && _diseases.Count > 0) jsonObject.AddStringValues("diseases", _diseases.Distinct().ToList());
            if (_homoplasmy.HasValue) jsonObject.AddBoolValue("hasHomoplasmy", _homoplasmy.Value, true); 
            if (_heteroplasmy.HasValue) jsonObject.AddBoolValue("hasHeteroplasmy", _heteroplasmy.Value, true);  
            if (!string.IsNullOrEmpty(_status)) jsonObject.AddStringValue("status", _status);
            if (!string.IsNullOrEmpty(_clinicalSignificance)) jsonObject.AddStringValue("clinicalSignificance", _clinicalSignificance);
            if (!string.IsNullOrEmpty(_scorePercentile)) jsonObject.AddStringValue("scorePercentile", _scorePercentile, false);
            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public static Dictionary<(string, string), MitoMapItem> AggregatedMutationsSamePosition(IEnumerable<MitoMapItem> mitoMapMutItems)
        {
            var aggregatedMutations = new Dictionary<(string, string), MitoMapItem>();

            foreach (var mitoMapMutItem in mitoMapMutItems)
            {
                var mutation = (mitoMapMutItem.RefAllele, mitoMapMutItem.AltAllele);
                if (aggregatedMutations.ContainsKey(mutation))
                {
                    var mergedItem = Merge(aggregatedMutations[mutation], mitoMapMutItem);
                    if (mergedItem == null) continue;
                    aggregatedMutations[mutation] = mergedItem;
                }
                else aggregatedMutations[mutation] = mitoMapMutItem;
            }
            return aggregatedMutations;
        }

        private static MitoMapItem Merge(MitoMapItem mitoMapItem1, MitoMapItem mitoMapItem2)
        {
            if (HasConflict(mitoMapItem1.Chromosome, mitoMapItem2.Chromosome) || HasConflict(mitoMapItem1.Position, mitoMapItem2.Position) ||
                HasConflict(mitoMapItem1.RefAllele, mitoMapItem2.RefAllele) || HasConflict(mitoMapItem1.AltAllele, mitoMapItem2.AltAllele) || HasConflict(mitoMapItem1._homoplasmy, mitoMapItem2._homoplasmy) || HasConflict(mitoMapItem1._heteroplasmy, mitoMapItem2._heteroplasmy) || HasConflict(mitoMapItem1._status, mitoMapItem2._status) || HasConflict(mitoMapItem1._clinicalSignificance, mitoMapItem2._clinicalSignificance) || HasConflict(mitoMapItem1._scorePercentile, mitoMapItem2._scorePercentile) //|| HasConflict(mitoMapItem1.IsInterval, mitoMapItem2.IsInterval) 
                || HasConflict(mitoMapItem1._intervalEnd, mitoMapItem2._intervalEnd) || HasConflict(mitoMapItem1._variantType, mitoMapItem2._variantType))
            {
                throw new InvalidDataException($"Conflict found at {mitoMapItem1.Position} when updating MITOMAP record: first record: {mitoMapItem1.GetJsonString()}; second record: {mitoMapItem2.GetJsonString()} ");
                //Console.WriteLine($"Conflict found at {mitoMapItem1.Position} when updating MITOMAP record: first record: {mitoMapItem1.GetJsonString()}; second record: {mitoMapItem2.GetJsonString()} ");
                //return null;
            }
            var homoplasmy = mitoMapItem1._homoplasmy ?? mitoMapItem2._homoplasmy;
            var heteroplasmy = mitoMapItem1._heteroplasmy ?? mitoMapItem2._heteroplasmy;
            List<string> diseases;
            if (mitoMapItem1._diseases != null && mitoMapItem2._diseases != null)
            {
                Console.WriteLine($"Merge diseases at {mitoMapItem1.Position}, {mitoMapItem1.RefAllele}-{mitoMapItem1.AltAllele}: {string.Join(",", mitoMapItem1._diseases)} and {string.Join(",",mitoMapItem2._diseases)}");
                diseases = mitoMapItem1._diseases.Concat(mitoMapItem2._diseases).Distinct().ToList();
            }
            else
            {
                diseases = mitoMapItem1._diseases?.Count > 0 ? mitoMapItem1._diseases : mitoMapItem2._diseases;
            }
            var status = mitoMapItem1._status ?? mitoMapItem2._status;
            var clinicalSignificance = mitoMapItem1._clinicalSignificance ?? mitoMapItem2._clinicalSignificance;
            var scorePercentile = mitoMapItem1._scorePercentile ?? mitoMapItem2._scorePercentile;
            var isInterval = mitoMapItem1.IsInterval;
            var intervalEnd = mitoMapItem1._intervalEnd ?? mitoMapItem2._intervalEnd;
            var variantType = mitoMapItem1._variantType ?? mitoMapItem2._variantType;
            return new MitoMapItem(mitoMapItem1.Chromosome, mitoMapItem1.Position, mitoMapItem1.RefAllele, mitoMapItem1.AltAllele,
                diseases, homoplasmy, heteroplasmy, status, clinicalSignificance, scorePercentile, isInterval,
                intervalEnd, variantType, null);
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

        public MitoMapSvItem ToMitoMapSvItem()
        {
            if (_intervalEnd == null || _variantType == null) throw new InvalidDataException($"Not an interval at {Position}:{GetJsonString()}");

            return new MitoMapSvItem(Chromosome, Position, _intervalEnd.Value, _variantType.Value, GetJsonString());
        }
    }
}
