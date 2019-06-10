using System;
using CommandLine.Utilities;
using Genome;
using VariantAnnotation.Interface;

namespace VariantAnnotation
{
    public sealed class PerformanceMetrics
    {
        private readonly Benchmark _benchmark = new Benchmark();
        private readonly ILogger _logger;

        private const int LineLength          = 75;
        private const int ReferenceNameLength = 51;

        private int _numVariantsInReference;
        private bool _hasStartedAnnotation;

        public PerformanceMetrics(ILogger logger)
        {
            _logger = logger;
            ShowTableHeader();
        }

        private void ShowTableHeader()
        {
            var divider = new string('-', LineLength);
            _logger.SetBold();
            _logger.WriteLine("Reference                                              Time      Variants/s");
            _logger.ResetColor();
            _logger.WriteLine(divider);
        }

        public void StartAnnotatingReference(IChromosome chromosome)
        {
            if (_hasStartedAnnotation) ShowAnnotationTime();

            ShowReferenceName(chromosome.UcscName);

            _benchmark.Reset();
            _hasStartedAnnotation = true;
        }

        private void ShowReferenceName(string referenceName)
        {
            int fillerLength = ReferenceNameLength - referenceName.Length + 1;

            if (fillerLength < 1)
            {
                throw new InvalidOperationException("Unable to display the performance metrics, the reference sequence name is too long.");
            }

            var filler = new string(' ', fillerLength);
            _logger.Write($"{referenceName}{filler}");
        }

        public void ShowAnnotationTime()
        {
            var annotationTime = Benchmark.ToHumanReadable(_benchmark.GetElapsedTime());

            _benchmark.GetElapsedIterationTime(_numVariantsInReference, out double variantsPerSecond);
            _numVariantsInReference = 0;

            _logger.WriteLine($"{annotationTime} {variantsPerSecond,12:N0}");
        }

        public void Increment() => _numVariantsInReference++;
    }
}