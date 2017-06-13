namespace VariantAnnotation.Interface
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
