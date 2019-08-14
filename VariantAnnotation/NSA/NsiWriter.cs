using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Algorithms;
using Genome;
using IO;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Providers;

namespace VariantAnnotation.NSA
{
    public sealed class NsiWriter:IDisposable
    {
        private readonly BinaryWriter _writer;
        private readonly ExtendedBinaryWriter _memWriter;
        private readonly MemoryStream _memStream;
        
        public NsiWriter(BinaryWriter writer, DataSourceVersion version,
            GenomeAssembly assembly, string jsonKey, ReportFor reportFor, int schemaVersion)
        {
            _writer    = writer;
            _memStream = new MemoryStream();
            _memWriter = new ExtendedBinaryWriter(_memStream);

            version.Write(_memWriter);
            _memWriter.Write((byte)assembly);
            _memWriter.WriteOptAscii(jsonKey);
            _memWriter.Write((byte)reportFor);
            _memWriter.WriteOpt(schemaVersion);

        }

        public int Write(IEnumerable<ISuppIntervalItem> siItems)
        {
            var sortedItems = siItems.OrderBy(x => x.Chromosome.Index).ThenBy(x => x.Start).ThenBy(x => x.End).ToList();

            Console.WriteLine($"Writing {sortedItems.Count} intervals to database...");
            _memWriter.WriteOpt(sortedItems.Count);
            foreach (ISuppIntervalItem item in sortedItems)
            {
                _memWriter.WriteOptAscii(item.Chromosome.EnsemblName);
                _memWriter.WriteOptAscii(item.Chromosome.UcscName);
                _memWriter.WriteOpt(item.Chromosome.Index);
                _memWriter.WriteOpt(item.Start);
                _memWriter.WriteOpt(item.End);
                _memWriter.Write(item.GetJsonString());
            }

            var uncompressedBytes = _memStream.ToArray();
            var compressedBytes = new byte[uncompressedBytes.Length + 32];

            var compressor = new Zstandard();
            var compressSize = compressor.Compress(uncompressedBytes, uncompressedBytes.Length, compressedBytes,
                compressedBytes.Length);

            _writer.Write(compressedBytes, 0, compressSize);
            _writer.Flush();
            return sortedItems.Count;
        }

        public void Dispose()
        {
            _writer?.Dispose();
            _memWriter?.Dispose();
            _memStream?.Dispose();
        }
    }
}