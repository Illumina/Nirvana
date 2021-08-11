using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.dbVar
{
    public sealed class DosageMapRegionItem : ISuppIntervalItem
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End   { get; }
        private readonly int _hiScore;
        private readonly int _tsScore;
        
        public DosageMapRegionItem(IChromosome chromosome, int start, int end, int hiScore, int tsScore)
        {
            Chromosome = chromosome;
            Start = start;
            End = end;
            _hiScore   = hiScore;
            _tsScore   = tsScore;
        }
        
        public string GetJsonString()
        {
            var sb= StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("chromosome", Chromosome.EnsemblName);
            jsonObject.AddIntValue("begin", Start);
            jsonObject.AddIntValue("end", End);
            jsonObject.AddStringValue("haploinsufficiency", Data.ScoreToDescription[_hiScore]);
            jsonObject.AddStringValue("triplosensitivity", Data.ScoreToDescription[_tsScore]);

            return StringBuilderPool.GetStringAndReturn(sb);
        }
    }
}