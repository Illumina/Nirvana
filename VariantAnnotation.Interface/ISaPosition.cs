namespace VariantAnnotation.Interface
{
    public interface ISaPosition
    {
        ISaDataSource[] DataSources { get; }
        string GlobalMajorAllele { get; }
        void Write(IExtendedBinaryWriter writer);
    }
}
