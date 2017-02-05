using System;

namespace VariantAnnotation.Utilities
{
    public sealed class Benchmark
    {
        #region members

        internal DateTime StartTime;

        #endregion

        // constructor
        public Benchmark()
        {
            Reset();
        }

        /// <summary>
        /// returns the number of elapsed time since the last reset
        /// </summary>
        public TimeSpan GetElapsedTime()
        {
            var stopTime = DateTime.Now;
            return new TimeSpan(stopTime.Ticks - StartTime.Ticks);
        }

        public static string ToHumanReadable(TimeSpan span)
        {
            return span.Days > 0
                ? $"{span.Days}:{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}.{span.Milliseconds/100:D1}"
                : $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}.{span.Milliseconds/100:D1}";
        }

        /// <summary>
        /// returns the number of elapsed time since the last reset
        /// </summary>
        public string GetElapsedIterationTime(int numUnits, string unitName, out double unitsPerSecond)
        {
            var stopTime = DateTime.Now;
            var span = new TimeSpan(stopTime.Ticks - StartTime.Ticks);

            unitsPerSecond = numUnits / span.TotalSeconds;

            return $"{span.Hours:D2}:{span.Minutes:D2}:{span.Seconds:D2}.{span.Milliseconds / 100:D1} ({unitsPerSecond:n0} {unitName}/s)";
        }

        /// <summary>
        /// resets the benchmark start time
        /// </summary>
        public void Reset()
        {
            StartTime = DateTime.Now;
        }
    }
}