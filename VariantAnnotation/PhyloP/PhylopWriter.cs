using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Algorithms;
using Compression.Utilities;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.IO;
using VariantAnnotation.IO.Caches;
using VariantAnnotation.Providers;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.PhyloP
{
    /// <summary>
    /// Reads the wigFix files and creates a list of nirvana phylop database objects (one object per file). 
    /// </summary>
    public sealed class PhylopWriter : IDisposable
    {
        #region members

        private string _refSeqName;
        private readonly StreamReader _reader;
        private ExtendedBinaryWriter _writer;
        private long _intervalListOffset = -1;
        private long _intervalListPosition = -1;
        private readonly int _intervalLength;

        private readonly byte[] _scoreBytes;
        private readonly short[] _scores;
        private int _scoreCount;
        private readonly byte[] _compressedBuffer;


        private readonly List<PhylopInterval> _chromosomeIntervals;
        private PhylopInterval _currentInterval;

        private int _maxValue = int.MinValue;
        private int _minValue = int.MaxValue;

        private readonly string _outputNirvanaDirectory;
        private readonly DataSourceVersion _version;
        private readonly GenomeAssembly _genomeAssembly;

        private readonly ICompressionAlgorithm _compressor;

        #endregion

        #region IDisposable

        private bool _isDisposed;

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

        private void Close()
        {
            if (_writer == null) return;
            //write the true value of intervalListPosition
            _intervalListPosition = _writer.BaseStream.Position;
            WriteIntervals();
            //going back to the location where we store the interval list position in file
            _writer.BaseStream.Position = _intervalListOffset;
            _writer.Write(_intervalListPosition);

            _reader?.Dispose();
            _writer?.Dispose();
        }

        #endregion

        private void CloseWriter()
        {
            // write out the last interval
            if (_currentInterval != null)
                WriteInterval(_currentInterval, _writer);

            // write the true value of intervalListPosition
            _intervalListPosition = _writer.BaseStream.Position;
            WriteIntervals();

            // going back to the location where we store the interval list position in file
            _writer.BaseStream.Position = _intervalListOffset;
            _writer.Write(_intervalListPosition);

            _writer.Dispose();
        }

        private void Clear()
        {
            _scoreCount           = 0;
            _currentInterval      = null;
            _intervalListOffset   = -1;
            _intervalListPosition = -1;

            _chromosomeIntervals.Clear();
        }

        private void OpenWriter(string refSeqName)
        {
            _refSeqName = refSeqName;

            Clear();

            var outputFileName = _outputNirvanaDirectory + Path.DirectorySeparatorChar + _refSeqName + ".npd";

            Console.WriteLine("Creating file: {0}", outputFileName);
            _writer = new ExtendedBinaryWriter(FileUtilities.GetCreateStream(outputFileName));

            WriteHeader();
        }

        /// <summary>
        /// constructor
        /// </summary>
        private PhylopWriter(string refSeqName, DataSourceVersion version, GenomeAssembly genomeAssembly,
            int intervalLength = PhylopCommon.MaxIntervalLength)
        {
            _scores             = new short[intervalLength];
            _scoreBytes         = new byte[intervalLength * 2];
            _intervalLength     = intervalLength;
            _refSeqName         = refSeqName;
            _version            = version;
            _compressor         = new QuickLZ();
            _chromosomeIntervals = new List<PhylopInterval>();
            _genomeAssembly     = genomeAssembly;

            var requiredBufferSize = _compressor.GetCompressedBufferBounds(_scoreBytes.Length);
            _compressedBuffer      = new byte[requiredBufferSize];
        }

        public PhylopWriter(string inputWigFixFile, DataSourceVersion version, GenomeAssembly genomeAssembly,
            string outputNirvanaDirectory, int intervalLength = PhylopCommon.MaxIntervalLength)
            : this(null, version, genomeAssembly, intervalLength)
        {
            _version                = version;
            _reader                 = GZipUtilities.GetAppropriateStreamReader(inputWigFixFile);
            _outputNirvanaDirectory = outputNirvanaDirectory;
        }

        public void ExtractPhylopScores()
        {
            using (_reader)
            {
                string line;
                while ((line = _reader.ReadLine()) != null)
                {
                    if (string.IsNullOrEmpty(line)) continue;

                    if (!line.Any(char.IsLetter))
                    {
                        // this is a phylop score
                        AddScore(GetPhylopShortValue(line));
                    }
                    else
                    {
                        StartNewInterval(line);
                    }
                }

                // writing the last remaining scores if any
                if (_currentInterval != null)
                    WriteInterval(_currentInterval, _writer);
            }

            Console.WriteLine($"Max observed value: {_maxValue}, Min observed value {_minValue}");
        }

        private void StartNewInterval(string line)
        {
            var words     = line.Split();
            var chromName = words[1].Split('=')[1];

            // checking if the writer needs to be initiated/re-initiated
            if (_writer == null)
            {
                OpenWriter(chromName);
            }

            if (chromName != _refSeqName)
            {
                CloseWriter();
                OpenWriter(chromName);
            }

            // dumping existing interval
            if (_currentInterval != null)
            {
                WriteInterval(_currentInterval, _writer);
            }

            var start = Convert.ToInt32(words[2].Split('=')[1]);
            var step  = Convert.ToInt16(words[3].Split('=')[1]);

            _currentInterval = new PhylopInterval(start, 0, step);
        }

        private void AddScore(short score)
        {
            UpdateMinMaxScore(score);
            _scores[_scoreCount++] = score;

            if (_scoreCount < _intervalLength) return;

            // buffer full, its time to compress and write it out to file
            var latestInterval = _currentInterval;
            WriteInterval(_currentInterval, _writer);

            _currentInterval = new PhylopInterval(latestInterval.Begin + latestInterval.Length, 0, 1);
            _scoreCount = 0;
        }

        private void UpdateMinMaxScore(short score)
        {
            if (score < _minValue) _minValue = score;
            if (score > _maxValue) _maxValue = score;
        }

        private static void ScoresToBytes(byte[] bytes, short[] scores, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var rawByte      = BitConverter.GetBytes(scores[i]);
                bytes[2 * i]     = rawByte[0];
                bytes[2 * i + 1] = rawByte[1];
            }
        }

        private void WriteInterval(PhylopInterval interval, ExtendedBinaryWriter writer)
        {
            if (_scoreCount == 0) return;
            ScoresToBytes(_scoreBytes, _scores, _scoreCount);

            var compressedSize = _compressor.Compress(_scoreBytes, _scoreCount * 2, _compressedBuffer, _compressedBuffer.Length);

            // finalizing the file position for this chromosome interval
            interval.FilePosition   = _writer.BaseStream.Position;
            _currentInterval.Length = _scoreCount;
            _chromosomeIntervals.Add(interval);

            _scoreCount = 0;

            WriteScores(writer, _compressedBuffer, compressedSize);
        }

        private static void WriteScores(ExtendedBinaryWriter writer, byte[] compressedBuffer, int length)
        {
            writer.Write(length);
            writer.Write(compressedBuffer, 0, length);
        }

        /// <summary>
        /// Writeout the header of the nirvana phylop database
        /// </summary>
        private void WriteHeader()
        {
            _writer.Write(PhylopCommon.Header);
            _writer.Write(PhylopCommon.SchemaVersion);
            _writer.Write(PhylopCommon.DataVersion);

            _writer.Write((byte)_genomeAssembly);

            if (_version == null) throw new MissingFieldException("Phylop data version cannot be null");

            _version.Write(_writer);
            _writer.Write(_refSeqName);

            // space holder for chromosome interval list position
            _intervalListOffset = _writer.BaseStream.Position;
            _writer.Write(_intervalListPosition); // this is just a temp value. We will come back and write the real one before closing
            _writer.Write(CacheConstants.GuardInt);
        }

        private static short GetPhylopShortValue(string line)
        {
            var phylopScore = Convert.ToDouble(line); // double.parse

            if (phylopScore * 1000 > 32767 || phylopScore * 1000 < -32768)
            {
                throw new InvalidDataException("PhyloP score beyond int16 range. Score:" + phylopScore);
            }

            var phylopShortValue = Convert.ToInt16(phylopScore * 1000);
            return phylopShortValue;
        }

        private void WriteIntervals()
        {
            _writer.Write(CacheConstants.GuardInt);

            if (_chromosomeIntervals != null)
                _writer.Write(_chromosomeIntervals.Count);
            else
            {
                _writer.Write(0);
                return;
            }

            foreach (var chromosomeInterval in _chromosomeIntervals)
            {
                chromosomeInterval.Write(_writer);
            }
        }
    }
}
