using System.Collections.Generic;
using System.Data;
using System.Text;
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
        private readonly List<SpliceScore> _scores;
        public const double MinSpliceAiScore = 0.1;


        public SpliceAiItem(IChromosome chromosome, int position, string refAllele, string altAllele, double acceptorGainScore, double acceptorLossScore, double donorGainScore, double donorLossScore, int acceptorGainPosition, int acceptorLossPosition, int donorGainPosition, int donorLossPosition ,bool isSpliceAdjacent)
        {
            Chromosome = chromosome;
            Position   = position;
            RefAllele  = refAllele;
            AltAllele  = altAllele;

            _scores = new List<SpliceScore>(4);
            if (isSpliceAdjacent)
            {
                _scores.Add(new SpliceScore(SpliceScoreType.acceptor_gain, acceptorGainScore, acceptorGainPosition));
                _scores.Add(new SpliceScore(SpliceScoreType.acceptor_loss, acceptorLossScore, acceptorLossPosition));
                _scores.Add(new SpliceScore(SpliceScoreType.donor_gain, donorGainScore, donorGainPosition));
                _scores.Add(new SpliceScore(SpliceScoreType.donor_loss, donorLossScore, donorLossPosition));
            }
            else
            {
                if (acceptorGainScore >= MinSpliceAiScore) _scores.Add(new SpliceScore(SpliceScoreType.acceptor_gain, acceptorGainScore, acceptorGainPosition));
                if (acceptorLossScore >= MinSpliceAiScore) _scores.Add(new SpliceScore(SpliceScoreType.acceptor_loss, acceptorLossScore, acceptorLossPosition));
                if (donorGainScore >= MinSpliceAiScore) _scores.Add(new SpliceScore(SpliceScoreType.donor_gain, donorGainScore, donorGainPosition));
                if (donorLossScore >= MinSpliceAiScore) _scores.Add(new SpliceScore(SpliceScoreType.donor_loss, donorLossScore, donorLossPosition));
            }


            if(_scores.Count ==0) throw new DataException($"No score above threshold for {chromosome.UcscName}:{position}");
        }

        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            bool isFirst = true;
            foreach (var score in _scores)
            {
                if (!isFirst) sb.Append("},{");
                isFirst = false;
                score.GetJsonString(sb);
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        private sealed class SpliceScore
        {
            private readonly SpliceScoreType _type;
            private readonly double _score;
            private readonly int _distance;

            public SpliceScore(SpliceScoreType type, double score, int distance)
            {
                _type     = type;
                _score    = score;
                _distance = distance;
            }

            public void GetJsonString(StringBuilder sb)
            {
                var jsonObject = new JsonObject(sb);

                //sb.Append(JsonObject.OpenBrace);
                jsonObject.AddStringValue("type", _type.ToString().Replace('_',' '));
                jsonObject.AddIntValue("distance", _distance);
                jsonObject.AddDoubleValue("score", _score,"0.#");
                //sb.Append(JsonObject.CloseBrace);
            }
        }

        private enum SpliceScoreType: byte
        {
            // ReSharper disable InconsistentNaming
            acceptor_gain,
            acceptor_loss,
            donor_gain,
            donor_loss
        }
    }
}