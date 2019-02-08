using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using IO;

namespace VariantAnnotation.NSA
{
    //these are non-overlapping buckets of positions 
    public sealed class IndexBucket : IComparable<IndexBucket>
    {
        private int FirstPosition { get; }
        private int LastPosition { get; set; }
        private readonly long _firstFileLocation;
        private readonly ushort _firstRecordLength;
        public int Count { get; private set; }
        private readonly List<ushort> _positionDiffs;
        private readonly List<ushort> _recordLengths;
        private readonly int _maxCount;

        private long _lastFileLocation;
        //the following data structures will only be used for query. They won't be needed for creating the index
        private readonly List<int> _positions;
        private readonly List<long> _fileLocations;
        private readonly byte[] _memStreamArray;
        private readonly byte[] _compressedBytes;

        public IndexBucket(int firstPosition, long firstFileLocation, ushort firstRecordLength, int maxCount= IndexCommons.MaxBucketSize)
        {
            FirstPosition = firstPosition;
            LastPosition = firstPosition;
            _firstFileLocation = firstFileLocation;
            _firstRecordLength = firstRecordLength;
            _maxCount = maxCount;

            Count = 1;

            _positionDiffs = new List<ushort>();
            _recordLengths = new List<ushort>();
            _memStreamArray = new byte[maxCount*8];
            _compressedBytes = new byte[maxCount * 8];

            _lastFileLocation = firstFileLocation;
            
        }

        public bool TryAdd(int position, long fileLocation, ushort recordLength)
        {
            if (position < LastPosition || fileLocation <= _lastFileLocation)
                throw new UserErrorException($"Positions and file locations can only be added in non-decreasing order.\nLast position: {LastPosition}, given position: {position}. Last file location {_lastFileLocation}, given file location {fileLocation}");

            if (Count >= _maxCount) return false;

            int positionDiff = position - LastPosition;
            if (positionDiff > ushort.MaxValue) return false;

            LastPosition = position;
            _lastFileLocation = fileLocation;
            _positionDiffs.Add((ushort)positionDiff);
            _recordLengths.Add(recordLength);
            Count++;
            return true;
        }

        public (long location, ushort length) GetAnnotationRecord(int position)
        {
            if (_positions == null)
                throw new InvalidExpressionException("Index not ready for query");

            int index = _positions.BinarySearch(position);

            return index < 0 ? ((long location, ushort length))(-1, 0) : (_fileLocations[index], _recordLengths[index]);
        }

        
        public void Write(ExtendedBinaryWriter writer, ICompressionAlgorithm compressionAlgorithm)
        {
            using (var stream = new MemoryStream(_memStreamArray))
            using(var extWriter = new ExtendedBinaryWriter(stream))
            {
                extWriter.WriteOpt(FirstPosition);
                extWriter.WriteOpt(_firstFileLocation);
                extWriter.WriteOpt(_firstRecordLength);

                extWriter.WriteOpt(_positionDiffs.Count);//diff counts

                foreach (ushort diff in _positionDiffs)
                    extWriter.WriteOpt(diff);

                foreach (ushort length in _recordLengths)
                    extWriter.WriteOpt(length);

                var compressedSize = NsaUtilities.GetCompressedBytes(compressionAlgorithm, _memStreamArray, (int)stream.Position,
                    _compressedBytes);

                writer.WriteOpt(compressedSize);
                writer.Write(_compressedBytes,0,compressedSize);
            }

            
        }

        public IndexBucket(ExtendedBinaryReader reader, ICompressionAlgorithm compressionAlgorithm)
        {
            _positions     = new List<int>();
            _fileLocations = new List<long>();
            _recordLengths = new List<ushort>();
            _compressedBytes = new byte[ushort.MaxValue];
            _memStreamArray = new byte[ushort.MaxValue];
            
            var compressedSize = reader.ReadOptInt32();
            reader.Read(_compressedBytes, 0, compressedSize);
            var decompressedSize = compressionAlgorithm.Decompress(_compressedBytes, compressedSize, _memStreamArray,
                _memStreamArray.Length);

            using (var memStream = new MemoryStream(_memStreamArray,0,decompressedSize))
            using (var extReader = new ExtendedBinaryReader(memStream))
            {
                FirstPosition = extReader.ReadOptInt32();//last position seen
                LastPosition = FirstPosition;
                _lastFileLocation = extReader.ReadOptInt64();
                _positions.Add(LastPosition);
                _fileLocations.Add(_lastFileLocation);
                var recordLength = extReader.ReadOptUInt16();
                _recordLengths.Add(recordLength);
                Count = 1;

                int diffCount = extReader.ReadOptInt32();

                for (var i = 0; i < diffCount; i++)
                {
                    LastPosition += extReader.ReadOptUInt16();
                    _positions.Add(LastPosition);
                }

                for (var i = 0; i < diffCount; i++)
                {
                    _lastFileLocation += recordLength;
                    _fileLocations.Add(_lastFileLocation);
                    recordLength = extReader.ReadOptUInt16();
                    _recordLengths.Add(recordLength);

                }

                Count += diffCount;
            }
            
        }


        public int CompareTo(IndexBucket other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;

            if (other.FirstPosition < FirstPosition) return 1;
            if (LastPosition < other.FirstPosition) return -1;

            return 0;
        }

        
    }
}