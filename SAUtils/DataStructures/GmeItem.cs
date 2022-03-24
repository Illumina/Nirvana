using System.Text;
using Genome;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class GmeItem : ISupplementaryDataItem
    {
        private readonly int?    _alleleCount;
        private readonly int?    _alleleNum;
        private readonly double? _alleleFreq;
        private readonly bool    _failedFilter;

        public Chromosome Chromosome { get; }
        public int         Position   { get; set; }
        public string      RefAllele  { get; set; }
        public string      AltAllele  { get; set; }



        public GmeItem(Chromosome chrom, int position, string refAllele, string altAllele, 
            int? alleleCount, int? alleleNum, double? alleleFreq, bool failedFilter)
        {
            Chromosome    = chrom;
            Position      = position;
            RefAllele     = refAllele;
            AltAllele     = altAllele;
            _alleleCount  = alleleCount;
            _alleleNum    = alleleNum;
            _alleleFreq   = alleleFreq;
            _failedFilter = failedFilter;
        }

        public string GetJsonString()
        {
            var sb         = new StringBuilder();
            var jsonObject = new JsonObject(sb);
            
            jsonObject.AddIntValue("allAc", _alleleCount); 
            jsonObject.AddIntValue("allAn", _alleleNum); 
            jsonObject.AddDoubleValue("allAf", _alleleFreq);
            if (_failedFilter) jsonObject.AddBoolValue("failedFilter", true);

            return sb.ToString();
        }

        public string InputLine { get; set; }
    }
}