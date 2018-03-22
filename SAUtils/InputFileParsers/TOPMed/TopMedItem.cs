using System.Text;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace SAUtils.InputFileParsers.TOPMed
{
    public sealed class TopMedItem: SupplementaryDataItem
    {
        private readonly int? _numSamples;
        private readonly int? _alleleNum;
        private readonly int? _alleleCount;
        private readonly int? _homCount;
        private readonly bool _hasFailedFilters;

        public TopMedItem(IChromosome chrom, int position, string refAllele, string altAllele, int? numSamples, int? alleleNum, int? alleleCount, int? homCount, bool hasFailedFilters)
        {
            Chromosome        = chrom;
            Start             = position;
            ReferenceAllele   = refAllele;
            AlternateAllele   = altAllele;
            _numSamples       = numSamples;
            _alleleNum        = alleleNum;
            _alleleCount      = alleleCount;
            _homCount         = homCount;
            _hasFailedFilters = hasFailedFilters;
        }

        public override bool Equals(object other)
        {
            if (!(other is GnomadItem otherItem)) return false;

            // Return true if the fields match:
            return Equals(Chromosome, otherItem.Chromosome)
                   && Start == otherItem.Start
                   && AlternateAllele.Equals(otherItem.AlternateAllele);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Start.GetHashCode() ^ Chromosome.GetHashCode();
                hashCode = (hashCode * 397) ^ (AlternateAllele?.GetHashCode() ?? 0);

                return hashCode;
            }
        }


        public string GetJsonString()
        {
            var sb = new StringBuilder();
            var jsonObject = new JsonObject(sb);
            
            if (_hasFailedFilters) jsonObject.AddBoolValue("hasFailedFilters", true);
            jsonObject.AddIntValue("numSamples", _numSamples);
            jsonObject.AddStringValue("alleleFreq", ComputingUtilities.ComputeFrequency(_alleleNum, _alleleCount), false);
            jsonObject.AddIntValue("alleleNumber", _alleleNum);
            jsonObject.AddIntValue("alleleCount", _alleleCount);
            jsonObject.AddIntValue("homCount", _homCount);

            return sb.ToString();
        }

        public override SupplementaryIntervalItem GetSupplementaryInterval()
        {
            return null;
        }


    }
}