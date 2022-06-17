using System.IO;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.ClinGen
{
    public sealed class DosageSensitivityItem : ISuppGeneItem
    {
        public          string GeneSymbol { get; }
        public readonly int    HiScore;
        public readonly int    TsScore;

        public DosageSensitivityItem(string geneSymbol, int hiScore, int tsScore)
        {
            GeneSymbol = geneSymbol;
            HiScore    = hiScore;
            TsScore    = tsScore;

            if (!Data.ScoreToDescription.ContainsKey(HiScore) || !Data.ScoreToDescription.ContainsKey(TsScore))
            {
                throw new InvalidDataException($"Unexpected score ({HiScore}, {TsScore}) observed for gene: {geneSymbol}");
            }
        }

        public string GetJsonString()
        {
            var sb = StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("haploinsufficiency", Data.ScoreToDescription[HiScore]);
            jsonObject.AddStringValue("triplosensitivity",  Data.ScoreToDescription[TsScore]);
            sb.Append(JsonObject.CloseBrace);

            return StringBuilderPool.GetStringAndReturn(sb);
        }
    }
}