using IO;
using JSON;

namespace Versioning;

public interface IDataSourceVersion : IJsonSerializer
{
    string Name             { get; }
    string Description      { get; }
    string Version          { get; }
    long   ReleaseDateTicks { get; }
    void Write(IExtendedBinaryWriter writer);
}