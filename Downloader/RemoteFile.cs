namespace Downloader
{
    public sealed class RemoteFile
    {
        public readonly string RemotePath;
        public readonly string LocalPath;
        public readonly string Description;

        public RemoteFile(string remotePath, string localPath, string description)
        {
            RemotePath  = remotePath;
            LocalPath   = localPath;
            Description = description;
        }
    }
}
