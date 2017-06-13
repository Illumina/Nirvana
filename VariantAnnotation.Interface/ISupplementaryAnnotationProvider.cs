namespace VariantAnnotation.Interface
{
    public interface ISupplementaryAnnotationProvider : IDataSource
    {
        void AddAnnotation(IVariantFeature variant);
        void Load(string ucscReferenceName);
    }
}
