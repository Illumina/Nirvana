using System;
using System.Diagnostics;
using CommandLine.Utilities;
using Genome;
using IO;

namespace VariantAnnotation
{
    public sealed class PerformanceMetrics
    {
        public readonly TimeKeeper Cache          = new TimeKeeper();
        public readonly TimeKeeper Annotation     = new TimeKeeper();
        public readonly TimeKeeper Preload        = new TimeKeeper();
        public readonly TimeKeeper SaPositionScan = new TimeKeeper();

        public void ShowAnnotationEntry(IChromosome chromosome, int numVariants)
        {
            Annotation.Stop();
            
            string referenceName     = GetPaddedField(chromosome.UcscName, 38);
            string preloadTime       = Preload.GetTime();
            string annotationTime    = Annotation.GetTime();
            double variantsPerSecond = Annotation.GetIterationsPerSecond(numVariants);
            
            Logger.WriteLine($"{referenceName}  {preloadTime}  {annotationTime}  {variantsPerSecond,11:N0}");
        }

        public void ShowCacheLoad()
        {
            Cache.Stop();
            string time = Cache.GetTime();
            Logger.WriteLine($"Cache                                               {time}");
        }

        public void ShowSaPositionScanLoad(int numPositions)
        {
            SaPositionScan.Stop();
            string time               = SaPositionScan.GetTime();
            double positionsPerSecond = SaPositionScan.GetIterationsPerSecond(numPositions);
            Logger.WriteLine($"SA Position Scan                                    {time}  {positionsPerSecond,11:N0}");
        }

        private static string GetPaddedField(string s, int fieldLength)
        {
            if (s.Length > fieldLength) return s.Substring(0, fieldLength - 3) + "...";
            return s.PadRight(fieldLength, ' ');
        }

        public static void ShowAnnotationHeader() =>
            MetricsCommon.DisplayHeader("\nReference                                Preload    Annotation   Variants/s");
        
        public static void ShowInitializationHeader() =>
            MetricsCommon.DisplayHeader("Initialization                                         Time     Positions/s");
        
        public void ShowSummaryTable()
        {
            MetricsCommon.DisplayHeader("\nSummary                                                Time         Percent");

            long processTicks        = GetTotalProcessTicks();
            long initializationTicks = Cache.TotalTicks + SaPositionScan.TotalTicks;
            long annotationTicks     = Annotation.TotalTicks;
            long preloadTicks        = Preload.TotalTicks;

            ShowSummaryEntry("Initialization", initializationTicks, processTicks);
            ShowSummaryEntry("Preload", preloadTicks, processTicks);
            ShowSummaryEntry("Annotation", annotationTicks, processTicks);
        }

        private void ShowSummaryEntry(string description, long entryTicks, long processTicks)
        {
            string paddedDescription = GetPaddedField(description, 50);
            string time              = Benchmark.ToHumanReadable(TimeSpan.FromTicks(entryTicks));
            double percentage        = entryTicks / (double) processTicks * 100.0;
            Logger.WriteLine($"{paddedDescription}  {time}  {percentage, 9:0.0} %");
        }

        private static long GetTotalProcessTicks() => DateTime.Now.Ticks - Process.GetCurrentProcess().StartTime.Ticks;
    }

    public sealed class TimeKeeper
    {
        public long TotalTicks { get; private set; }

        private readonly Benchmark _benchmark = new Benchmark();
        private          TimeSpan  _elapsedTime;
        
        public void Stop()
        {
            _elapsedTime = _benchmark.GetElapsedTime();
            TotalTicks += _elapsedTime.Ticks;
        }

        public void   Start()                         => _benchmark.Reset();
        public string GetTime()                       => Benchmark.ToHumanReadable(_elapsedTime);
        public double GetIterationsPerSecond(int num) => Benchmark.GetElapsedIterationsPerSecond(_elapsedTime, num);
    }
    
    public static class MetricsCommon
    {
        private const int LineLength = 75;
        private static readonly string Divider = new string('-', LineLength);

        public static void DisplayHeader(string s)
        {
            Logger.SetBold();
            Logger.WriteLine(s);
            Logger.ResetColor();
            Logger.WriteLine(Divider);
        }
    }
}