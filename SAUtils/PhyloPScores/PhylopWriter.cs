using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.Compression;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.Phylop;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;

namespace SAUtils.PhyloPScores
{
    /// <summary>
    /// Reads the wigFix files and creates a list of nirvana phylop database objects (one object per file). 
    /// </summary>
    public sealed class PhylopWriter : IDisposable 
    {
	    #region members

	    private string _refSeqName;
        private readonly StreamReader _reader;
        private BinaryWriter _writer;
	    private long _intervalListOffset = -1;
	    private long _intervalListPosition = -1;
	    private readonly int _intervalLength;

	    private readonly byte[] _scoreBytes;
	    private readonly short[] _scores;
	    private int _scoreCount;
	    private readonly byte[] _compressedBuffer;
	    

		internal readonly List<PhylopInterval> ChromosomeIntervals;
	    private PhylopInterval _currentInterval;

        #endregion

        #region IDisposable

        private bool _isDisposed;
		private readonly string _outputNirvanaDirectory;
	    private readonly DataSourceVersion _version;
	    private readonly GenomeAssembly _genomeAssembly;

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
			//write out the last interval
			if (_currentInterval != null)
				WriteInterval(_currentInterval, _writer);
			//write the true value of intervalListPosition
			_intervalListPosition = _writer.BaseStream.Position;
			WriteIntervals();
			
			//going back to the location where we store the interval list position in file
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

			ChromosomeIntervals.Clear();
		}

		private void OpenWriter(string refSeqName)
	    {
			_refSeqName = refSeqName;

			Clear();

			var outputFileName = _outputNirvanaDirectory + Path.DirectorySeparatorChar + _refSeqName + ".npd";

			Console.WriteLine("Creating file:{0}", outputFileName);
		    _writer = new BinaryWriter(File.Open(outputFileName, FileMode.Create));

			WriteHeader();
	    }

	    //constructor
		private PhylopWriter(string refSeqName, DataSourceVersion version, GenomeAssembly genomeAssembly ,int intervalLength = PhylopCommon.MaxIntervalLength)
	    {
			_scores              = new short[intervalLength];
			_scoreBytes          = new byte[intervalLength * 2];
			_intervalLength      = intervalLength;
		    _refSeqName          = refSeqName;
			_version             = version;
			_compressedBuffer    = new byte[intervalLength * 2 + QuickLZ.CompressionOverhead];
			ChromosomeIntervals  = new List<PhylopInterval>();
			_genomeAssembly      = genomeAssembly;
	    }

		internal PhylopWriter(string refSeqName, DataSourceVersion version, GenomeAssembly genomeAssembly, short[] scores, BinaryWriter writer) : this(refSeqName, version,genomeAssembly,scores.Length)
		{
			_scores          = scores;
			_scoreCount      = scores.Length;
			_writer          = writer;

			WriteHeader();
			_currentInterval = new PhylopInterval(100, 0, 1);

		}
		//create a phylopWriter with a certain interval length and empty score buffer
		internal PhylopWriter(string refSeqName, DataSourceVersion version, GenomeAssembly genomeAssembly, int intervalLength, BinaryWriter writer) : this(refSeqName, version,genomeAssembly ,intervalLength)
		{
			_scoreCount = 0;// tbe score buffer is empty
			_writer = writer;

			WriteHeader();
			_currentInterval = new PhylopInterval(100, 0, 1);

		}

		public PhylopWriter(string inputWigFixFile, DataSourceVersion version, GenomeAssembly genomeAssembly, string outputNirvanaDirectory, int intervalLength = PhylopCommon.MaxIntervalLength) : this(null, version, genomeAssembly,intervalLength)
		{
			_version = version;
			_reader = GZipUtilities.GetAppropriateStreamReader(inputWigFixFile);
			_outputNirvanaDirectory = outputNirvanaDirectory;
		}



		public void ExtractPhylopScores()
	    {
		    using (_reader )
		    {
			    string line;
			    while ((line = _reader.ReadLine()) != null)
			    {
				    if (string.IsNullOrEmpty(line)) continue;

					if (! line.Any(char.IsLetter))
				    {
					    //this is a phylop score
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
	    }

	    private void StartNewInterval(string line)
	    {
		    var words = line.Split();
		    var chromName = words[1].Split('=')[1];

			//checking if the writer needs to be initiated/re-initiated
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

		    
		    int start  = Convert.ToInt32(words[2].Split('=')[1]);
			short step = Convert.ToInt16(words[3].Split('=')[1]);

		    _currentInterval = new PhylopInterval(start, 0, step);
	    }

	    
	    internal void AddScore(short score)
	    {

		    _scores[_scoreCount++] = score;
			
		    if (_scoreCount < _intervalLength) return;
			
			//buffer full, its time to compress and write it out to file
			var latestInterval = _currentInterval;
			WriteInterval(_currentInterval, _writer);

		    _currentInterval = new PhylopInterval(latestInterval.Begin + latestInterval.Length, 0, 1);
		    _scoreCount = 0;
	    }

	    internal void ScoresToBytes(byte[] bytes, short[] scores, int count)
	    {
		    
		    for (int i = 0; i < count; i++)
		    {
			    var rawByte = BitConverter.GetBytes(scores[i]);

			    bytes[2*i]     = rawByte[0];
			    bytes[2*i + 1] = rawByte[1];
		    }

		}

		internal void WriteInterval(PhylopInterval interval, BinaryWriter writer)
		{
			if (_scoreCount == 0) return;
			ScoresToBytes(_scoreBytes, _scores, _scoreCount);
		    var compressor = new QuickLZ();
		    var compressedSize = compressor.Compress(_scoreBytes, _scoreCount*2 , _compressedBuffer, _compressedBuffer.Length);

		    // finalizing the file position for this chromosome interval
		    interval.FilePosition = _writer.BaseStream.Position;
			_currentInterval.Length = _scoreCount;
			ChromosomeIntervals.Add(interval);

			_scoreCount = 0;

			WriteScores(writer, _compressedBuffer, compressedSize);
	    }

		private void WriteScores(BinaryWriter writer, byte[] compressedBuffer, int length)
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
			if (_version==null)
				throw new MissingFieldException("Phylop data version cannot be null");
			_version.Write(_writer);

			

			_writer.Write(_refSeqName);

			//space holder for chromosome interval list position
	        _intervalListOffset = _writer.BaseStream.Position;
			_writer.Write(_intervalListPosition);// this is just a temp value. We will come back and write the real one before closing
			_writer.Write(NirvanaDatabaseCommon.GuardInt);
        }

	    private short GetPhylopShortValue(string line)
	    {
		    var phylopScore = Convert.ToDouble(line); // double.parse
		    if (phylopScore*1000 > 32767 || phylopScore*1000 < -32768)
		    {
			    throw new InvalidDataException("PhyloP score beyond int16 range. Score:" + phylopScore);
		    }
		    var phylopShortValue = Convert.ToInt16(phylopScore*1000);
		    return phylopShortValue;
	    }


	    private void WriteIntervals()
		{
			_writer.Write(NirvanaDatabaseCommon.GuardInt);
			if (ChromosomeIntervals != null)
				_writer.Write(ChromosomeIntervals.Count);
			else
			{
				_writer.Write(0);
				return;
			}
			foreach (var chromosomeInterval in ChromosomeIntervals)
			{
				chromosomeInterval.Write(_writer);
			}
		}

		        
    }
}
