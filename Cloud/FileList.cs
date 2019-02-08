using System.Diagnostics.CodeAnalysis;

namespace Cloud
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public sealed class FileList
    {
        public string bucketName;
        public string outputDir;
        public string[] files;
    }
}