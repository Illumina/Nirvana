using System;
using System.Collections.Generic;
using System.Linq;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class CosmicItem : ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }

        private string Id { get; }
        private string Gene { get; }
        private int? SampleCount { get; }
        public HashSet<CosmicTumor> Tumors { get; }

        public CosmicItem(
            IChromosome chromosome,
            int position,
            string id,
            string refAllele,
            string altAllele,
            string gene,
            HashSet<CosmicTumor> tumors, int? sampleCount)
        {
            Chromosome      = chromosome;
            Position        = position;
            Id              = id;
            RefAllele       = refAllele;
            AltAllele       = altAllele;
            Gene            = gene;
            Tumors          = tumors;
            SampleCount     = sampleCount;
        }

        public sealed class CosmicTumor : IEquatable<CosmicTumor>
        {
            #region members

            public string Id { get; }
            public IEnumerable<string> Histologies { get; }
            public IEnumerable<string> Sites { get; }
            public IEnumerable<string> Tiers { get; }

            #endregion

            public CosmicTumor(string tumorId,
                               IEnumerable<string> histologies,
                               IEnumerable<string> sites,
                               IEnumerable<string> tiers)
            {
                Id          = tumorId;
                Sites       = sites;
                Histologies = histologies;
                Tiers       = tiers;
            }

            public bool Equals(CosmicTumor other)
            {
                if (other == null) return false;
                return Id.Equals(other.Id)
                    && Histologies.SequenceEqual(other.Histologies)
                    && Sites.SequenceEqual(other.Sites)
                    && Tiers.SequenceEqual(other.Tiers);
            }

            public override int GetHashCode()
            {
                var hashCode = Id?.GetHashCode() ?? 0;
                //hashCode ^= Histologies.GetHashCode() ^ Sites.GetHashCode();
                return hashCode;
            }
        }

        public string GetJsonString()
        {
            var sb = StringBuilderPool.Get();

            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("id", Id);
            jsonObject.AddStringValue("refAllele", string.IsNullOrEmpty(RefAllele) ? "-" : RefAllele);
            jsonObject.AddStringValue("altAllele", SaUtilsCommon.ReverseSaReducedAllele(AltAllele));
            jsonObject.AddStringValue("gene", Gene);
            jsonObject.AddIntValue("sampleCount", SampleCount);

            jsonObject.AddStringValue("cancerTypesAndCounts", GetJsonStringFromDict("cancerType",GetCancerTypeCounts()), false);
            jsonObject.AddStringValue("cancerSitesAndCounts", GetJsonStringFromDict("cancerSite",GetTissueCounts()), false);
            jsonObject.AddStringValue("tiersAndCounts", GetJsonStringFromDict("tier",GetTierCounts()), false);

            return StringBuilderPool.GetStringAndReturn(sb);
        }

        internal IDictionary<string,int> GetTissueCounts()
        {
            if (Tumors == null) return null;
            var tissueCounts = new Dictionary<string, int>();
            foreach (var tumor in Tumors)
            {
                if (tumor.Sites == null) continue;

                foreach (var site in tumor.Sites)
                {
                    if (tissueCounts.TryGetValue(site, out _))
                    {
                        tissueCounts[site]++;
                    }
                    else tissueCounts[site] = 1;
                }
            }

            return tissueCounts; 
        }

        internal IDictionary<string,int> GetCancerTypeCounts()
        {
            if (Tumors == null) return null;
            var cancerTypeCounts = new Dictionary<string, int>();
            foreach (var tumor in Tumors)
            {
                if (tumor.Histologies == null) continue;

                foreach (var histology in tumor.Histologies)
                {
                    if (cancerTypeCounts.TryGetValue(histology, out _))
                    {
                        cancerTypeCounts[histology]++;
                    }
                    else cancerTypeCounts[histology] = 1;
                }
            }

            return cancerTypeCounts;
        }

        internal IDictionary<string,int> GetTierCounts()
        {
            if (Tumors == null) return null;
            var tierCounts = new Dictionary<string, int>();
            foreach (var tumor in Tumors)
            {
                if (tumor.Tiers == null) continue;

                foreach (var tier in tumor.Tiers)
                {
                    if (tierCounts.TryGetValue(tier, out _))
                    {
                        tierCounts[tier]++;
                    }
                    else tierCounts[tier] = 1;
                }
            }

            return tierCounts;
        }

        private static string GetJsonStringFromDict(string dataType, IDictionary<string, int> dictionary)
        {
            if (dictionary == null) return null;

            var sb = StringBuilderPool.Get();
            sb.Append(JsonObject.OpenBracket);

            bool isFirstItem = true;
            foreach (var kvp in dictionary)
            {
                if (!isFirstItem)
                    sb.Append(JsonObject.Comma);

                sb.Append(JsonObject.OpenBrace);
                sb.Append($"\"{dataType}\":\"{kvp.Key}\",");
                sb.Append($"\"count\":{kvp.Value}");
                sb.Append(JsonObject.CloseBrace);
                
                isFirstItem = false;
            }

            sb.Append(JsonObject.CloseBracket);

            return StringBuilderPool.GetStringAndReturn(sb);
        }

       
    }
}


