using System;
using CommandLine.Utilities;

namespace VariantAnnotation.GenericScore
{
    public sealed class MetaData
    {
        private int     _totalChromosomeCount;
        private int     _totalBlockCount;
        private int     _blankBlockCount;
        private uint    _compressedChromosomeSize;
        private uint    _uncompressedChromosomeSize;
        private uint    _compressedSize;
        private uint    _uncompressedSize;
        private uint    _unmatchedReferencePositionsCount;
        private double  _totalProcessingTime;
        private double  _chromosomeProcessingTime;
        private ushort? _lastChromosome = null;

        private readonly Benchmark _blockBenchmark;

        private const string DashedLine = "________________________________________________________________";

        private double AverageCompressionRatio    => CalculateCompressionRatio(_compressedSize, _uncompressedSize);
        private double AverageCompressedBlockSize => (double) _compressedSize / _totalBlockCount;
        private double AverageProcessingTime      => _totalProcessingTime     / _totalBlockCount;
        private double AverageWriteSpeed          => _compressedSize          / _totalProcessingTime / 1_000_000;

        private static double CalculateCompressionRatio(uint compressedSize, uint uncompressedSize)
        {
            return compressedSize * 100.0 / uncompressedSize;
        }

        public MetaData()
        {
            _blockBenchmark = new Benchmark();
        }

        public void AddIndexBlock(ushort chromosomeIndex, int blockNumber, long fileStartingPosition, uint uncompressedSize, uint compressedSize)
        {
            double processingTime = _blockBenchmark.GetElapsedTime().TotalSeconds;

            _totalBlockCount++;
            if (fileStartingPosition < 0)
            {
                _blankBlockCount++;
            }

            _uncompressedSize += uncompressedSize;
            _compressedSize   += compressedSize;

            _uncompressedChromosomeSize += uncompressedSize;
            _compressedChromosomeSize   += compressedSize;

            _totalProcessingTime      += processingTime;
            _chromosomeProcessingTime += processingTime;

            PrintFormattedString(chromosomeIndex, blockNumber, uncompressedSize, compressedSize, processingTime);

            _blockBenchmark.Reset();
        }

        public void AddChromosomeBlock(ushort chromosomeIndex)
        {
            _totalChromosomeCount++;

            if (_lastChromosome == null)
            {
                _lastChromosome = chromosomeIndex;
                return;
            }

            PrintFormattedString(_lastChromosome, null, _uncompressedChromosomeSize, _compressedChromosomeSize, _chromosomeProcessingTime);

            _lastChromosome             = chromosomeIndex;
            _chromosomeProcessingTime   = 0;
            _uncompressedChromosomeSize = 0;
            _compressedChromosomeSize   = 0;
        }

        public void TrackUnmatchedReferencePositions()
        {
            _unmatchedReferencePositionsCount++;
        }

        private static void PrintFormattedString(ushort? chromosomeIndex, int? blockNumber, uint uncompressedSize, uint compressedSize,
            double processingTime)
        {
            string headerLine = $"{chromosomeIndex}:{blockNumber}";

            if (blockNumber == null)
            {
                headerLine = $"{DashedLine}\n{chromosomeIndex}";
            }

            Console.WriteLine(
                $"{headerLine}"                                                          +
                $"\t{compressedSize} bytes/{uncompressedSize} bytes\t= "                 +
                $"{CalculateCompressionRatio(compressedSize, uncompressedSize):F1} % \t" +
                $"Processing Time {processingTime:F4} s"
            );

            if (blockNumber == null)
            {
                Console.WriteLine($"{DashedLine}");
            }
        }

        public void PrintWriteMetrics()
        {
            PrintFormattedString(_lastChromosome, null, _uncompressedChromosomeSize, _compressedChromosomeSize, _chromosomeProcessingTime);

            Console.WriteLine(
                $"{DashedLine}\n"                                                          +
                $"Write Metrics\n"                                                         +
                $"{DashedLine}\n"                                                          +
                $"Total Chromosomes = {_totalChromosomeCount}\n"                           +
                $"Total Blocks = {_totalBlockCount}\n"                                     +
                $"Blank Blocks = {_blankBlockCount}\n"                                     +
                $"Unmatched Reference Positions = {_unmatchedReferencePositionsCount}\n"   +
                $"Total Compressed Size = {_compressedSize} bytes\n"                       +
                $"Total Uncompressed Size = {_uncompressedSize} bytes\n"                   +
                $"Total Processing Time = {_totalProcessingTime:F3} seconds\n"             +
                $"Average Compressed Block Size = {AverageCompressedBlockSize:F0} bytes\n" +
                $"Average Compression Ratio = {AverageCompressionRatio:F1} %\n"            +
                $"Average Processing Time = {AverageProcessingTime:F4} seconds\n"          +
                $"Average Writing Speed = {AverageWriteSpeed:F4} MB/second\n"              +
                $"{DashedLine}"
            );
        }
    }
}