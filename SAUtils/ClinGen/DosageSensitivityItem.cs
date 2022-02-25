using System.IO;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.ClinGen
{
    public sealed class DosageSensitivityItem : ISuppGeneItem
    {
        public string GeneSymbol { get; }
        private readonly int _hiScore;
        private readonly int _tsScore;

        public DosageSensitivityItem(string geneSymbol, int hiScore, int tsScore)
        {
            GeneSymbol     = geneSymbol;
            _hiScore       = hiScore;
            _tsScore       = tsScore;

            if (!Data.ScoreToDescription.ContainsKey(_hiScore) || !Data.ScoreToDescription.ContainsKey(_tsScore))
            {
                throw new InvalidDataException($"Unexpected score ({_hiScore}, {_tsScore}) observed for gene: {geneSymbol}");
            }
        }

        public string GetJsonString()
        {
            var sb = StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("haploinsufficiency", Data.ScoreToDescription[_hiScore]);
            jsonObject.AddStringValue("triplosensitivity", Data.ScoreToDescription[_tsScore]);
            sb.Append(JsonObject.CloseBrace);

            return StringBuilderPool.GetStringAndReturn(sb);
        }
    }
}