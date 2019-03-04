using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;
using Variants;

namespace SAUtils.DataStructures
{
    public sealed class DgvItem : ISuppIntervalItem
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }

        private string Id { get; }

        private int ObservedGains { get; }

        private int ObservedLosses { get; }

        private int SampleSize { get; }

        private VariantType VariantType { get; }

        private double? VariantFreqAll { get; }

        
        public DgvItem(string id, IChromosome chromosome, int start, int end, int sampleSize, int observedGains, int observedLosses,
            VariantType variantType)
        {
            Id             = id;
            Chromosome     = chromosome;
            Start          = start;
            End            = end;
            SampleSize     = sampleSize;
            ObservedGains  = observedGains;
            ObservedLosses = observedLosses;
            VariantType    = variantType;

            if (SampleSize == 0 || ObservedLosses + ObservedGains == 0) return;
            VariantFreqAll = (ObservedLosses + ObservedGains) / (double)SampleSize;
            VariantFreqAll = VariantFreqAll > 1.0 ? 1.0 : VariantFreqAll;
        }

        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("chromosome", Chromosome.EnsemblName);
            jsonObject.AddIntValue("begin", Start);
            jsonObject.AddIntValue("end", End);
            jsonObject.AddStringValue("variantType", VariantType.ToString());

            jsonObject.AddStringValue("id", Id);
            jsonObject.AddIntValue("sampleSize", SampleSize);
            if (ObservedGains != 0) jsonObject.AddIntValue("observedGains", ObservedGains);
            if (ObservedLosses != 0) jsonObject.AddIntValue("observedLosses", ObservedLosses);
            jsonObject.AddDoubleValue("variantFreqAll", VariantFreqAll, "0.#####");

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public override bool Equals(object obj)
        {
            // If parameter is null return false.

            if (!(obj is DgvItem otherItem)) return false;

            // Return true if the fields match:
            return Equals(Chromosome, otherItem.Chromosome)
                   && Start          == otherItem.Start
                   && End            == otherItem.End
                   && ObservedGains  == otherItem.ObservedGains
                   && SampleSize     == otherItem.SampleSize
                   && ObservedLosses == otherItem.ObservedLosses
                   && string.Equals(Id, otherItem.Id)
                   && Equals(VariantType, otherItem.VariantType)
                   && Equals(VariantFreqAll, otherItem.VariantFreqAll);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Id?.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ (Chromosome?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ Start.GetHashCode();
                hashCode = (hashCode * 397) ^ End.GetHashCode();
                hashCode = (hashCode * 397) ^ VariantType.GetHashCode();
                hashCode = (hashCode * 397) ^ SampleSize.GetHashCode();
                hashCode = (hashCode * 397) ^ ObservedGains.GetHashCode();
                hashCode = (hashCode * 397) ^ ObservedLosses.GetHashCode();
                hashCode = (hashCode * 397) ^ (VariantFreqAll?.GetHashCode() ?? 0);

                return hashCode;
            }
        }

     
    }
}
