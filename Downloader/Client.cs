using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Downloader
{
    public sealed class Client : IClient
    {
        private readonly HttpClient _httpClient;

        public Client(string hostName)
        {
            var baseUri = new Uri($"http://{hostName}");

            ServicePointManager.DefaultConnectionLimit                           = int.MaxValue;
            ServicePointManager.FindServicePoint(baseUri).ConnectionLeaseTimeout = 60 * 1000;

            _httpClient = new HttpClient { BaseAddress = baseUri };
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.ConnectionClose = false;
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));
        }

        public async Task<List<string>> DownloadLinesAsync(string path)
        {
            var lines = new List<string>();

            using (var response = await _httpClient.GetAsync(path, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
            {
                var stream = await response.Content.ReadAsStreamAsync();
                if (!response.IsSuccessStatusCode) return lines;

                using (var reader = new StreamReader(stream))
                {
                    while (true)
                    {
                        string line = await reader.ReadLineAsync();
                        if (line == null) break;

                        lines.Add(line);
                    }
                }
            }

            return lines;
        }

        public void Download(RemoteFile file, CancellationTokenSource tokenSource)
        {
            if (File.Exists(file.LocalPath)) return;

            Console.WriteLine($"- downloading {file.Description}");

            var numAttempts       = 0;
            const int maxAttempts = 3;

            while (!SuccessfulDownloadAsync(file).ConfigureAwait(false).GetAwaiter().GetResult())
            {
                if (numAttempts == maxAttempts)
                {
                    tokenSource.Cancel();
                    throw new InvalidDataException($"Unable to download {file.Description} after retrying {maxAttempts} times.");
                }

                Console.WriteLine($"- requeuing download of {file.Description}");
                numAttempts++;
            }
        }

        private async Task<bool> SuccessfulDownloadAsync(RemoteFile file)
        {
            try
            {
                using (var response = await _httpClient.GetAsync(file.RemotePath, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false))
                {
                    var stream = await response.Content.ReadAsStreamAsync();
                    if (!response.IsSuccessStatusCode) return false;

                    var fileInfo = new FileInfo(file.LocalPath);
                    using (var fileStream = fileInfo.OpenWrite())
                    {
                        await stream.CopyToAsync(fileStream);
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }
    }
}
