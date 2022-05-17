using System.Text;
using Genome;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.DataStructures
{
    public sealed class DecipherItem : ISuppIntervalItem
    {
        private readonly int?    _delNum;
        private readonly double? _delFreq;
        private readonly int?    _dupNum;
        private readonly double? _dupFreq;
        private readonly int?    _sampleSize;
        
        public Chromosome Chromosome { get; }
        public int        Start      { get; }
        public int        End      { get; }

        public DecipherItem(Chromosome chrom, int start, int end, 
            int? delNum, double? delFreq, int? dupNum, double? dupFreq, int? sampleSize)
        {
            Chromosome  = chrom;
            Start       = start;
            End         = end;
            _delNum     = delNum;
            _delFreq    = delFreq;
            _dupNum     = dupNum;
            _dupFreq    = dupFreq;
            _sampleSize = sampleSize;
        }

        public string GetJsonString()
        {
            var sb         = new StringBuilder();
            var jsonObject = new JsonObject(sb);
            
            jsonObject.AddStringValue("chromosome", Chromosome.EnsemblName);
            jsonObject.AddIntValue("begin", Start);
            jsonObject.AddIntValue("end",   End);
            jsonObject.AddIntValue("numDeletions", _delNum); 
            jsonObject.AddDoubleValue("deletionFrequency", _delFreq, JsonCommon.FrequencyRoundingFormat);
            jsonObject.AddIntValue("numDuplications", _dupNum); 
            jsonObject.AddDoubleValue("duplicationFrequency", _dupFreq, JsonCommon.FrequencyRoundingFormat);
            jsonObject.AddIntValue("sampleSize", _sampleSize); 

            return sb.ToString();
        }
    }
}