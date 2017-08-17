using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.SA
{
    public interface ISaPosition
    {
        ISaDataSource[] DataSources { get; }
        string GlobalMajorAllele { get; }
        void Write(IExtendedBinaryWriter writer);
    }
}