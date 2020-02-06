using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.MitoMap
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
        
        private readonly List<string> _diseases;
        private readonly bool? _homoplasmy;
        private readonly bool? _heteroplasmy;
        private readonly string _status;
        private readonly string _clinicalSignificance;
        private readonly string _scorePercentile;
        private readonly int _numGenBankFullLengthSeqs;
        private readonly List<string> _pubMedIds;

        public MitoMapItem(IChromosome chromosome, int posi, string refAllele, string altAllele, List<string> diseases, bool? homoplasmy, bool? heteroplasmy, string status, string clinicalSignificance, string scorePercentile, ISequenceProvider sequenceProvider, int numGenBankFullLengthSeqs, List<string> pubMedIds)
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
            _diseases = diseases;
            _homoplasmy = homoplasmy;
            _heteroplasmy = heteroplasmy;
            _status = status;
            _clinicalSignificance = clinicalSignificance;
            _scorePercentile = scorePercentile;
            _numGenBankFullLengthSeqs = numGenBankFullLengthSeqs;
            _pubMedIds = pubMedIds;
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
            jsonObject.AddStringValues("diseases", _diseases?.Distinct());
            if (_homoplasmy.HasValue) jsonObject.AddBoolValue("hasHomoplasmy", _homoplasmy.Value, true); 
            if (_heteroplasmy.HasValue) jsonObject.AddBoolValue("hasHeteroplasmy", _heteroplasmy.Value, true);  
            jsonObject.AddStringValue("status", _status);
            jsonObject.AddStringValue("clinicalSignificance", _clinicalSignificance);
            jsonObject.AddStringValue("scorePercentile", _scorePercentile, false);
            jsonObject.AddIntValue("numGenBankFullLengthSeqs", _numGenBankFullLengthSeqs);
            jsonObject.AddStringValues("pubMedIds", _pubMedIds);
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
            if (HasConflictValue(mitoMapItem1.Chromosome, mitoMapItem2.Chromosome) || HasConflictValue(mitoMapItem1.Position, mitoMapItem2.Position) ||
                HasConflictValue(mitoMapItem1.RefAllele, mitoMapItem2.RefAllele) || HasConflictValue(mitoMapItem1.AltAllele, mitoMapItem2.AltAllele) || 
                HasConflictValue(mitoMapItem1._homoplasmy, mitoMapItem2._homoplasmy) || HasConflictValue(mitoMapItem1._heteroplasmy, mitoMapItem2._heteroplasmy) || 
                HasConflictValue(mitoMapItem1._status, mitoMapItem2._status) || HasConflictValue(mitoMapItem1._clinicalSignificance, mitoMapItem2._clinicalSignificance) ||
                HasConflictValue(mitoMapItem1._scorePercentile, mitoMapItem2._scorePercentile))
            {
                throw new InvalidDataException($"Conflict found at {mitoMapItem1.Position} when updating MITOMAP record: first record: {mitoMapItem1.GetJsonString()}; second record: {mitoMapItem2.GetJsonString()} ");
            }
            var homoplasmy = mitoMapItem1._homoplasmy ?? mitoMapItem2._homoplasmy;
            var heteroplasmy = mitoMapItem1._heteroplasmy ?? mitoMapItem2._heteroplasmy;
            string alleleInfo = $"{mitoMapItem1.Position} (Ref: {mitoMapItem1.RefAllele}, Alt: {mitoMapItem1.AltAllele})";
            var diseases = MergeCollections(mitoMapItem1._diseases, mitoMapItem2._diseases, alleleInfo).ToList();
            var pubMedIds = MergeCollections(mitoMapItem1._pubMedIds, mitoMapItem2._pubMedIds, alleleInfo).ToList();
            var status = mitoMapItem1._status ?? mitoMapItem2._status;
            var clinicalSignificance = mitoMapItem1._clinicalSignificance ?? mitoMapItem2._clinicalSignificance;
            var scorePercentile = mitoMapItem1._scorePercentile ?? mitoMapItem2._scorePercentile;
            var numFullLengthSequences = Math.Max(mitoMapItem1._numGenBankFullLengthSeqs, mitoMapItem2._numGenBankFullLengthSeqs);
            return new MitoMapItem(mitoMapItem1.Chromosome, mitoMapItem1.Position, mitoMapItem1.RefAllele, mitoMapItem1.AltAllele,
                diseases, homoplasmy, heteroplasmy, status, clinicalSignificance, scorePercentile, null, numFullLengthSequences, pubMedIds);
        }

        private static IEnumerable<string> MergeCollections(ICollection<string> collection1, ICollection<string> collection2, string alleleInfo)
        {
            if (IsNullOrEmpty(collection1) || IsNullOrEmpty(collection2)) 
                return (collection1?.Count ?? -1) > 0 
                ? collection1 
                : collection2 ?? Enumerable.Empty<string>();
            
            Console.WriteLine($"Merge data at {alleleInfo}: {string.Join(",", collection1)} and {string.Join(",", collection2)}");
            return collection1.Concat(collection1).Distinct();

        }

        private static bool HasConflictValue<T>(T originalValue, T newValue)
        {
            bool hasConflict = !IsNullOrEmpty(originalValue) && !IsNullOrEmpty(newValue) && !originalValue.Equals(newValue);
            if (hasConflict) Console.WriteLine($"Conflict found: {originalValue}, {newValue}");

            return hasConflict;
        }

        private static bool IsNullOrEmpty<T>(T value)
        {
            if (typeof(T) == typeof(string))
                return string.IsNullOrEmpty(value as string);
            return value == null || value.Equals(default(T));
        }
    }
}
