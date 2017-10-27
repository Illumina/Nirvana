using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Compression.Algorithms;
using VariantAnnotation.Interface.Intervals;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.IO;

namespace VariantAnnotation.SA
{
    public sealed class SaWriter : IDisposable
    {
        private readonly Stream _stream;
        private readonly Stream _idxStream;
        private readonly ExtendedBinaryWriter _writer;

        private readonly SaWriteBlock _block;
        private readonly MemoryStream _memoryStream;
        private readonly ExtendedBinaryWriter _msWriter;

        private readonly List<Interval<long>> _intervals;
        private readonly List<(int, string)> _globalMajorAllleInRefMinors;
        private readonly bool _leaveOpen;
        
        public int RefMinorCount => _globalMajorAllleInRefMinors.Count;

        public SaWriter(Stream stream, Stream idxStream, ISupplementaryAnnotationHeader header,
            List<ISupplementaryInterval> smallVariantIntervals, List<ISupplementaryInterval> svIntervals,
            List<ISupplementaryInterval> allVariantIntervals,List<(int,string)> globalMajorAllelesInRefMinors,bool leaveOpen=false)
        {
            _leaveOpen = leaveOpen;
            _stream = stream;
            _writer = new ExtendedBinaryWriter(stream, new UTF8Encoding(false, true), _leaveOpen);
            _idxStream = idxStream;
            _block = new SaWriteBlock(new Zstandard(1));
            _memoryStream = new MemoryStream();
            _msWriter = new ExtendedBinaryWriter(_memoryStream);

            _intervals = new List<Interval<long>>(34000);

            _globalMajorAllleInRefMinors = globalMajorAllelesInRefMinors;
            
            WriteHeader(header);
            WriteIntervals(smallVariantIntervals);
            WriteIntervals(svIntervals);
            WriteIntervals(allVariantIntervals);
        }

        private void WriteHeader(ISupplementaryAnnotationHeader header)
        {
            _writer.WriteOptAscii(SaDataBaseCommon.DataHeader);
            _writer.Write(SaDataBaseCommon.DataVersion);
            _writer.Write(SaDataBaseCommon.SchemaVersion);
            _writer.Write((byte)header.GenomeAssembly);
            _writer.Write(DateTime.Now.Ticks);
            _writer.WriteOptAscii(header.ReferenceSequenceName);

            var dataSourceVersions = header.DataSourceVersions.ToList();
            _writer.WriteOpt(dataSourceVersions.Count);
            foreach (var version in dataSourceVersions) version.Write(_writer);
        }

        private void WriteIntervals(List<ISupplementaryInterval> intervals)
        {
            _writer.WriteOpt(intervals.Count);
            foreach (var interval in intervals) interval.Write(_writer);
        }

        public void Dispose()
        {
            Flush();
            WriteIndex();
            //Console.WriteLine($"positions/block={_blockPositionCount*1.0/_blockCount}");
            if(!_leaveOpen) _stream.Dispose();
            _writer.Dispose();
            
        }

        private void WriteIndex()
        {
            using (var writer = new ExtendedBinaryWriter(_idxStream, new UTF8Encoding(false, true), _leaveOpen))
            {
                SaIndex.Write(writer, _intervals, _globalMajorAllleInRefMinors);
            }
        }

        public void Write(ISaPosition saPosition, int position)
        {
            var uncompressedBytes = GetUncompressedBytes(saPosition);
            if (!_block.HasSpace(uncompressedBytes.Length)) Flush();

            // add new content to the block
            _block.Add(uncompressedBytes, position);
        }

        private void Flush()
        {
            if (_block.BlockOffset == 0) return;

            var fileOffset = _stream.Position;
            var positions = _block.Write(_stream);
            _intervals.Add(new Interval<long>(positions.FirstPosition, positions.LastPosition, fileOffset));
        }

        private byte[] GetUncompressedBytes(ISaPosition position)
        {
            _memoryStream.Position = 0;
            position.Write(_msWriter);

            if (!_memoryStream.TryGetBuffer(out var buff)) throw new InvalidDataException("Unable to get the MemoryStream buffer.");

            var result = new byte[_memoryStream.Position];
            Buffer.BlockCopy(buff.Array, 0, result, 0, (int)_memoryStream.Position);
            return result;
        }
    }
}