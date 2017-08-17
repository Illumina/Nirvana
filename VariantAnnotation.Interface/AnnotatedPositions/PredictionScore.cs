namespace VariantAnnotation.Interface.AnnotatedPositions
{
	public class PredictionScore
	{
		public readonly double Score;
		public readonly string Prediction;

	    public PredictionScore(string prediction, double score)
	    {
	        Prediction = prediction;
	        Score = score;
	    }
	}

}