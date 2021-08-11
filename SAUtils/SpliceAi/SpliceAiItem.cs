using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace SAUtils.SpliceAi
{
    public sealed class SpliceAiItem:ISupplementaryDataItem
    {
        public IChromosome Chromosome { get; }
        public int Position { get; set; }
        public string RefAllele { get; set; }
        public string AltAllele { get; set; }
        public string Hgnc { get; set; }
        public const double MinSpliceAiScore = 0.1;
        private readonly bool _isSpliceAdjacent;

        private readonly double _acceptorGainScore;
        private readonly double _acceptorLossScore;
        private readonly double _donorGainScore;
        private readonly double _donorLossScore;

        private readonly int _acceptorGainPosition;
        private readonly int _acceptorLossPosition;
        private readonly int _donorGainPosition;
        private readonly int _donorLossPosition;

        public SpliceAiItem(IChromosome chromosome, int position, string refAllele, string altAllele, string hgnc,
            double acceptorGainScore, double acceptorLossScore, double donorGainScore, double donorLossScore,
            int acceptorGainPosition, int acceptorLossPosition, int donorGainPosition, int donorLossPosition,
            bool isSpliceAdjacent)
        {
            Chromosome = chromosome;
            Position   = position;
            RefAllele  = refAllele;
            AltAllele  = altAllele;

            Hgnc                  = hgnc;
            _acceptorGainScore    = acceptorGainScore;
            _acceptorLossScore    = acceptorLossScore;
            _donorGainScore       = donorGainScore;
            _donorLossScore        = donorLossScore;
            _acceptorGainPosition = acceptorGainPosition;
            _acceptorLossPosition = acceptorLossPosition;
            _donorGainPosition    = donorGainPosition;
            _donorLossPosition    = donorLossPosition;
            _isSpliceAdjacent     = isSpliceAdjacent;
        }

        public string GetJsonString()
        {
            var sb = StringBuilderPool.Get();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("hgnc", Hgnc);
            if (_isSpliceAdjacent)
            {
                jsonObject.AddDoubleValue("acceptorGainScore", _acceptorGainScore, "0.#");
                jsonObject.AddDoubleValue("acceptorGainDistance", _acceptorGainPosition);

                jsonObject.AddDoubleValue("acceptorLossScore", _acceptorLossScore, "0.#");
                jsonObject.AddDoubleValue("acceptorLossDistance", _acceptorLossPosition);

                jsonObject.AddDoubleValue("donorGainScore", _donorGainScore, "0.#");
                jsonObject.AddDoubleValue("donorGainDistance", _donorGainPosition);

                jsonObject.AddDoubleValue("donorLossScore", _donorLossScore, "0.#");
                jsonObject.AddDoubleValue("donorLossDistance", _donorLossPosition);
            }
            else
            {
                if (_acceptorGainScore >= MinSpliceAiScore)
                {
                    jsonObject.AddDoubleValue("acceptorGainScore", _acceptorGainScore, "0.#");
                    jsonObject.AddDoubleValue("acceptorGainDistance", _acceptorGainPosition);
                }

                if (_acceptorLossScore >= MinSpliceAiScore)
                {
                    jsonObject.AddDoubleValue("acceptorLossScore", _acceptorLossScore, "0.#");
                    jsonObject.AddDoubleValue("acceptorLossDistance", _acceptorLossPosition);
                }

                if (_donorGainScore >= MinSpliceAiScore)
                {
                    jsonObject.AddDoubleValue("donorGainScore", _donorGainScore, "0.#");
                    jsonObject.AddDoubleValue("donorGainDistance", _donorGainPosition);

                }

                if (_donorLossScore >= MinSpliceAiScore)
                {
                    jsonObject.AddDoubleValue("donorLossScore", _donorLossScore, "0.#");
                    jsonObject.AddDoubleValue("donorLossDistance", _donorLossPosition);

                }
            }

            return StringBuilderPool.GetStringAndReturn(sb);
        }
    }
}