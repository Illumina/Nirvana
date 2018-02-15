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

        public string GetElapsedIterationTime(int numUnits, string unitName, out double unitsPerSecond)
        {
            var stopTime = DateTime.Now;
            var span = new TimeSpan(stopTime.Ticks - _startTime.Ticks);

            unitsPerSecond = numUnits / span.TotalSeconds;

            return $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}.{span.Milliseconds / 100:D1} ({unitsPerSecond:n0} {unitName}/s)";
        }

        public void Reset() => _startTime = DateTime.Now;
    }
}