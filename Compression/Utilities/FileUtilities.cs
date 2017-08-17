using System.IO;

namespace Compression.Utilities
{
    internal static class FileUtilities
    {
        internal static FileStream GetReadStream(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        internal static FileStream GetCreateStream(string path) => new FileStream(path, FileMode.Create);
    }
}
