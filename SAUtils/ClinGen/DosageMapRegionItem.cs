using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.ClinGen
{
    public sealed class DosageMapRegionItem : ISuppIntervalItem
    {
        public          Chromosome Chromosome { get; }
        public          int        Start      { get; }
        public          int        End        { get; }
        public readonly int        HiScore;
        public readonly int        TsScore;
        
        public DosageMapRegionItem(Chromosome chromosome, int start, int end, int hiScore, int tsScore)
        {
            Chromosome = chromosome;
            Start      = start;
            End        = end;
            HiScore    = hiScore;
            TsScore    = tsScore;
        }
        
        public string GetJsonString()
        {
            var sb= StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("chromosome", Chromosome.EnsemblName);
            jsonObject.AddIntValue("begin", Start);
            jsonObject.AddIntValue("end", End);
            jsonObject.AddStringValue("haploinsufficiency", Data.ScoreToDescription[HiScore]);
            jsonObject.AddStringValue("triplosensitivity",  Data.ScoreToDescription[TsScore]);

            return StringBuilderPool.GetStringAndReturn(sb);
        }
    }
}