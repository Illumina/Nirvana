using System;
using System.IO;

namespace VariantAnnotation.FileHandling.SupplementaryAnnotations
{
    public class SaIndexNode : IComparable<SaIndexNode>
    {
        #region members

        internal const int SaNodeWidth = 31; // +1 for the first item makes it 32.
        public uint Position;
        private uint _lastPosition;
        private readonly uint _fileLocation; // we are expecting the files to be smaller than 4GB
        private uint _lastFilePosition;
        private readonly byte[] _nextPositions; // differential offset of the next few(SaNodeWidth) elements
        private readonly ushort[] _nextFileLocations; // differential file locations of the next few(SaNodeWidth) elements
        private int _count;
        private uint _refMinorFlags;

        #endregion

        // constructor
        public SaIndexNode(uint position, uint fileLocation, bool isRefMinor = false)
        {
            Position           = position;
            _lastPosition      = position;
            _fileLocation      = fileLocation;
            _lastFilePosition  = fileLocation;
            _refMinorFlags     = 0;
            _nextPositions     = new byte[SaNodeWidth];
            _nextFileLocations = new ushort[SaNodeWidth];

            if (isRefMinor) _refMinorFlags = 1; // setting first bit to 1 
            _count = 0;
        }

        public SaIndexNode(BinaryReader reader)
        {
            Position       = reader.ReadUInt32();
            _fileLocation  = reader.ReadUInt32();
            _count         = reader.ReadInt32();
            _refMinorFlags = reader.ReadUInt32(); // ref minor flags

            _nextPositions     = new byte[SaNodeWidth];
            _nextFileLocations = new ushort[SaNodeWidth];

            _lastPosition = Position;
            for (int i = 0; i < _count; i++)
            {
                _nextPositions[i] = reader.ReadByte();
                _lastPosition += _nextPositions[i];
            }

            _lastFilePosition = _fileLocation;
            for (int i = 0; i < _count; i++)
            {
                _nextFileLocations[i] = reader.ReadUInt16();
                _lastFilePosition += _nextFileLocations[i];
            }

        }

        public bool TryAdd(uint refPosition, uint fileLocation, bool isRefMinor)
        {
            if (_count >= SaNodeWidth) return false; //node full

            // we store a diff between positions in the node.
            if ((refPosition - _lastPosition > byte.MaxValue)
                || (fileLocation - _lastFilePosition > ushort.MaxValue))
                return false; //next position or file offset is too fat

            _nextPositions[_count]       = (byte)(refPosition - _lastPosition);
            _nextFileLocations[_count++] = (ushort)(fileLocation - _lastFilePosition);

            // doing this after incrementing the count is critical since the first bit is used for the anchor position
            if (isRefMinor) _refMinorFlags |= (uint)1 << _count;

            _lastPosition     = refPosition;
            _lastFilePosition = fileLocation;

            return true;
        }

        public uint GetFileLocation(uint position)
        {
            if (position == Position) return _fileLocation;

            // unfortunately, we cannot use binary search since the we are saving a diff of locations and not a sorted list.
            var tempPosition     = Position;
            var tempFileLocation = _fileLocation;

            for (int i = 0; i < _count; i++)
            {
                tempPosition     += _nextPositions[i];
                tempFileLocation += _nextFileLocations[i];

                if (position < tempPosition) break;
                if (position == tempPosition) return tempFileLocation;
            }

            return uint.MinValue;
        }

        public bool IsRefMinor(uint position)
        {
            if (position == Position) return (_refMinorFlags & 1) != 0;
            var tempPosition = Position;

            for (int i = 0; i < _count; i++)
            {
                tempPosition += _nextPositions[i];
                if (position < tempPosition) break;
                if (position == tempPosition) return (_refMinorFlags & ((uint)1 << (i + 1))) != 0;
            }

            return false;
        }

        public int CompareTo(SaIndexNode other)
        {
            return Position.CompareTo(other.Position);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Position);
            writer.Write(_fileLocation);
            writer.Write(_count);
            writer.Write(_refMinorFlags);

            for (int i = 0; i < _count; i++) writer.Write(_nextPositions[i]);
            for (int i = 0; i < _count; i++) writer.Write(_nextFileLocations[i]);
        }
    }
}
