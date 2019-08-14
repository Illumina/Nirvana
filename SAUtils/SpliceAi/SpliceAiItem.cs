using System;
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
        public readonly bool IsSpliceAdjacent;

        public readonly double AcceptorGainScore;
        public readonly double AcceptorLossScore;
        public readonly double DonorGainScore;
        public readonly double DonorLossScore;

        public readonly int AcceptorGainPosition;
        public readonly int AcceptorLossPosition;
        public readonly int DonorGainPosition;
        public readonly int DonorLossPosition;

        public SpliceAiItem(IChromosome chromosome, int position, string refAllele, string altAllele, string hgnc, double acceptorGainScore, double acceptorLossScore, double donorGainScore, double donorLossScore, int acceptorGainPosition, int acceptorLossPosition, int donorGainPosition, int donorLossPosition ,bool isSpliceAdjacent)
        {
            Chromosome = chromosome;
            Position   = position;
            RefAllele  = refAllele;
            AltAllele  = altAllele;

            Hgnc                 = hgnc;
            AcceptorGainScore    = acceptorGainScore;
            AcceptorLossScore    = acceptorLossScore;
            DonorGainScore       = donorGainScore;
            DonorLossScore       = donorLossScore;
            AcceptorGainPosition = acceptorGainPosition;
            AcceptorLossPosition = acceptorLossPosition;
            DonorGainPosition    = donorGainPosition;
            DonorLossPosition    = donorLossPosition;
            IsSpliceAdjacent     = isSpliceAdjacent;
            
        }

        
        public string GetJsonString()
        {
            var sb = StringBuilderCache.Acquire();
            var jsonObject = new JsonObject(sb);

            jsonObject.AddStringValue("hgnc", Hgnc);
            if (IsSpliceAdjacent)
            {
                jsonObject.AddDoubleValue("acceptorGainScore", AcceptorGainScore, "0.#");
                jsonObject.AddDoubleValue("acceptorGainDistance", AcceptorGainPosition);

                jsonObject.AddDoubleValue("acceptorLossScore", AcceptorLossScore, "0.#");
                jsonObject.AddDoubleValue("acceptorLossDistance", AcceptorLossPosition);

                jsonObject.AddDoubleValue("donorGainScore", DonorGainScore, "0.#");
                jsonObject.AddDoubleValue("donorGainDistance", DonorGainPosition);

                jsonObject.AddDoubleValue("donorLossScore", DonorLossScore, "0.#");
                jsonObject.AddDoubleValue("donorLossDistance", DonorLossPosition);
            }
            else
            {
                if (AcceptorGainScore >= MinSpliceAiScore)
                {
                    jsonObject.AddDoubleValue("acceptorGainScore", AcceptorGainScore, "0.#");
                    jsonObject.AddDoubleValue("acceptorGainDistance", AcceptorGainPosition);
                }

                if (AcceptorLossScore >= MinSpliceAiScore)
                {
                    jsonObject.AddDoubleValue("acceptorLossScore", AcceptorLossScore, "0.#");
                    jsonObject.AddDoubleValue("acceptorLossDistance", AcceptorLossPosition);
                }

                if (DonorGainScore >= MinSpliceAiScore)
                {
                    jsonObject.AddDoubleValue("donorGainScore", DonorGainScore, "0.#");
                    jsonObject.AddDoubleValue("donorGainDistance", DonorGainPosition);

                }

                if (DonorLossScore >= MinSpliceAiScore)
                {
                    jsonObject.AddDoubleValue("donorLossScore", DonorLossScore, "0.#");
                    jsonObject.AddDoubleValue("donorLossDistance", DonorLossPosition);

                }
            }

            return StringBuilderCache.GetStringAndRelease(sb);
        }

        public static int CompareTo(SpliceAiItem one, SpliceAiItem other)
        {
            if (one.Chromosome.Index != other.Chromosome.Index)
                return one.Chromosome.Index.CompareTo(other.Chromosome.Index);

            return one.Position.CompareTo(other.Position);
        }
    }
}