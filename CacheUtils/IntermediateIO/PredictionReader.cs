using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using IO;
using OptimizedCore;

namespace CacheUtils.IntermediateIO
{
    public sealed class PredictionReader : IDisposable
    {
        private readonly IDictionary<ushort, IChromosome> _refIndexToChromosome;
        private readonly StreamReader _reader;

        public PredictionReader(Stream stream, IDictionary<ushort, IChromosome> refIndexToChromosome,
            IntermediateIoCommon.FileType expectedFileType)
        {
            _refIndexToChromosome = refIndexToChromosome;
            _reader = FileUtilities.GetStreamReader(stream);
            IntermediateIoCommon.ReadHeader(_reader, expectedFileType);
        }

        public (string[] PredictionData, Dictionary<int, int> TranscriptToPredictionIndex, IChromosome Chromosome)
            GetPredictionData()
        {
            var chromosomeHeader            = GetChromosomeHeader();
            var predictionData              = new string[chromosomeHeader.NumPredictions];
            var transcriptToPredictionIndex = new Dictionary<int, int>(chromosomeHeader.NumPredictions);

            for (var predictionIndex = 0; predictionIndex < chromosomeHeader.NumPredictions; predictionIndex++)
            {
                var prediction = GetNextPrediction();
                predictionData[predictionIndex] = prediction.PredictionData;
                foreach (int index in prediction.TranscriptIndices)
                    transcriptToPredictionIndex[index] = predictionIndex;
            }
                              
            return (predictionData, transcriptToPredictionIndex, chromosomeHeader.Chromosome);
        }

        private (IChromosome Chromosome, int NumPredictions) GetChromosomeHeader()
        {
            string line = _reader.ReadLine();
            var cols = line?.OptimizedSplit('\t');
            if (cols == null) throw new InvalidDataException("Found an unexpected null line when parsing the chromosome header in the prediction reader.");
            if (cols.Length != 3) throw new InvalidDataException($"Expected 3 columns in the chromosome header, but found {cols.Length}");

            ushort referenceIndex = ushort.Parse(cols[1]);
            var chromosome        = ReferenceNameUtilities.GetChromosome(_refIndexToChromosome, referenceIndex);
            int numPredictions    = int.Parse(cols[2]);

            return (chromosome, numPredictions);
        }

        private (List<int> TranscriptIndices, string PredictionData) GetNextPrediction()
        {
            string line = _reader.ReadLine();
            if (line == null) throw new InvalidDataException("Found an unexpected empty line while parsing the prediction file.");

            var cols = line.OptimizedSplit('\t');
            if (cols.Length != 2) throw new InvalidDataException($"Expected 2 columns in the prediction entry, but found {cols.Length}");

            var transcriptIndices = GetTranscriptIndices(cols[0]);
            string predictionData = cols[1];

            return (transcriptIndices, predictionData);
        }

        private static List<int> GetTranscriptIndices(string s)
        {
            var indexStrings = s.OptimizedSplit(',');
            var indices      = new int[indexStrings.Length];
            for (var i = 0; i < indexStrings.Length; i++) indices[i] = int.Parse(indexStrings[i]);
            return indices.ToList();
        }

        public void Dispose() => _reader.Dispose();
    }
}
