namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IFeatureVariantEffects
    {
        bool Ablation();
        bool Amplification();
        bool Truncation();
        bool Elongation();
    }
}