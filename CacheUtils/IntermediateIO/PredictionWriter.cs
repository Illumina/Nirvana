using System;
using System.Collections.Generic;
using System.IO;
using Genome;

namespace CacheUtils.IntermediateIO
{
    internal sealed class PredictionWriter : IDisposable
    {
        private readonly StreamWriter _writer;

        internal PredictionWriter(StreamWriter writer, IntermediateIoHeader header,
            IntermediateIoCommon.FileType fileType)
        {
            _writer = writer;
            _writer.NewLine = "\n";
            header.Write(_writer, fileType);
        }

        internal void Write(IChromosome chromosome, Dictionary<string, List<int>> predictionDict)
        {
            _writer.WriteLine($"{chromosome.UcscName}\t{chromosome.Index}\t{predictionDict.Count}");
            foreach (var kvp in predictionDict) WritePrediction(kvp.Value, kvp.Key);
        }

        private void WritePrediction(IEnumerable<int> transcriptIds, string predictionData)
        {
            string transcriptIdString = string.Join(',', transcriptIds);
            _writer.WriteLine($"{transcriptIdString}\t{predictionData}");
        }

        public void Dispose() => _writer.Dispose();
    }
}
