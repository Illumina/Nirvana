using IO;

namespace Cache.IO;

public interface IWritable
{
    void Write(ExtendedBinaryWriter writer);
}