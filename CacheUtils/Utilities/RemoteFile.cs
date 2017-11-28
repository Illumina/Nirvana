using System;
using System.IO;
using System.Net;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;

namespace CacheUtils.Utilities
{
    public sealed class RemoteFile
    {
        private readonly string _description;
        public readonly string FilePath;
        private readonly string _url;

        static RemoteFile() => ServicePointManager.DefaultConnectionLimit = int.MaxValue;

        public RemoteFile(string description, string url, bool addDate = true)
        {
            _description = description;
            _url         = url;
            FilePath     = Path.Combine(Path.GetTempPath(), GetFilename(url, addDate));
        }

        internal static string GetFilename(string url, bool addDate)
        {
            int lastSlashPos = url.LastIndexOf('/');
            string originalFilename = url.Substring(lastSlashPos + 1);

            if (!addDate) return originalFilename;

            string extension    = Path.GetExtension(originalFilename);
            string filenameStub = Path.GetFileNameWithoutExtension(originalFilename);

            return $"{filenameStub}_{Date.GetDate(DateTime.Now.Ticks)}{extension}";
        }

        public void Download(ILogger logger)
        {
            if (File.Exists(FilePath)) return;

            logger.WriteLine($"- downloading the {_description}");
            while (!SuccessfulDownload())
            {
                logger.WriteLine($"- requeueing download of the {_description}");
            }
        }

        private bool SuccessfulDownload()
        {
            try
            {
                using (var client = new WebClient())
                {
                    client.Proxy = null;
                    client.DownloadFileTaskAsync(_url, FilePath).Wait();
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
