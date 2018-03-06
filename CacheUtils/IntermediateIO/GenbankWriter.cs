using System;
using System.IO;
using CacheUtils.Genbank;

namespace CacheUtils.IntermediateIO
{
    internal sealed class GenbankWriter : IDisposable
    {
        private readonly StreamWriter _writer;

        internal GenbankWriter(StreamWriter writer, IntermediateIoHeader header)
        {
            _writer = writer;
            _writer.NewLine = "\n";
            header.Write(_writer, IntermediateIoCommon.FileType.Genbank);
        }

        internal void Write(GenbankEntry entry)
        {
            var numExons = entry.Exons?.Length ?? 0;

            var codingRegionStart = entry.CodingRegion?.Start ?? -1;
            var codingRegionEnd   = entry.CodingRegion?.End   ?? -1;

            var proteinId      = entry.ProteinId ?? "";
            var proteinVersion = entry.ProteinVersion;

            _writer.WriteLine($"{entry.TranscriptId}\t{entry.TranscriptVersion}\t{proteinId}\t{proteinVersion}\t{entry.GeneId}\t{entry.Symbol}\t{codingRegionStart}\t{codingRegionEnd}\t{numExons}");
            if (entry.Exons == null) return;

            _writer.Write("Exons");
            foreach (var exon in entry.Exons) _writer.Write($"\t{exon.Start}\t{exon.End}");
            _writer.WriteLine();
        }

        public void Dispose() => _writer.Dispose();
    }
}
