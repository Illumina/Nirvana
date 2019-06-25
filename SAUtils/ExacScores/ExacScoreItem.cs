using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.ExacScores
{
    internal sealed class ExacScoreItem : ISuppGeneItem
    {
        public string GeneSymbol { get; }
        private readonly double _pLi;
        private readonly double _pRec;
        private readonly double _pNull;

        public ExacScoreItem(string gene, double pLi, double pRec, double pNull)
        {
            GeneSymbol = gene;
            _pLi       = pLi;
            _pRec      = pRec;
            _pNull     = pNull;
        }

        
        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            sb.Append(JsonObject.OpenBrace);
            jsonObject.AddDoubleValue("pLi", _pLi, "0.00e0");
            jsonObject.AddDoubleValue("pRec", _pRec, "0.00e0");
            jsonObject.AddDoubleValue("pNull", _pNull, "0.00e0");
            sb.Append(JsonObject.CloseBrace);

            return StringBuilderCache.GetStringAndRelease(sb);

        }
    }
}