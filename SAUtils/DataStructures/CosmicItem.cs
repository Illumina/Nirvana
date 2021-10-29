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
            HashSet<CosmicTumor> tumors,
            int? sampleCount)
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
            public string Histology { get; }
            public string Site { get; }
            public string Tier { get; }

            #endregion

            public CosmicTumor(string tumorId,
                               string histology,
                               string site,
                               string tier)
            {
                Id         = tumorId;
                Site       = site;
                Histology  = histology;
                Tier       = tier;
            }

            public bool Equals(CosmicTumor other)
            {
                if (other == null) return false;

                return Id.Equals(other.Id)
                    && StringsEqual(Histology, other.Histology)
                    && StringsEqual(Site, other.Site)
                    && StringsEqual(Tier, other.Tier);
            }

            private static bool StringsEqual(string s1, string s2)
            {
                if (s1 == null && s2 != null) return false;
                if (s1 != null && s2 == null) return false;
                if (s1 == null && s2 == null) return true;
                return s1.Equals(s2);
            }

            public override int GetHashCode()
            {
                var hashCode = Id?.GetHashCode() ?? 0;
                //hashCode ^= Histology.GetHashCode() ^ Site.GetHashCode();
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

            jsonObject.AddStringValue("cancerTypesAndCounts", GetJsonStringFromDict("cancerType", GetCancerTypeCounts()), false);
            jsonObject.AddStringValue("cancerSitesAndCounts", GetJsonStringFromDict("cancerSite", GetTissueCounts()), false);
            jsonObject.AddStringValue("tiersAndCounts", GetJsonStringFromDict("tier", GetTierCounts()), false);

            return StringBuilderPool.GetStringAndReturn(sb);
        }

        internal IDictionary<string,int> GetTissueCounts()
        {
            if (Tumors == null) return null;
            var tissueCounts = new Dictionary<string, int>();
            foreach (var tumor in Tumors)
            {
                if (string.IsNullOrEmpty(tumor.Site)) continue;

                if (tissueCounts.TryGetValue(tumor.Site, out _))
                {
                    tissueCounts[tumor.Site]++;
                }
                else tissueCounts[tumor.Site] = 1;
            }

            return tissueCounts; 
        }

        internal IDictionary<string,int> GetCancerTypeCounts()
        {
            if (Tumors == null) return null;
            var histologyCounts = new Dictionary<string, int>();
            foreach (var tumor in Tumors)
            {
                if (string.IsNullOrEmpty(tumor.Histology)) continue;

                if (histologyCounts.TryGetValue(tumor.Histology, out _))
                {
                    histologyCounts[tumor.Histology]++;
                }
                else histologyCounts[tumor.Histology] = 1;
            }

            return histologyCounts; 
        }

        internal IDictionary<string,int> GetTierCounts()
        {
            if (Tumors == null) return null;
            var tierCounts = new Dictionary<string, int>();
            foreach (var tumor in Tumors)
            {
                if (string.IsNullOrEmpty(tumor.Tier)) continue;

                if (tierCounts.TryGetValue(tumor.Tier, out _))
                {
                    tierCounts[tumor.Tier]++;
                }
                else tierCounts[tumor.Tier] = 1;
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


