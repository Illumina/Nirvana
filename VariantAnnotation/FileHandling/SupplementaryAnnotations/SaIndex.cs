using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.FileHandling.SupplementaryAnnotations
{
    public class SaIndex
    {
        #region members

        // a sorted array of SaIndexNodes that will be used for binary searching
        private readonly SaIndexNode[] _saIndexNodes;//will be used for searching by SA reader
        private readonly List<SaIndexNode> _saIndexNodesList;

        // some private variables for caching search
        private uint _lastSearchPosition = uint.MinValue;
        private int _lastSearchIndex = -1;

        private readonly SaIndexNode _searchNode = new SaIndexNode(uint.MinValue, uint.MinValue);

        #endregion

        // for the SA writer
        public SaIndex(int numExpectedItems = 1024)
        {
            _saIndexNodesList = new List<SaIndexNode>(numExpectedItems);
        }

        // for the SA reader
        public SaIndex(BinaryReader reader)
        {
            string header = reader.ReadString();
            ushort version = reader.ReadUInt16();
            reader.ReadInt64();
            reader.ReadString();
            SupplementaryAnnotationCommon.CheckGuard(reader);


            if ((header != SupplementaryAnnotationCommon.IndexHeader) || (version != SupplementaryAnnotationCommon.IndexVersion))
            {
                throw new UserErrorException($"The header check failed for the supplementary annotation index file ({reader.BaseStream}): ID: exp: {SupplementaryAnnotationCommon.IndexHeader} obs: {header}, version: exp: {SupplementaryAnnotationCommon.IndexVersion} obs: {version}");
            }

            var count = reader.ReadInt32();
            _saIndexNodes = new SaIndexNode[count];

            for (int i = 0; i < count; i++) _saIndexNodes[i] = new SaIndexNode(reader);

            SupplementaryAnnotationCommon.CheckGuard(reader);
        }

        public void Add(uint position, uint fileLocation, bool isRefMinor)
        {
            if (_saIndexNodesList.Count == 0)
            {
                _saIndexNodesList.Add(new SaIndexNode(position, fileLocation, isRefMinor));
                return;
            }

            var currIndex = _saIndexNodesList.Count - 1;

            if (_saIndexNodesList[currIndex].TryAdd(position, fileLocation, isRefMinor)) return;

            _saIndexNodesList.Add(new SaIndexNode(position, fileLocation, isRefMinor));
        }

        private int GetNodeIndex(uint position)
        {
            if (_lastSearchPosition == position)
                return _lastSearchIndex;
            //The index of the specified value in the specified array, if value is found; otherwise, a negative number. If value is not found andvalue is less than one or more elements in array, the negative number returned is the bitwise complement of the index of the first element that is larger than value. If value is not found and value is greater than all elements in array, the negative number returned is the bitwise complement of (the index of the last element plus 1). If this method is called with a non-sorted array, the return value can be incorrect and a negative number could be returned, even if valueis present in array.

            _searchNode.Position = position;//reusing _searchNode
            _lastSearchIndex = Array.BinarySearch(_saIndexNodes, _searchNode);

            //If value is not found and value is less than one or more elements in array, the negative number returned is the bitwise complement of the index of the first element that is larger than value

            return _lastSearchIndex < 0 ? ~_lastSearchIndex - 1 : _lastSearchIndex;

        }

        public uint GetFileLocation(uint position)
        {
            var index = GetNodeIndex(position);

            return index < 0 ? uint.MinValue : _saIndexNodes[index].GetFileLocation(position);
        }

        public bool IsRefMinor(uint position)
        {
            var index = GetNodeIndex(position);

            return index >= 0 && _saIndexNodes[index].IsRefMinor(position);
        }

        public void Write(string fileName, string refSeq)
        {
            using (var stream = new FileStream(fileName, FileMode.Create))
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write(SupplementaryAnnotationCommon.IndexHeader);
                writer.Write(SupplementaryAnnotationCommon.IndexVersion);
                writer.Write(DateTime.UtcNow.Ticks);
                writer.Write(refSeq);

                // write the guard integer
                writer.Write(SupplementaryAnnotationCommon.GuardInt);

                // write the index
                writer.Write(_saIndexNodesList.Count);

                foreach (var saIndexNode in _saIndexNodesList)
                    saIndexNode.Write(writer);

                // write the guard integer
                writer.Write(SupplementaryAnnotationCommon.GuardInt);
            }
        }


    }
}
