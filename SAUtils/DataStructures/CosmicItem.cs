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
        public HashSet<CosmicStudy> Studies { get; }

        public CosmicItem(
            IChromosome chromosome,
            int position,
            string id,
            string refAllele,
            string altAllele,
            string gene,
            HashSet<CosmicStudy> studies, int? sampleCount)
        {
            Chromosome      = chromosome;
            Position        = position;
            Id              = id;
            RefAllele = refAllele;
            AltAllele = altAllele;
            Gene            = gene;
            Studies         = studies;
            SampleCount     = sampleCount;

        }

        public sealed class CosmicStudy : IEquatable<CosmicStudy>
        {
            #region members

            public string Id { get; }
            public IEnumerable<string> Histologies { get; }
            public IEnumerable<string> Sites { get; }

            #endregion

            public CosmicStudy(string studyId, IEnumerable<string> histologies, IEnumerable<string> sites)
            {
                Id          = studyId;
                Sites       = sites;
                Histologies = histologies;
            }

            public bool Equals(CosmicStudy other)
            {
                if (other == null) return false;
                return Id.Equals(other.Id)
                    && Histologies.SequenceEqual(other.Histologies)
                    && Sites.SequenceEqual(other.Sites);
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
            var sb = StringBuilderCache.Acquire();

            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("id", Id);
            jsonObject.AddStringValue("refAllele", string.IsNullOrEmpty(RefAllele) ? "-" : RefAllele);
            jsonObject.AddStringValue("altAllele", SaUtilsCommon.ReverseSaReducedAllele(AltAllele));
            jsonObject.AddStringValue("gene", Gene);
            jsonObject.AddIntValue("sampleCount", SampleCount);

            jsonObject.AddStringValue("cancerTypesAndCounts", GetJsonStringFromDict("cancerType",GetCancerTypeCounts()), false);
            jsonObject.AddStringValue("cancerSitesAndCounts", GetJsonStringFromDict("cancerSite",GetTissueCounts()), false);

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        internal IDictionary<string,int> GetTissueCounts()
        {
            if (Studies == null) return null;
            var tissueCounts = new Dictionary<string, int>();
            foreach (var study in Studies)
            {
                if (study.Sites == null) return null;

                foreach (var site in study.Sites)
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
            if (Studies == null) return null;
            var cancerTypeCounts = new Dictionary<string, int>();
            foreach (var study in Studies)
            {
                if (study.Histologies == null) return null;
                foreach (var histology in study.Histologies)
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

        private static string GetJsonStringFromDict(string dataType, IDictionary<string, int> dictionary)
        {
            if (dictionary == null) return null;

            var sb = StringBuilderCache.Acquire();
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

            return StringBuilderCache.GetStringAndRelease(sb);
        }

       
    }
}


