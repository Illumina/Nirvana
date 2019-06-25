using System.Collections.Generic;
using System.IO;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.dbVar
{
    public sealed class DosageSensitivityItem : ISuppGeneItem
    {
        public string GeneSymbol { get; }
        private int _hiScore;
        private int _tsScore;

        public readonly Dictionary<int, string> ScoreToDescription = new Dictionary<int, string>()
        {
            { -1, null},
            { 0, "no evidence to suggest that dosage sensitivity is associated with clinical phenotype" },
            { 1, "little evidence suggesting dosage sensitivity is associated with clinical phenotype" },
            { 2, "emerging evidence suggesting dosage sensitivity is associated with clinical phenotype" },
            { 3, "sufficient evidence suggesting dosage sensitivity is associated with clinical phenotype" },
            { 30, "gene associated with autosomal recessive phenotype" },
            { 40, "dosage sensitivity unlikely" }
        };

        public DosageSensitivityItem(string geneSymbol, int hiScore, int tsScore)
        {
            GeneSymbol     = geneSymbol;
            _hiScore       = hiScore;
            _tsScore       = tsScore;

            if (!ScoreToDescription.TryGetValue(_hiScore, out var description))
            {
                throw new InvalidDataException($"Unexpected score ({_hiScore}, {_tsScore})observed for gene: {geneSymbol}");
            }
        }

        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddStringValue("haploinsufficiency", ScoreToDescription[_hiScore]);
            jsonObject.AddStringValue("triplosensitivity", ScoreToDescription[_tsScore]);
            sb.Append(JsonObject.CloseBrace);

            return StringBuilderCache.GetStringAndRelease(sb);
        }
    }
}