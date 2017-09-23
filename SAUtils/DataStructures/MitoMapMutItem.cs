using System;
using System.Collections.Generic;
using System.Text;
using CommonUtilities;
using VariantAnnotation.Interface.Sequence;
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

    public sealed class MitoMapMutItem : SupplementaryDataItem
    {
        private readonly string _disease;
        private bool? _homoplasmy;
        private bool? _heteroplasmy;
        private readonly string _status;
        private readonly string _clinicalSignificance;
        private readonly string _scorePercentile;

        public MitoMapMutItem(int posi, string refAllele, string altAllele, string disease, bool? homoplasmy, bool? heteroplasmy, string status, string clinicalSignificance, string scorePercentile)
        {
            Chromosome = new Chromosome("chrM", "MT", 24);
            Start = posi;
            ReferenceAllele = refAllele;
            AlternateAllele = altAllele;
            IsInterval = false;
            _disease = disease;
            _homoplasmy = homoplasmy;
            _heteroplasmy = heteroplasmy;
            _status = status;
            _clinicalSignificance = clinicalSignificance;
            _scorePercentile = scorePercentile;
        }

        public string GetJsonString()
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
            throw new System.NotImplementedException();
        }


        public Dictionary<(string, string), List<MitoMapMutItem>> GroupByAltAllele(List<MitoMapMutItem> mitoMapMutItems)
        {
            var groups = new Dictionary<(string, string), List<MitoMapMutItem>>();

            foreach (var mitoMapMutItem in mitoMapMutItems)
            {
                var alleleTuple = (mitoMapMutItem.ReferenceAllele, mitoMapMutItem.AlternateAllele);
                if (groups.ContainsKey(alleleTuple))
                    groups[alleleTuple].Add(mitoMapMutItem);
                else groups[alleleTuple] = new List<MitoMapMutItem> { mitoMapMutItem };
            }

            return groups;
        }

        /*
        private List<MitoMapMutItem> CombineFiles(List<MitoMapMutItem> mitoMapMutItems)
        {
            var mitoMapMutcDict = new Dictionary<string, MitoMapMutItem>();

            foreach (var mitoMapItem in mitoMapMutItems)
            {
                if (mitoMapMutcDict.ContainsKey(cosmicItem.ID))
                {
                    cosmicDict[cosmicItem.ID].MergeStudies(cosmicItem);
                }
                else cosmicDict[cosmicItem.ID] = cosmicItem;
            }

            return cosmicDict.Values.ToList();
        }
        */
    }
}
