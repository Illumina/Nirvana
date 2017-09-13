using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Interface.Caches
{
    public interface IPredictionCache : IProvider
    {
        PredictionScore GetProteinFunctionPrediction(int predictionIndex, char newAminoAcid, int aaPosition);
    }
}