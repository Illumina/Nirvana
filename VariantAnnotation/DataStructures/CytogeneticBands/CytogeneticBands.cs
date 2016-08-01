using System.Collections.Generic;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures.CytogeneticBands
{
    public class CytogeneticBands : ICytogeneticBands
    {
        #region members

        private readonly Band[][] _cytogeneticBands;
        private readonly Dictionary<string, int> _ensemblReferenceIndex;

        private string _currentReferenceName;
        private int _currentReferenceIndex;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public CytogeneticBands(Dictionary<string, int> ensemblReferenceIndex, Band[][] cytogeneticBands)
        {
            _ensemblReferenceIndex = ensemblReferenceIndex;
            _cytogeneticBands      = cytogeneticBands;
        }

        /// <summary>
        /// returns the correct cytogenetic band representation for this chromosome given
        /// the start and end coordinates
        /// </summary>
        public string GetCytogeneticBand(string ensemblRefName, int start, int end)
        {
            var startCytogeneticBand = GetCytogeneticBand(ensemblRefName, start);
            if (startCytogeneticBand == null) return null;

            // handle the single coordinate case
            if (start == end) return $"{ensemblRefName}{startCytogeneticBand}";

            // handle the dual coordinate case
            var endCytogeneticBand = GetCytogeneticBand(ensemblRefName, end);
            if (endCytogeneticBand == null) return null;

            return startCytogeneticBand == endCytogeneticBand
                ? $"{ensemblRefName}{startCytogeneticBand}"
                : $"{ensemblRefName}{startCytogeneticBand}-{endCytogeneticBand}";
        }

        /// <summary>
        /// returns the cytogenetic band corresponding to the specified position
        /// </summary>
        private string GetCytogeneticBand(string ensemblRefName, int pos)
        {
            if (ensemblRefName != _currentReferenceName)
            {
                _currentReferenceName  = ensemblRefName;

                if (!_ensemblReferenceIndex.TryGetValue(ensemblRefName, out _currentReferenceIndex))
                {
                    _currentReferenceIndex = -1;
                }
            }

            if (_currentReferenceIndex == -1) return null;

            var bands = _cytogeneticBands[_currentReferenceIndex];
            int index = BinarySearch(bands, pos);

            return index < 0 ? null : bands[index].Name;
        }

        /// <summary>
        /// returns the index of the desired element, otherwise it returns a negative number
        /// </summary>
        private static int BinarySearch(Band[] array, int position)
        {
            int begin = 0;
            int end = array.Length - 1;

            while (begin <= end)
            {
                int index = begin + (end - begin >> 1);

                int ret = array[index].Compare(position);
                if (ret == 0) return index;
                if (ret < 0) begin = index + 1;
                else end = index - 1;
            }

            return ~begin;
        }

        /// <summary>
        /// reads the cytogenetic bands from the file
        /// </summary>
        public static Band[][] Read(ExtendedBinaryReader reader)
        {
            int numReferences    = reader.ReadInt();
            var cytogeneticBands = new Band[numReferences][];

            for (int refIndex = 0; refIndex < numReferences; refIndex++)
            {
                int numBands = reader.ReadInt();
                cytogeneticBands[refIndex] = new Band[numBands];

                for (int bandIndex = 0; bandIndex < numBands; bandIndex++)
                {
                    int begin   = reader.ReadInt();
                    int end     = reader.ReadInt();
                    string name = reader.ReadAsciiString();

                    cytogeneticBands[refIndex][bandIndex] = new Band(begin, end, name);
                }
            }

            return cytogeneticBands;
        }

        /// <summary>
        /// writes the cytogenetic bands to the file
        /// </summary>
        public void Write(ExtendedBinaryWriter writer)
        {
            int numReferences = _cytogeneticBands.Length;
            writer.WriteInt(numReferences);

            for (int refIndex = 0; refIndex < numReferences; refIndex++)
            {
                int numRefEntries = _cytogeneticBands[refIndex].Length;
                writer.WriteInt(numRefEntries);

                foreach (var band in _cytogeneticBands[refIndex])
                {
                    writer.WriteInt(band.Begin);
                    writer.WriteInt(band.End);
                    writer.WriteAsciiString(band.Name);
                }
            }
        }
    }
}