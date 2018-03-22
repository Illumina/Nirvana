namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public sealed class PredictionScore
    {
        public readonly double Score;
        public readonly string Prediction;

        public PredictionScore(string prediction, double score)
        {
            Prediction = prediction;
            Score      = score;
        }
    }
}