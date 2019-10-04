namespace Cloud.Messages
{
    public sealed class FileList
    {
        public string bucketName;
        public string outputDir;
        public string[] files;
    }
}