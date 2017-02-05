using System.IO;

namespace VariantAnnotation.Utilities
{
    public static class FileUtilities
    {
        /// <summary>
        /// returns the original file path if this was a FileStream
        /// </summary>
        public static string GetPath(Stream stream)
        {
            var fs = stream as FileStream;
            return fs == null ? "(stream)" : fs.Name;
        }

        public static FileStream GetReadStream(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        public static FileStream GetCreateStream(string path) => new FileStream(path, FileMode.Create);
    }
}
