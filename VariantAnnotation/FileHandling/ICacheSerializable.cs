using VariantAnnotation.FileHandling.Binary;

namespace VariantAnnotation.FileHandling
{
    public interface ICacheSerializable
    {
        void Write(ExtendedBinaryWriter writer);
    }
}
