using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling.Compression;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.FileHandling.Phylop
{
    /// <summary>
    /// This is the database object that gives the user the ability to query phylop score for any chormosome location.
    /// </summary>
    public sealed class PhylopReader : IConservationScoreReader, IDisposable
    {
        #region members

        private readonly string _saDirectory;
        private ExtendedBinaryReader _reader;
        private readonly ICompressionAlgorithm _qlz;

        //in wigFix files, values for all positions are not available.
        //They come as contiguous intervals and knowing the file location of the score of a particular location is not possible.
        //This limitation motivated creation of the npd binary file format.
        //This list is our data structure that helps us to determine file location of the score of a chromosome location.
        private readonly List<PhylopInterval> _phylopIntervals;
        private int _currentIntervalIndex;
        private long _intervalListPosition;
        private readonly short[] _scores;
        private int _scoreCount;
        private byte[] _scoreBytes;

        public bool IsInitialized { get; private set; }

        private string _currentReferenceName;

        #endregion
        #region IDisposable

        private bool _isDisposed;
        private DataSourceVersion _version;
        private GenomeAssembly _genomeAssembly;

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// protected implementation of Dispose pattern. 
        /// </summary>
        private void Dispose(bool disposing)
        {
            lock (this)
            {
                if (_isDisposed) return;

                if (disposing)
                {
                    // Free any other managed objects here. 
                    Close();
                }

                // Free any unmanaged objects here. 
                _isDisposed = true;
            }
        }
        #endregion

        public GenomeAssembly GenomeAssembly => PhylopCommon.GetGenomeAssembly(_saDirectory);

        public IEnumerable<IDataSourceVersion> DataSourceVersions => PhylopCommon.GetDataSourceVersions(_saDirectory);

        private void Close()
        {
            _reader.Dispose();
        }

        /// <summary>
        /// constructor
        /// </summary>
        private PhylopReader()
        {
            _phylopIntervals      = new List<PhylopInterval>();
            _currentIntervalIndex = -1;

            _scores     = new short[PhylopCommon.MaxIntervalLength];
            _scoreBytes = new byte[PhylopCommon.MaxIntervalLength * 2];
            _qlz        = new QuickLZ();
        }

        /// <summary>
        /// constructor (stream)
        /// </summary>
        public PhylopReader(Stream stream) : this()
        {
            if (stream == null) return;
            _reader = new ExtendedBinaryReader(stream);
            LoadHeader();
        }

        /// <summary>
        /// constructor (SA directory)
        /// </summary>
        public PhylopReader(string saDirectory) : this()
        {
            _saDirectory = saDirectory;
        }

        public void LoadReference(string ucscReferenceName)
        {
            IsInitialized = false;
            if (_saDirectory == null || ucscReferenceName == _currentReferenceName) return;
            _currentReferenceName = ucscReferenceName;

            _currentIntervalIndex = -1;
            _phylopIntervals.Clear();

            // load the appropriate phyloP file
            var stream = PhylopCommon.GetStream(_saDirectory, ucscReferenceName);

            if (stream == null)
            {
                _reader = null;
                return;
            }

            _reader = new ExtendedBinaryReader(stream);
            LoadHeader();
        }

        private void LoadHeader()
        {
            var identifier = _reader.ReadString();
            if (identifier != PhylopCommon.Header)
                throw new InvalidDataException("Unrecognized file header: " + identifier);

            var schemaVersion = _reader.ReadInt16();
            if (schemaVersion != PhylopCommon.SchemaVersion)
                throw new InvalidDataException("Expected phylop schema version:" + PhylopCommon.SchemaVersion + " observed schema version: " + schemaVersion);

            var dataVersion = _reader.ReadInt16();
            if (dataVersion != PhylopCommon.DataVersion)
                Console.WriteLine("WARNING: Expected phylop data version:" + PhylopCommon.DataVersion + " observed data version: " + dataVersion);

            _genomeAssembly = (GenomeAssembly)_reader.ReadByte();
            _version = new DataSourceVersion(_reader);

            // skip the reference name
            _reader.ReadString();

            _intervalListPosition = _reader.ReadInt64();

            CheckGuard();

            LoadChromosomeIntervals();
            IsInitialized = true;
        }

        public DataSourceVersion GetDataSourceVersion()
        {
            return _version;
        }

        public GenomeAssembly GetGenomeAssembly()
        {
            return _genomeAssembly;
        }

        private void CheckGuard()
        {
            var observedGuard = _reader.ReadUInt32();
            if (observedGuard != CacheConstants.GuardInt)
            {
                throw new GeneralException($"Expected a guard integer ({SupplementaryAnnotationCommon.GuardInt}), but found another value: ({observedGuard})");
            }
        }

        private void LoadChromosomeIntervals()
        {
            var currentPos = _reader.BaseStream.Position;

            _reader.BaseStream.Position = _intervalListPosition;

            CheckGuard();

            var intervalCount = _reader.ReadInt32();

            for (var i = 0; i < intervalCount; i++)
            {
                var chromosomeInterval = new PhylopInterval(_reader);
                _phylopIntervals.Add(chromosomeInterval);
            }

            _reader.BaseStream.Position = currentPos;
        }

        public int ReadIntervalScores(PhylopInterval interval)
        {
            if (interval == null) return 0;

            var filePosition = interval.FilePosition;
            //going to the file location that contains this interval.
            if (filePosition != -1)
                _reader.BaseStream.Position = filePosition;
            else return 0; //the interval does not contain any file location for the scores

            var length = _reader.ReadInt32();
            var compressedScores = _reader.ReadBytes(length);

            int requiredBufferSize = _qlz.GetDecompressedLength(compressedScores, length);
            if (requiredBufferSize > _scoreBytes.Length) _scoreBytes = new byte[requiredBufferSize];

            var uncompressedLength = _qlz.Decompress(compressedScores, length, _scoreBytes, _scoreBytes.Length);

            BytesToScores(uncompressedLength, _scoreBytes, _scores);

            _scoreCount = uncompressedLength / 2;
            return _scoreCount;
        }

        internal void BytesToScores(int uncompressedLength, byte[] uncompressedScores, short[] scores)
        {
            for (var i = 0; i < uncompressedLength / 2; i++)
            {
                scores[i] = BitConverter.ToInt16(uncompressedScores, 2 * i);
            }
        }

        private int FindInterval(int position)
        {
            if (_currentIntervalIndex != -1)
            {
                var interval = _phylopIntervals[_currentIntervalIndex];
                if (interval.ContainsPosition(position))
                    return _currentIntervalIndex;
            }

            var pointInterval = new PhylopInterval(position, 1, 1);// overload construtor for points

            var containingIntervalIndex = _phylopIntervals.BinarySearch(pointInterval);
            // The zero-based index of item in the sorted List<T>, if item is found; otherwise, a negative number that is the bitwise complement of the index of the next element that is larger than item or, if there is no larger element, the bitwise complement of Count.
            if (containingIntervalIndex == -1)
            {
                // smaller than the first position of the first interval
                return -1;// empty string represe
            }

            if (containingIntervalIndex < 0) containingIntervalIndex = ~containingIntervalIndex - 1;

            return containingIntervalIndex;
        }

        private void LoadScores(int intervalIndex)
        {
            _currentIntervalIndex = intervalIndex;

            if (intervalIndex == -1) return;

            ReadIntervalScores(_phylopIntervals[_currentIntervalIndex]);
        }

        public string GetScore(int position)
        {
            //locate the interval that contains this position
            var intervalIndex = FindInterval(position);
            if (_currentIntervalIndex != intervalIndex)
                LoadScores(intervalIndex);

            if (_currentIntervalIndex == -1)
                return null;

            var interval = _phylopIntervals[_currentIntervalIndex];

            if (_scoreCount == 0 || !interval.ContainsPosition(position))
                return null;

            var score = _scores[position - interval.Begin];

            return (score * 0.001).ToString(CultureInfo.InvariantCulture);
        }

        internal short[] GetAllScores()
        {
            return _scores;
        }

        public void Clear()
        {
            IsInitialized = false;
            _currentIntervalIndex = -1;
            _phylopIntervals.Clear();
        }
    }
}
