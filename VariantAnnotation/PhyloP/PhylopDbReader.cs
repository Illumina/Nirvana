using System;
using System.Collections.Generic;
using System.IO;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace VariantAnnotation.PhyloP
{
    public sealed class PhylopDbReader : IDisposable
    {
        private readonly string _saDirectory;
        private ExtendedBinaryReader _reader;
        private readonly ICompressionAlgorithm _qlz;

        // in wigFix files, values for all positions are not available.
        // They come as contiguous intervals and knowing the file location of the score of a particular location is not possible.
        // This limitation motivated creation of the npd binary file format.
        // This list is our data structure that helps us to determine file location of the score of a chromosome location.
        private readonly List<PhylopInterval> _phylopIntervals;
        private int _currentIntervalIndex;
        private long _intervalListPosition;
        private readonly short[] _scores;
        private int _scoreCount;
        private byte[] _scoreBytes;

        private string _currentReferenceName;

        private IDataSourceVersion _version;
        private GenomeAssembly _genomeAssembly;

        public GenomeAssembly GenomeAssembly => PhylopCommon.GetGenomeAssembly(_saDirectory);

        public IEnumerable<IDataSourceVersion> DataSourceVersions => PhylopCommon.GetDataSourceVersions(_saDirectory);

        private PhylopDbReader()
        {
            _phylopIntervals      = new List<PhylopInterval>();
            _currentIntervalIndex = -1;

            _scores     = new short[PhylopCommon.MaxIntervalLength];
            _scoreBytes = new byte[PhylopCommon.MaxIntervalLength * 2];
            _qlz        = new QuickLZ();
        }

        public PhylopDbReader(Stream stream) : this()
        {
            if (stream == null) return;
            _reader = new ExtendedBinaryReader(stream);
            LoadHeader();
        }

        public PhylopDbReader(IEnumerable<string> saDirectories) : this()
        {
	        _saDirectory = null;

	        foreach (string saDir in saDirectories)
	        {
				var phylopFiles = Directory.GetFiles(saDir, "*.npd");
		        if (phylopFiles.Length > 0)
		        {
			        _saDirectory = saDir;
					break;
		        }
			}
        }

        public void LoadChromosome(string ucscReferenceName)
        {
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
            string identifier = _reader.ReadString();
            if (identifier != PhylopCommon.Header)
                throw new InvalidDataException("Unrecognized file header: " + identifier);

            short schemaVersion = _reader.ReadInt16();
            if (schemaVersion != PhylopCommon.SchemaVersion)
                throw new InvalidDataException("Expected phylop schema version:" + PhylopCommon.SchemaVersion + " observed schema version: " + schemaVersion);

            short dataVersion = _reader.ReadInt16();
            if (dataVersion != PhylopCommon.DataVersion)
                Console.WriteLine("WARNING: Expected phylop data version:" + PhylopCommon.DataVersion + " observed data version: " + dataVersion);

            _genomeAssembly = (GenomeAssembly)_reader.ReadByte();
            _version        = DataSourceVersion.Read(_reader);

            // skip the reference name
            _reader.ReadString();

            _intervalListPosition = _reader.ReadInt64();

            CheckGuard();

            LoadChromosomeIntervals();
        }

        public IDataSourceVersion GetDataSourceVersion() => _version;

        public GenomeAssembly GetGenomeAssembly() => _genomeAssembly;

        private void CheckGuard()
        {
            uint observedGuard = _reader.ReadUInt32();
            if (observedGuard != CacheConstants.GuardInt)
            {
                throw new InvalidFileFormatException($"Expected a guard integer ({SaCommon.GuardInt}), but found another value: ({observedGuard})");
            }
        }

        private void LoadChromosomeIntervals()
        {
            long currentPos = _reader.BaseStream.Position;

            _reader.BaseStream.Position = _intervalListPosition;

            CheckGuard();

            int intervalCount = _reader.ReadInt32();

            for (var i = 0; i < intervalCount; i++)
            {
                var chromosomeInterval = new PhylopInterval(_reader);
                _phylopIntervals.Add(chromosomeInterval);
            }

            _reader.BaseStream.Position = currentPos;
        }

        private void ReadIntervalScores(PhylopInterval interval)
        {
            if (interval == null) return;

            long filePosition = interval.FilePosition;
            //going to the file location that contains this interval.
            if (filePosition != -1)
                _reader.BaseStream.Position = filePosition;
            else return;

            int length           = _reader.ReadInt32();
            var compressedScores = _reader.ReadBytes(length);

            int requiredBufferSize = _qlz.GetDecompressedLength(compressedScores, length);
            if (requiredBufferSize > _scoreBytes.Length) _scoreBytes = new byte[requiredBufferSize];

            int uncompressedLength = _qlz.Decompress(compressedScores, length, _scoreBytes, _scoreBytes.Length);

            BytesToScores(uncompressedLength, _scoreBytes, _scores);

            _scoreCount = uncompressedLength / 2;
        }

        private static void BytesToScores(int uncompressedLength, byte[] uncompressedScores, short[] scores)
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

            int containingIntervalIndex = _phylopIntervals.BinarySearch(pointInterval);
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

        public double? GetScore(int position)
        {
            // locate the interval that contains this position
            int intervalIndex = FindInterval(position);
            if (_currentIntervalIndex != intervalIndex)
                LoadScores(intervalIndex);

            if (_currentIntervalIndex == -1)
                return null;

            var interval = _phylopIntervals[_currentIntervalIndex];

            if (_scoreCount == 0 || !interval.ContainsPosition(position))
                return null;

            short score = _scores[position - interval.Begin];

            return score * 0.001;
        }

        public void Dispose() => _reader.Dispose();
    }
}
