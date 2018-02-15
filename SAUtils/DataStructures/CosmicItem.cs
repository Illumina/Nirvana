using System;
using System.Collections.Generic;
using System.Text;
using CommonUtilities;
using VariantAnnotation.Interface.IO;
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
            Chromosome = chromosome;
            Start = start;
            Id = id;
            ReferenceAllele = refAllele;
            AlternateAllele = altAllele;
            Gene = gene;

            Studies = studies;
            SampleCount = sampleCount;

        }

        public sealed class CosmicStudy : IEquatable<CosmicStudy>, IJsonSerializer
        {
            #region members

            public string Id { get; }
            public string Histology { get; }
            public string PrimarySite { get; }

            #endregion

            public CosmicStudy(string studyId, string histology, string primarySite)
            {
                Id = studyId;
                Histology = histology;
                PrimarySite = primarySite;
            }

            public bool Equals(CosmicStudy other)
            {
                return Id.Equals(other?.Id) &&
                       Histology.Equals(other?.Histology) &&
                       PrimarySite.Equals(other?.PrimarySite);
            }

            public override int GetHashCode()
            {
                var hashCode = Id?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Histology?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (PrimarySite?.GetHashCode() ?? 0);
                return hashCode;
            }


            public void SerializeJson(StringBuilder sb)
            {
                var jsonObject = new JsonObject(sb);

                sb.Append(JsonObject.OpenBrace);
                if (!string.IsNullOrEmpty(Id)) jsonObject.AddStringValue("id", Id, false);
                jsonObject.AddStringValue("histology", Histology?.Replace('_', ' '));
                jsonObject.AddStringValue("primarySite", PrimarySite?.Replace('_', ' '));
                sb.Append(JsonObject.CloseBrace);
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
            jsonObject.AddObjectValues("studies", Studies);

            return StringBuilderCache.GetStringAndRelease(sb);
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


