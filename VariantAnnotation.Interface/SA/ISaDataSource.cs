using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.SA
{
    public interface ISaDataSource
    {
        string KeyName { get; }
        string VcfkeyName { get; }
        bool MatchByAllele { get; }
        bool IsArray { get; }
        string AltAllele { get; }
        string[] JsonStrings { get; }
        string VcfString { get; }
        void Write(IExtendedBinaryWriter writer);
    }
}