using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.FileHandling.Binary;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling.SA
{
    public class SaWriter : IDisposable
    {
        private readonly Stream _stream;
        private readonly Stream _idxStream;
        private readonly ExtendedBinaryWriter _writer;

        private readonly SaWriteBlock _block;
        private readonly MemoryStream _memoryStream;
        private readonly ExtendedBinaryWriter _msWriter;

        private readonly List<Interval<long>> _intervals;
        private readonly List<int> _refMinorPositions;

        public int RefMinorCount => _refMinorPositions.Count;

        public SaWriter(Stream stream, Stream idxStream, ISupplementaryAnnotationHeader header,
            List<IInterimInterval> smallVariantIntervals, List<IInterimInterval> svIntervals,
            List<IInterimInterval> allVariantIntervals)
        {
            _stream       = stream;
            _writer       = new ExtendedBinaryWriter(stream);
            _idxStream    = idxStream;
            _block        = new SaWriteBlock(new Zstandard(1));
            _memoryStream = new MemoryStream();
            _msWriter     = new ExtendedBinaryWriter(_memoryStream);

            _intervals         = new List<Interval<long>>(34000);
            _refMinorPositions = new List<int>(22000);

            WriteHeader(header);
            WriteIntervals(smallVariantIntervals);
            WriteIntervals(svIntervals);
            WriteIntervals(allVariantIntervals);
        }

        private void WriteHeader(ISupplementaryAnnotationHeader header)
        {
            _writer.WriteOptAscii(SupplementaryAnnotationCommon.DataHeader);
            _writer.Write(SupplementaryAnnotationCommon.DataVersion);
            _writer.Write(SupplementaryAnnotationCommon.SchemaVersion);
            _writer.Write((byte)header.GenomeAssembly);
            _writer.Write(DateTime.Now.Ticks);
            _writer.WriteOptAscii(header.ReferenceSequenceName);

            var dataSourceVersions = header.DataSourceVersions.ToList();
            _writer.WriteOpt(dataSourceVersions.Count);
            foreach (var version in dataSourceVersions) version.Write(_writer);
        }

        private void WriteIntervals(List<IInterimInterval> intervals)
        {
            _writer.WriteOpt(intervals.Count);
            foreach(var interval in intervals) interval.Write(_writer);
        }

        public void Dispose()
        {
            Flush();
            WriteIndex();
            _writer.Dispose();
            _stream.Dispose();
        }

        private void WriteIndex()
        {
            using (var writer = new ExtendedBinaryWriter(_idxStream))
            {
                SaIndex.Write(writer, _intervals, _refMinorPositions);
            }
        }

        public void Write(ISaPosition saPosition, int position, bool isRefMinor)
        {
            var uncompressedBytes = GetUncompressedBytes(saPosition);
            if (!_block.HasSpace(uncompressedBytes.Length)) Flush();

            // add new content to the block
            _block.Add(uncompressedBytes, position);
            if (isRefMinor) _refMinorPositions.Add(position);
        }

        private void Flush()
        {
            if (_block.BlockOffset == 0) return;

            var fileOffset = _stream.Position;
            var positions  = _block.Write(_stream);
            _intervals.Add(new Interval<long>(positions.Item1, positions.Item2, fileOffset));
        }

        private byte[] GetUncompressedBytes(ISaPosition position)
        {
            _memoryStream.Position = 0;
            position.Write(_msWriter);

            ArraySegment<byte> buff;
            if(!_memoryStream.TryGetBuffer(out buff)) throw new InvalidDataException("Unable to get the MemoryStream buffer.");

            var result = new byte[_memoryStream.Position];
            Buffer.BlockCopy(buff.Array, 0, result, 0, (int)_memoryStream.Position);
            return result;
        }
    }
}
