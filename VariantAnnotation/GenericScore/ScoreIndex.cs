using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using IO.v2;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace VariantAnnotation.GenericScore
{
    public sealed class ScoreIndex
    {
        private readonly ExtendedBinaryWriter                _writer;
        private readonly int                                 _blockLength;
        private readonly ushort                              _scoreSize;
        private readonly byte                                _nucleotideCount;
        private readonly Dictionary<string, ushort>          _nucleotideIndexMapper;
        public readonly  GenomeAssembly                      Assembly;
        public readonly  int                                 SchemaVersion;
        private readonly Header                              _indexHeader;
        private readonly int                                 _filePairId;
        public readonly  IDataSourceVersion                  Version;
        private readonly MetaData                            _metaData;
        private          Dictionary<ushort, ChromosomeBlock> _chromosomeBlocks;
        public readonly  ReaderSettings                      ReaderSettings;

        public ScoreIndex(
            ExtendedBinaryWriter indexWriter,
            ReaderSettings readerSettings,
            GenomeAssembly assembly,
            IDataSourceVersion version,
            int schemaVersion,
            Header indexHeader,
            int filePairId
        )
        {
            _writer       = indexWriter;
            Assembly      = assembly;
            Version       = version;
            SchemaVersion = schemaVersion;
            _indexHeader  = indexHeader;
            _filePairId   = filePairId;

            ReaderSettings = readerSettings;

            _chromosomeBlocks = new Dictionary<ushort, ChromosomeBlock>();
            _metaData         = new MetaData();

            string[] nucleotides = readerSettings.Nucleotides;
            _nucleotideCount = (byte) nucleotides.Length;

            _scoreSize   = readerSettings.BytesRequired;
            _blockLength = _nucleotideCount * readerSettings.BlockLength * _scoreSize;

            // Nucleotide to position mapping
            _nucleotideIndexMapper = new Dictionary<string, ushort>();
            for (ushort i = 0; i < _nucleotideCount; i++)
            {
                _nucleotideIndexMapper[readerSettings.Nucleotides[i]] = (ushort) (i * _scoreSize);
            }
        }

        /// <summary>
        /// Add the block to index
        /// </summary>
        /// <param name="chromIndex"></param>
        /// <param name="filePosition"></param>
        /// <param name="compressedSize"></param>
        /// <param name="uncompressedSize"></param>
        public void Add(ushort chromIndex, long filePosition, int compressedSize, uint uncompressedSize)
        {
            // Create index block and add to chromosome block
            var indexBlock = new ScoreIndexBlock(filePosition, compressedSize);
            _chromosomeBlocks[chromIndex].Add(indexBlock);
            int blockNumber = GetLastBlockNumber(chromIndex);
            _metaData.AddIndexBlock(chromIndex, blockNumber, filePosition, uncompressedSize, (uint) compressedSize);
        }

        public void AddChromosomeBlock(ushort chromIndex, int chromosomeStartingPosition)
        {
            _chromosomeBlocks[chromIndex] = new ChromosomeBlock(new List<ScoreIndexBlock>(), 0, chromosomeStartingPosition);
            _metaData.AddChromosomeBlock(chromIndex);
        }

        public void TrackUnmatchedReferencePositions()
        {
            _metaData.TrackUnmatchedReferencePositions();
        }

        private void WriteHeader()
        {
            _indexHeader.Write(_writer);
            _writer.WriteOpt(_filePairId);
            _writer.Write(SaCommon.GuardInt);
        }

        private static void CheckHeader(Header header)
        {
            (FileType fileType, ushort fileFormatVersion) = header;
            if (fileType != FileType.GsaIndex)
                throw new UserErrorException($"The file type {fileType} version {fileFormatVersion} " +
                                             $"is not supported by this reader {FileType.GsaIndex}");
        }

        private static (Header indexHeader, int filePairId) ReadHeader(ExtendedBinaryReader reader, int expectedFilePairId)
        {
            Header indexHeader = Header.Read(reader);
            CheckHeader(indexHeader);
            int  filePairId = reader.ReadOptInt32();
            uint guardInt   = reader.ReadUInt32();

            if (guardInt != SaCommon.GuardInt || filePairId != expectedFilePairId)
            {
                throw new UserErrorException("Unable to read the index");
            }

            return (indexHeader, filePairId);
        }

        /// <summary>
        /// Serialize the instance to writer stream
        /// </summary>
        public void Write()
        {
            WriteHeader();
            _writer.Write((byte) Assembly);
            Version.Write(_writer);
            _writer.WriteOpt(SchemaVersion);

            _writer.WriteOpt(_chromosomeBlocks.Count);
            // Write the Chromsome Blocks
            foreach ((ushort index, ChromosomeBlock chromosomeBlocks) in _chromosomeBlocks)
            {
                _writer.WriteOpt(index);
                chromosomeBlocks.Write(_writer);
            }

            ReaderSettings.Write(_writer);

            _metaData.PrintWriteMetrics();
        }

        /// <summary>
        /// Deserialize the instance from reader stream
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="dataFilePairId"></param>
        /// <returns></returns>
        public static ScoreIndex Read(Stream stream, int dataFilePairId)
        {
            using (var memStream = new MemoryStream())
            using (var reader = new ExtendedBinaryReader(memStream))
            {
                stream.CopyTo(memStream); //reading all bytes in stream to memStream
                memStream.Position = 0;

                (Header indexHeader, int filePairId) = ReadHeader(reader, dataFilePairId);

                GenomeAssembly     assembly      = (GenomeAssembly) reader.ReadByte();
                IDataSourceVersion version       = DataSourceVersion.Read(reader);
                int                schemaVersion = reader.ReadOptInt32();

                int chromCount = reader.ReadOptInt32();

                // read the chromblocks
                var chromBlocks = new Dictionary<ushort, ChromosomeBlock>(chromCount);
                for (var i = 0; i < chromCount; i++)
                {
                    var chromIndex = reader.ReadOptUInt16();
                    chromBlocks[chromIndex] = ChromosomeBlock.Read(reader);
                }

                ReaderSettings readerSettings = ReaderSettings.Read(reader);

                var scoreIndex = new ScoreIndex(
                    null,
                    readerSettings,
                    assembly,
                    version,
                    schemaVersion,
                    indexHeader,
                    filePairId
                )
                {
                    _chromosomeBlocks = chromBlocks,
                };

                return scoreIndex;
            }
        }

        /// <summary>
        /// Return the file position of the block containing the given chromosome and chromosomal position
        /// </summary>
        /// <param name="chromIndex"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public long GetFilePosition(ushort chromIndex, int position)
        {
            if (_chromosomeBlocks == null || !_chromosomeBlocks.TryGetValue(chromIndex, out var chromosomeBlock)) return -1;
            int blockNumber = GetBlockNumber(chromosomeBlock, position);

            if (blockNumber < 0) return -1;
            return chromosomeBlock.Get(blockNumber) != null ? chromosomeBlock.Get(blockNumber).FilePosition : -1;
        }


        /// <summary>
        /// Returns the block number which would contain the position
        /// Because each block is of a known size, (e.g. 10_000)
        /// the first position (e.g 10_001) can be used to find the file position
        /// Example: 
        ///     blockNumber = (354_011 - 10_001) / 10_000 = 45th block contains the position 354_011
        /// </summary>
        /// <param name="chromosomeBlock"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        private int GetBlockNumber(ChromosomeBlock chromosomeBlock, int position)
        {
            // Position is less than start position
            if (position < chromosomeBlock.StartingPosition) return -1;

            var blockNumber = (int) ((position - chromosomeBlock.StartingPosition) * _nucleotideCount / _blockLength);

            // Position is outside the last block
            if (blockNumber >= chromosomeBlock.BlockCount) return -1;

            return blockNumber;
        }

        public int GetBlockNumber(ushort chromosomeIndex, int position)
        {
            if (_chromosomeBlocks == null || !_chromosomeBlocks.TryGetValue(chromosomeIndex, out var chromosomeBlock)) return -1;
            return GetBlockNumber(chromosomeBlock, position);
        }

        public int GetBytesToRead(ushort chromIndex, int blockNumber)
        {
            return _chromosomeBlocks[chromIndex].Get(blockNumber).BytesWritten;
        }

        public int GetBlockLength()
        {
            return _blockLength;
        }

        public uint GetNucleotideCount()
        {
            return _nucleotideCount;
        }

        public ushort? GetNucleotidePosition(string saItemAltAllele)
        {
            if (!_nucleotideIndexMapper.ContainsKey(saItemAltAllele)) return null;
            return _nucleotideIndexMapper[saItemAltAllele];
        }

        public (int blockNumber, int localBlockIndex) PositionToBlockLocation(int position, int startingPosition)
        {
            // Position is less than start position
            if (position < startingPosition) return (-1, -1);

            int deltaPosition = (position - startingPosition) * _nucleotideCount * _scoreSize;

            return (deltaPosition / _blockLength, deltaPosition % _blockLength);
        }

        public (int blockNumber, int localBlockIndex) PositionToBlockLocation(ChromosomeBlock chromosomeBlock, int position)
        {
            return PositionToBlockLocation(position, (int) chromosomeBlock.StartingPosition);
        }

        public (int blockNumber, int localBlockIndex) PositionToBlockLocation(ushort chromosomeIndex, int position)
        {
            if (_chromosomeBlocks == null || !_chromosomeBlocks.TryGetValue(chromosomeIndex, out var chromosomeBlock)) return (-1, -1);
            return PositionToBlockLocation(chromosomeBlock, position);
        }

        public Dictionary<ushort, ChromosomeBlock> GetChromosomeBlocks()
        {
            return _chromosomeBlocks;
        }

        public int GetLastBlockNumber(ushort chromosomeIndex)
        {
            return _chromosomeBlocks[chromosomeIndex].BlockCount - 1;
        }
    }
}