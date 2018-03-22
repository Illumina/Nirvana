using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CommandLine.Utilities;
using VariantAnnotation.Interface;

namespace CacheUtils.Utilities
{
    public static class TaskExtensions
    {
        public static void Execute<T>(this IReadOnlyList<T> items, ILogger logger, string description,
            Action<T> executeAction, int numThreads = 5)
        {
            var bench     = new Benchmark();
            var tasks     = new Task[items.Count];
            var maxThread = new SemaphoreSlim(numThreads);

            for (var i = 0; i < items.Count; i++)
            {
                maxThread.Wait();
                var item = items[i];
                tasks[i] = Task.Factory.StartNew(() => executeAction(item), TaskCreationOptions.LongRunning)
                    .ContinueWith(task => maxThread.Release());
            }

            Task.WaitAll(tasks);
            logger.WriteLine($"- all {description} finished ({Benchmark.ToHumanReadable(bench.GetElapsedTime())}).\n");
        }
    }
}
