using System;

namespace Downloader
{
    public sealed class RemoteFile
    {
        public readonly string RemotePath;
        public readonly string LocalPath;
        public readonly string Description;

        public DateTimeOffset LastModified;
        public long           FileSize;
        public bool           Skipped; // skipped from downloading
        public bool           Missing; // missing from the server
        public bool           Pass; // passes the checks after download

        public RemoteFile(string remotePath, string localPath, string description)
        {
            RemotePath  = remotePath;
            LocalPath   = localPath;
            Description = description;
        }
    }
}