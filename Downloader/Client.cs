using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Downloader.Utilities;

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

        public List<string> DownloadLines(string remotePath)
        {
            var lines = new List<string>();

            using (var response = _httpClient.GetAsync(remotePath, HttpCompletionOption.ResponseHeadersRead).AsSync())
            {
                var stream = response.Content.ReadAsStreamAsync().AsSync();
                if (!response.IsSuccessStatusCode) return lines;

                using (var reader = new StreamReader(stream))
                {
                    while (true)
                    {
                        string line = reader.ReadLineAsync().AsSync();
                        if (line == null) break;

                        lines.Add(line);
                    }
                }
            }

            return lines;
        }

        public bool SetMetadata(RemoteFile file)
        {
            using (var response = _httpClient.GetAsync(file.RemotePath, HttpCompletionOption.ResponseHeadersRead).AsSync())
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.Write("  - ");
                    ConsoleEmbellishments.PrintWarning("WARNING: ");
                    Console.WriteLine($"{file.Description} could not be found. Skipping this file.");
                    file.Missing = true;
                    file.Skipped = true;
                    return true;
                }

                if (!response.IsSuccessStatusCode) return false;

                long? contentLength = response.Content.Headers.ContentLength;
                if (contentLength.HasValue) file.FileSize = contentLength.Value;

                DateTimeOffset? lastModified = response.Content.Headers.LastModified;
                if (lastModified.HasValue) file.LastModified = lastModified.Value;
            }

            return true;
        }

        public bool DownloadFile(RemoteFile file)
        {
            using (var response = _httpClient.GetAsync(file.RemotePath, HttpCompletionOption.ResponseHeadersRead).AsSync())
            {
                if (!response.IsSuccessStatusCode) return false;

                Console.WriteLine($"  - downloading {file.Description}");

                var stream   = response.Content.ReadAsStreamAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var fileInfo = new FileInfo(file.LocalPath);
                using (var fileStream = fileInfo.OpenWrite()) stream.CopyTo(fileStream);
            }

            return true;
        }
    }
}