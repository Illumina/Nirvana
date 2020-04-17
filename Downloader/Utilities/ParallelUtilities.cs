using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommandLine.Utilities;
using ErrorHandling.Exceptions;

namespace Downloader.Utilities
{
    public static class ParallelUtilities
    {
        private const int NumThreads = 5;

        public static void ParallelExecute(this List<RemoteFile> files, Func<RemoteFile, bool> clientFunc,
            Action<RemoteFile, Func<RemoteFile, bool>, CancellationTokenSource, string> httpAction,
            string finishedMessage, string exceptionMessage)
        {
            var bench     = new Benchmark();
            var tasks     = new Task[files.Count];
            var maxThread = new SemaphoreSlim(NumThreads);

            var tokenSource       = new CancellationTokenSource();
            var cancellationToken = tokenSource.Token;

            try
            {
                for (var i = 0; i < files.Count; i++)
                {
                    maxThread.Wait(cancellationToken);

                    var file = files[i];
                    tasks[i] = Task.Factory
                        .StartNew(() => httpAction(file, clientFunc, tokenSource, exceptionMessage), TaskCreationOptions.LongRunning)
                        .ContinueWith(task => maxThread.Release(), cancellationToken);

                    if (cancellationToken.IsCancellationRequested) break;
                }

                Task.WaitAll(tasks);
                Console.WriteLine($"  - {finishedMessage} ({Benchmark.ToHumanReadable(bench.GetElapsedTime())}).\n");
            }
            catch (OperationCanceledException)
            {
                throw new UserErrorException($"Unable to {exceptionMessage}. Please verify network connection.");
            }
        }
    }
}