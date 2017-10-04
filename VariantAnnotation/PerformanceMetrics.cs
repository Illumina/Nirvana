using System;
using CommandLine.Utilities;

namespace VariantAnnotation
{
    public sealed class PerformanceMetrics
    {
        #region members

        private readonly Benchmark _referenceBenchmark;
        private readonly Benchmark _cacheBenchmark;
        private readonly Benchmark _annotationBenchmark;

        private readonly bool _disableOutput = false;

        private string _referenceName;
        private string _referenceTime;

        private const int LineLength = 75;
        private readonly string _divider;

        private int _numVariantsInReference;
        private bool _hasStartedAnnotation;

        #endregion

        /// <summary>
        /// private constructor for our singleton
        /// </summary>
        private PerformanceMetrics()
        {
            _referenceBenchmark = new Benchmark();
            _cacheBenchmark = new Benchmark();
            _annotationBenchmark = new Benchmark();

            _divider = new string('-', LineLength);
        }

        /// <summary>
        /// access PerformanceMetrics.Instance to get the singleton object
        /// </summary>
        public static PerformanceMetrics Instance { get; } = new PerformanceMetrics();

        /// <summary>
        /// starts benchmarking the reference loading time
        /// </summary>
        public void StartReference(string referenceName)
        {
            StopAnnotation();
            _referenceName = referenceName;
            _referenceBenchmark.Reset();
            _cacheBenchmark.Reset();
            _annotationBenchmark.Reset();
            _hasStartedAnnotation = true;
        }

        /// <summary>
        /// stop benchmarking the reference loading time
        /// </summary>
        public void StopReference()
        {
            _referenceTime = Benchmark.ToHumanReadable(_referenceBenchmark.GetElapsedTime());
            if (!_disableOutput) ShowReferenceTime();
            if (!_disableOutput) Console.WriteLine("cache & sa: {0}", Benchmark.ToHumanReadable(_cacheBenchmark.GetElapsedTime()));
           
        }

        /// <summary>
        /// stop benchmarking the annotation time
        /// </summary>
        private void StopAnnotation()
        {
            if (!_hasStartedAnnotation) return;

            double dummy;
            if (!_disableOutput) Console.WriteLine((string) "annotation: {0}", (object) _annotationBenchmark.GetElapsedIterationTime(_numVariantsInReference, "variants", out dummy));
            _numVariantsInReference = 0;
        }

        /// <summary>
        /// increments the variant counter
        /// </summary>
        public void Increment()
        {
            _numVariantsInReference++;
        }

        /// <summary>
        /// returns a string representation of the performance metrics
        /// </summary>
        private void ShowReferenceTime()
        {
            // create the filler string
            const int referenceTimeLength = 22;
            int fillerLength = LineLength - referenceTimeLength - _referenceName.Length;

            if (fillerLength < 1)
            {
                throw new InvalidOperationException("Unable to display the performance metrics, the reference sequence name is too long.");
            }

            var filler = new string(' ', fillerLength);

            // display the reference time
            Console.WriteLine((string) _divider);
            Console.Write("reference:  {0}{1}", _referenceTime, filler);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine((string) _referenceName);
            Console.ResetColor();
        }
    }
}