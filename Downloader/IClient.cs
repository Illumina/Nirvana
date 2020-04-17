using System.Collections.Generic;

namespace Downloader
{
    /// <summary>
    /// IClient should abstract away all network activity for improved testing
    /// </summary>
    public interface IClient
    {
        List<string> DownloadLines(string remotePath);
        bool SetMetadata(RemoteFile file);
        bool DownloadFile(RemoteFile file);
    }
}
