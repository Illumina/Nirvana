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

        /// <summary>
        /// returns a read-only file stream specified by the path
        /// </summary>
        public static FileStream GetFileStream(string path)
        {
            return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        }
    }
}
