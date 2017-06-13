namespace VariantAnnotation.Interface
{
    public interface IDataSourceVersion
    {
        string Name { get; }
        string Description { get; }
        string Version { get; }
        long ReleaseDateTicks { get; }
        void Write(IExtendedBinaryWriter writer);
    }
}
