using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Downloader
{
    public interface IClient
    {
        Task<List<string>> DownloadLinesAsync(string path);
        void Download(RemoteFile file, CancellationTokenSource tokenSource);
    }
}
