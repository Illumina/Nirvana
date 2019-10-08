namespace Cloud.Messages
{
    public sealed class FileList
    {
        // ReSharper disable InconsistentNaming
        public string bucketName;
        public string outputDir;
        public string[] files;
        // ReSharper restore InconsistentNaming
    }
}