using System.Text;
using Genome;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class TopMedItem : ISupplementaryDataItem
    {
        private readonly int? _alleleNum;
        private readonly int? _alleleCount;
        private readonly int? _homCount;
        private readonly bool _failedFilter;

        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }



        public TopMedItem(IChromosome chrom, int position, string refAllele, string altAllele, int? alleleNum,
            int? alleleCount, int? homCount, bool failedFilter)
        {
            Chromosome      = chrom;
            Position        = position;
            RefAllele       = refAllele;
            AltAllele       = altAllele;
            _alleleNum      = alleleNum;
            _alleleCount    = alleleCount;
            _homCount       = homCount;
            _failedFilter   = failedFilter;
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

        
    }
}