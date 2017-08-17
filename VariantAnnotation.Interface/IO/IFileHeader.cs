using System.IO;

namespace VariantAnnotation.Interface.IO
{
    public interface IFileHeader
    {
        void Write(BinaryWriter writer);
    }

    public interface ICustomCacheHeader : IFileHeader { }
}
