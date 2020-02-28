using System;

namespace CommandLine.Utilities
{
    public sealed class Benchmark
    {
        private DateTime _startTime;

        public Benchmark() => Reset();

        public TimeSpan GetElapsedTime()
        {
            var stopTime = DateTime.Now;
            return new TimeSpan(stopTime.Ticks - _startTime.Ticks);
        }

        public static string ToHumanReadable(TimeSpan span)
        {
            return span.Days > 0
                ? $"{span.Days}:{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}.{span.Milliseconds/100:D1}"
                : $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}.{span.Milliseconds/100:D1}";
        }

        public static double GetElapsedIterationsPerSecond(TimeSpan span, int numUnits) => numUnits / span.TotalSeconds;

        public void Reset() => _startTime = DateTime.Now;
    }
}