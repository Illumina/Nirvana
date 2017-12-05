using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.GeneAnnotation
{
    public interface IGeneAnnotationSource 
    {
        string DataSource { get; }
        string[] JsonStrings { get; }
        bool IsArray { get; }

        void Write(IExtendedBinaryWriter writer);

    }
}