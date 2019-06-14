using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommandLine.Utilities;
using ErrorHandling.Exceptions;

namespace Downloader.FileExtensions
{
    public static class DownloadExtensions
    {
        private const int NumThreads = 5;

        public static void Download(this List<RemoteFile> files, IClient client)
        {
            var bench     = new Benchmark();
            var tasks     = new Task[files.Count];
            var maxThread = new SemaphoreSlim(NumThreads);

            var tokenSource = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            try
            {
                for (var i = 0; i < files.Count; i++)
                {
                    maxThread.Wait(cancellationToken);
                    var item = files[i];
                    tasks[i] = Task.Factory.StartNew(() => client.Download(item, tokenSource), TaskCreationOptions.LongRunning)
                        .ContinueWith(task => maxThread.Release(), cancellationToken);

                    if (cancellationToken.IsCancellationRequested) break;
                }

                Task.WaitAll(tasks);
                Console.WriteLine($"- all downloads finished ({Benchmark.ToHumanReadable(bench.GetElapsedTime())}).\n");
            }
            catch (OperationCanceledException)
            {
                throw new UserErrorException("Unable to download the annotation files. Please verify network connection.");
            }
        }
    }
}
