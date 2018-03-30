using System.Text;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;

namespace SAUtils.InputFileParsers.TOPMed
{
    public sealed class TopMedItem : SupplementaryDataItem
    {
        private readonly int? _alleleNum;
        private readonly int? _alleleCount;
        private readonly int? _homCount;
        private readonly bool _failedFilter;

        public TopMedItem(IChromosome chrom, int position, string refAllele, string altAllele, int? alleleNum,
            int? alleleCount, int? homCount, bool failedFilter)
        {
            Chromosome      = chrom;
            Start           = position;
            ReferenceAllele = refAllele;
            AlternateAllele = altAllele;
            _alleleNum      = alleleNum;
            _alleleCount    = alleleCount;
            _homCount       = homCount;
            _failedFilter   = failedFilter;
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
            var sb         = new StringBuilder();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("allAf", ComputingUtilities.ComputeFrequency(_alleleNum, _alleleCount), false);
            jsonObject.AddIntValue("allAn", _alleleNum);
            jsonObject.AddIntValue("allAc", _alleleCount);            
            jsonObject.AddIntValue("allHc", _homCount);
            if (_failedFilter) jsonObject.AddBoolValue("failedFilter", true);

            return sb.ToString();
        }

        public override SupplementaryIntervalItem GetSupplementaryInterval() => null;
    }
}