using System;
using System.Buffers;
using System.IO;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using IO.v2;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.SA;

namespace VariantAnnotation.GenericScore
{
    public sealed class ScoreReader : ISaMetadata
    {
        private const    int                  FileFormatVersion = 1;
        private readonly ExtendedBinaryReader _reader;
        public           GenomeAssembly       Assembly { get; }
        private readonly ScoreIndex           _index;
        public           IDataSourceVersion   Version { get; }
        public           string               JsonKey { get; }

        private readonly ICompressionAlgorithm _compressionAlgorithm = new Zstandard();

        private readonly byte[] _uncompressedBlock;
        private readonly byte[] _compressedBlock;

        private          long? _lastFileLocation;
        private readonly int   _encodedScoreSize;

        private ScoreReader(ScoreIndex scoreIndex, ExtendedBinaryReader dataFileReader)
        {
            _index  = scoreIndex;
            _reader = dataFileReader;

            Assembly = _index.Assembly;
            Version  = _index.Version;
            JsonKey  = _index.ReaderSettings.ScoreJsonEncoder.JsonKey;

            if (_index.SchemaVersion != SaCommon.SchemaVersion)
                throw new UserErrorException(
                    $"SA schema version mismatch. Expected {SaCommon.SchemaVersion}, observed {_index.SchemaVersion} for {JsonKey}");

            _encodedScoreSize  = _index.ReaderSettings.BytesRequired;
            _uncompressedBlock = ArrayPool<byte>.Shared.Rent(_index.GetBlockLength());

            int compressedBlockSize = _compressionAlgorithm.GetCompressedBufferBounds(_index.GetBlockLength());
            _compressedBlock = ArrayPool<byte>.Shared.Rent(compressedBlockSize);
        }

        public static ScoreReader Read(Stream dataStream, Stream indexStream)
        {
            var        dataFileReader = new ExtendedBinaryReader(dataStream);
            int        filePairId     = ReadHeader(dataFileReader);
            ScoreIndex index          = ScoreIndex.Read(indexStream, filePairId);

            return new ScoreReader(index, dataFileReader);
        }

        private static void CheckHeader(Header header)
        {
            (FileType fileType, ushort fileFormatVersion) = header;
            if (fileType != FileType.GsaWriter || fileFormatVersion != FileFormatVersion)
            {
                throw new UserErrorException(
                    $"The file type {fileType} version {fileFormatVersion} is not supported by this reader " +
                    $"{FileType.GsaWriter} version {FileFormatVersion}."
                );
            }
        }

        private static int ReadHeader(ExtendedBinaryReader dataFileReader)
        {
            Header header = Header.Read(dataFileReader);
            CheckHeader(header);
            int  filePairId = dataFileReader.ReadOptInt32();
            uint guardInt   = dataFileReader.ReadUInt32();

            if (guardInt != SaCommon.GuardInt)
            {
                throw new UserErrorException("The data file may be corrupted");
            }

            return filePairId;
        }


        private bool GetUncompressedBlock(ushort chromIndex, int position)
        {
            long fileLocation = _index.GetFilePosition(chromIndex, position);

            if (fileLocation < 0) return false;

            // Reuse the current block
            if (_lastFileLocation == fileLocation) return true;

            _lastFileLocation = fileLocation;

            Array.Clear(_uncompressedBlock, 0, _uncompressedBlock.Length);
            _reader.BaseStream.Position = fileLocation;

            int blockNumber = _index.GetBlockNumber(chromIndex, position);
            int bytesToRead = _index.GetBytesToRead(chromIndex, blockNumber);
            _reader.BaseStream.Read(_compressedBlock, 0, bytesToRead);

            _compressionAlgorithm.Decompress(_compressedBlock, bytesToRead, _uncompressedBlock, _index.GetBlockLength());
            return true;
        }

        public double GetScore(ushort chromosomeIndex, int position, string allele)
        {
            if (!GetUncompressedBlock(chromosomeIndex, position)) return double.NaN;

            (_, int localBlockIndex) = _index.PositionToBlockLocation(chromosomeIndex, position);
            ushort? allelePosition = _index.GetNucleotidePosition(allele);
            if (allelePosition == null) return double.NaN;

            Span<byte> score = _uncompressedBlock.AsSpan(localBlockIndex + (ushort) allelePosition, _encodedScoreSize);
            return _index.ReaderSettings.ScoreEncoder.DecodeFromBytes(score);
        }

        public string GetAnnotationJson(ushort chromosomeIndex, int position, string altAllele)
        {
            double score = GetScore(chromosomeIndex, position, altAllele);
            return double.IsNaN(score) ? null : _index.ReaderSettings.ScoreJsonEncoder.JsonRepresentation(score);
        }
    }
}