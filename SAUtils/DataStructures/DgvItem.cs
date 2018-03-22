using System.Collections.Generic;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.DataStructures
{
    public sealed class DgvItem : SupplementaryDataItem
    {
        private string Id { get; }

        private int ObservedGains { get; }

        private int ObservedLosses { get; }

        private int SampleSize { get; }

        private VariantType VariantType { get; }

        private double? VariantFreqAll { get; }

        private int End { get; }

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
            IsInterval     = true;

            if (SampleSize != 0 && ObservedLosses + ObservedGains != 0)
            {
                VariantFreqAll = (ObservedLosses + ObservedGains) / (double)SampleSize;
                VariantFreqAll = VariantFreqAll > 1.0 ? 1.0 : VariantFreqAll;
            }
        }



        public override SupplementaryIntervalItem GetSupplementaryInterval()
        {
            if (!IsInterval) return null;

            var intValues    = new Dictionary<string, int>();
            var doubleValues = new Dictionary<string, double>();
            var freqValues   = new Dictionary<string, double>();
            var stringValues = new Dictionary<string, string>();
            var boolValues   = new List<string>();

            var suppInterval = new SupplementaryIntervalItem(Chromosome, Start, End, null, VariantType,
                "dgv", intValues, doubleValues, freqValues, stringValues, boolValues);

            if (Id             != null) suppInterval.AddStringValue("id", Id);
            if (SampleSize     != 0)    suppInterval.AddIntValue("sampleSize", SampleSize);
            if (ObservedGains  != 0)    suppInterval.AddIntValue("observedGains", ObservedGains);
            if (ObservedLosses != 0)    suppInterval.AddIntValue("observedLosses", ObservedLosses);
            if (VariantFreqAll != null) suppInterval.AddFrequencyValue("variantFreqAll", VariantFreqAll.Value);

            return suppInterval;
        }

        public override bool Equals(object other)
        {
            // If parameter is null return false.

            if (!(other is DgvItem otherItem)) return false;

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
