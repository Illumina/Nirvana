namespace VariantAnnotation.Interface
{
    public interface IConservationScoreReader : IDataSource
    {
        bool IsInitialized { get; }
        string GetScore(int position);
        void LoadReference(string ucscReferenceName);
    }
}
