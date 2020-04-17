using System;
using System.Collections.Generic;
using System.Threading;
using Downloader.Utilities;

namespace Downloader
{
    public static class AnnotationRepository
    {
        public static void DownloadMetadata(IClient client, List<RemoteFile> files) =>
            files.ParallelExecute(client.SetMetadata, Retry, "finished", "download the file metadata");

        public static void DownloadFiles(IClient client, List<RemoteFile> files) =>
            files.ParallelExecute(client.DownloadFile, Retry, "finished", "download the file");

        private static void Retry(RemoteFile file, Func<RemoteFile, bool> clientFunc,
            CancellationTokenSource tokenSource, string exceptionMessage)
        {
            var       numAttempts = 0;
            const int maxAttempts = 3;

            while (true)
            {
                numAttempts++;

                if (numAttempts == maxAttempts)
                {
                    Console.WriteLine($"  - Unable to {exceptionMessage} for {file.Description} after {maxAttempts} attempts.");
                    tokenSource.Cancel();
                    break;
                }

                bool success = clientFunc(file);
                if (success) break;
            }
        }
    }
}