using System;
using System.Collections.Generic;
using CommonUtilities;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class CosmicItem : SupplementaryDataItem, IEquatable<CosmicItem>
    {
        #region members

        public string Id { get; }
        private string Gene { get; }
        private int? SampleCount { get; }


        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private string IsAlleleSpecific { get; set; }

        public HashSet<CosmicStudy> Studies { get; private set; }

        #endregion

        public CosmicItem(
            IChromosome chromosome,
            int start,
            string id,
            string refAllele,
            string altAllele,
            string gene,
            HashSet<CosmicStudy> studies, int? sampleCount)
        {
            Chromosome      = chromosome;
            Start           = start;
            Id              = id;
            ReferenceAllele = refAllele;
            AlternateAllele = altAllele;
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
                return Id.Equals(other?.Id);
            }

            public override int GetHashCode()
            {
                var hashCode = Id?.GetHashCode() ?? 0;
                return hashCode;
            }
        }



        public override SupplementaryIntervalItem GetSupplementaryInterval()
        {
            throw new NotImplementedException();
        }


        public bool Equals(CosmicItem otherItem)
        {
            // If parameter is null return false.
            if (otherItem == null) return false;

            // Return true if the fields match:
            return Equals(Chromosome, otherItem.Chromosome) &&
                   Start == otherItem.Start &&
                   string.Equals(Id, otherItem.Id) &&
                   string.Equals(ReferenceAllele, otherItem.ReferenceAllele) &&
                   string.Equals(AlternateAllele, otherItem.AlternateAllele) &&
                   string.Equals(Gene, otherItem.Gene);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Chromosome?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ Start;
                hashCode = (hashCode * 397) ^ (Id?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (ReferenceAllele?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (AlternateAllele?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (Gene?.GetHashCode() ?? 0);

                return hashCode;
            }
        }




        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();

            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("id", Id);
            jsonObject.AddStringValue("isAlleleSpecific", IsAlleleSpecific, false);
            jsonObject.AddStringValue("refAllele", string.IsNullOrEmpty(ReferenceAllele) ? "-" : ReferenceAllele);
            jsonObject.AddStringValue("altAllele",
                SupplementaryAnnotationUtilities.ReverseSaReducedAllele(AlternateAllele));
            jsonObject.AddStringValue("gene", Gene);
            jsonObject.AddIntValue("sampleCount", SampleCount);

            jsonObject.AddStringValues("cancerTypes", GetJsonStrings(GetCancerTypeCounts()), false);
            jsonObject.AddStringValues("tissues", GetJsonStrings(GetTissueCounts()), false);

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
                    if (tissueCounts.TryGetValue(site, out var _))
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
                    if (cancerTypeCounts.TryGetValue(histology, out var _))
                    {
                        cancerTypeCounts[histology]++;
                    }
                    else cancerTypeCounts[histology] = 1;
                }
            }

            return cancerTypeCounts;
        }

        private static IEnumerable<string> GetJsonStrings(IDictionary<string, int> dictionary)
        {
            if (dictionary == null) yield break;
            foreach (var kvp in dictionary)
            {
                yield return $"{{\"{kvp.Key.Replace('_', ' ')}\":{kvp.Value}}}";
            }
        }

        public void MergeStudies(CosmicItem otherItem)
        {
            if (Studies == null)
                Studies = otherItem.Studies;
            else
            {
                foreach (var study in otherItem.Studies)
                {
                    Studies.Add(study);
                }
            }
        }
    }
}


