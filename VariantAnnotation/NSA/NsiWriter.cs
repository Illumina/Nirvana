using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Compression.Algorithms;
using Compression.FileHandling;
using Genome;
using IO;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace VariantAnnotation.NSA
{
    public sealed class NsiWriter:IDisposable
    {
        private readonly Stream _stream;
        private readonly ExtendedBinaryWriter _writer;
        private readonly bool _leaveOpen;
        
        public NsiWriter(Stream stream, DataSourceVersion version,
            GenomeAssembly assembly, string jsonKey, ReportFor reportFor, int schemaVersion,
            bool leaveOpen = false)
        {
            _stream = stream;
            _leaveOpen = leaveOpen;
            WriteHeader(version, assembly, jsonKey, reportFor, schemaVersion);

            var blockStream = new BlockStream(new Zstandard(), stream, CompressionMode.Compress);
            _writer = new ExtendedBinaryWriter(blockStream, Encoding.UTF8, leaveOpen);

        }

        private void WriteHeader(DataSourceVersion version, GenomeAssembly assembly, string jsonKey, ReportFor reportFor, int schemaVersion)
        {
            using (var writer = new ExtendedBinaryWriter(_stream, Encoding.UTF8, true))
            {
                writer.WriteOptAscii(SaCommon.NsiIdentifier);
                version.Write(writer);
                writer.Write((byte)assembly);
                writer.WriteOptAscii(jsonKey);
                writer.Write((byte)reportFor);
                writer.Write(schemaVersion);
                writer.Write(SaCommon.GuardInt);
            }
        }

        public void Write(IEnumerable<ISuppIntervalItem> siItems)
        {
            var sortedItems = siItems.OrderBy(x => x.Chromosome.Index).ThenBy(x => x.Start).ThenBy(x => x.End).ToList();

            Console.WriteLine($"Writing {sortedItems.Count} intervals to database...");
            _writer.WriteOpt(sortedItems.Count);
            
            foreach (ISuppIntervalItem item in sortedItems)
            {
                _writer.WriteOptAscii(item.Chromosome.EnsemblName);
                _writer.WriteOptAscii(item.Chromosome.UcscName);
                _writer.WriteOpt(item.Chromosome.Index);
                _writer.WriteOpt(item.Start);
                _writer.WriteOpt(item.End);
                _writer.Write(item.GetJsonString());
            }
        }

        public void Dispose()
        {
            _writer?.Dispose();
            if(!_leaveOpen) _stream?.Dispose();
        }
    }
}