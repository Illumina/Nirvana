using System;
using System.Collections.Generic;
using System.IO;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using IO;
using IO.v2;
using SAUtils.GenericScore.GenericScoreParser;
using VariantAnnotation.GenericScore;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.SA;


namespace SAUtils.GenericScore
{
    public sealed class ScoreFileWriter : IDisposable
    {
        private readonly ExtendedBinaryWriter _writer;
        private readonly ExtendedBinaryWriter _indexWriter;
        private readonly ScoreIndex           _index;
        private readonly ScoreBlock           _block;
        private readonly WriterSettings       _writerSettings;
        private readonly ISequenceProvider    _refProvider;

        private readonly bool _leaveOpen;
        private readonly bool _skipIncorrectRefEntries;

        public ScoreFileWriter(
            WriterSettings writerSettings,
            Stream stream,
            Stream indexStream,
            IDataSourceVersion version,
            ISequenceProvider refProvider,
            int schemaVersion,
            bool skipIncorrectRefEntries = true,
            bool leaveOpen = false)
        {
            _leaveOpen               = leaveOpen;
            _skipIncorrectRefEntries = skipIncorrectRefEntries;
            _refProvider             = refProvider;
            _writerSettings          = writerSettings;

            var readerSettings = new ReaderSettings(
                _writerSettings.IsPositional,
                _writerSettings.EncoderType,
                _writerSettings.ScoreEncoder,
                _writerSettings.ScoreJsonEncoder,
                _writerSettings.Nucleotides,
                _writerSettings.BlockLength
            );

            _writer      = new ExtendedBinaryWriter(stream,      System.Text.Encoding.Default, _leaveOpen);
            _indexWriter = new ExtendedBinaryWriter(indexStream, System.Text.Encoding.Default, _leaveOpen);

            _index = new ScoreIndex(
                _indexWriter,
                readerSettings,
                _refProvider.Assembly,
                version,
                schemaVersion,
                _writerSettings.IndexHeader,
                _writerSettings.FilePairId
            );
            _block = new ScoreBlock(
                new Zstandard(),
                _index.GetBlockLength()
            );
        }

        private long FilePosition => _writer.BaseStream.Position;

        private void WriteHeader()
        {
            _writerSettings.Header.Write(_writer);
            _writer.WriteOpt(_writerSettings.FilePairId);
            _writer.Write(SaCommon.GuardInt);
        }

        public void Write(IEnumerable<GenericScoreItem> saItems)
        {
            WriteHeader();

            uint nucleotideSize  = _index.GetNucleotideCount() * _writerSettings.ScoreEncoder.BytesRequired;
            var  nucleotideArray = new byte[nucleotideSize];
            Array.Fill(nucleotideArray, byte.MaxValue);

            var  chromosomeIndex            = ushort.MaxValue;
            int  chromosomeStartingPosition = -1;
            int  previousPosition           = -1;
            uint blockNumber                = 0;
            uint localBlockIndex            = 0;

            foreach (GenericScoreItem saItem in saItems)
            {
                if (chromosomeStartingPosition < 0 && previousPosition < 0)
                {
                    (chromosomeIndex, chromosomeStartingPosition) = AddNewChromosome(saItem);
                    previousPosition                              = chromosomeStartingPosition;
                }

                if (!_writerSettings.SaItemValidator.Validate(saItem, _refProvider))
                {
                    _index.TrackUnmatchedReferencePositions();
                    continue;
                }

                int previousBlockNumber = _index.GetLastBlockNumber(chromosomeIndex);

                int    position     = saItem.Position;
                byte[] encodedScore = _writerSettings.ScoreEncoder.EncodeToBytes(saItem.Score);

                // Still on the same chromosome and postion, hence just fill the nucleotide array only
                if (chromosomeIndex == saItem.Chromosome.Index && position == previousPosition)
                {
                    // Write 4 {A,C,T,G} score values to nucleotide array
                    AddEncodedScoreToNucleotideArray(nucleotideArray, saItem.AltAllele, encodedScore);
                    continue;
                }

                (blockNumber, localBlockIndex) = PositionToBlockLocation(previousPosition, chromosomeStartingPosition);

                // Handle empty blocks by skipping them and adding them to index
                if (blockNumber - previousBlockNumber > 1)
                {
                    // Finalize previous memory buffer before writing empty blocks (creats an additional block)
                    WriteToDiskAndUpdateIndex(chromosomeIndex);

                    // write blockNumber - previousBlockNumber - 2 blank blocks and write them to disk
                    int blankBlockCount = (int) blockNumber - previousBlockNumber - 2;
                    WriteBlankBlocks(chromosomeIndex, blankBlockCount);
                }

                // Add nucleotide array to memory at appropriate index
                _block.Add(localBlockIndex, nucleotideArray, nucleotideSize);

                // writeout if memory buffer is full
                if (_block.IsFull())
                {
                    WriteToDiskAndUpdateIndex(chromosomeIndex);
                }

                Array.Fill(nucleotideArray, byte.MaxValue);
                AddEncodedScoreToNucleotideArray(nucleotideArray, saItem.AltAllele, encodedScore);

                // A new chromosome
                if (chromosomeIndex != saItem.Chromosome.Index)
                {
                    WriteToDiskAndUpdateIndex(chromosomeIndex);
                    (chromosomeIndex, chromosomeStartingPosition) = AddNewChromosome(saItem);
                }

                previousPosition = position;
            }

            // Writeout the partial block at the end
            (_, localBlockIndex) = PositionToBlockLocation(previousPosition, chromosomeStartingPosition);
            _block.Add(localBlockIndex, nucleotideArray, nucleotideSize);
            WriteToDiskAndUpdateIndex(chromosomeIndex);

            _writer.Write(Header.NirvanaFooter);

            //Write Index to disk
            _index.Write();
        }

        private void AddEncodedScoreToNucleotideArray(byte[] nucleotideArray, string allele, byte[] encodedScore)
        {
            ushort? nucleotidePosition = _index.GetNucleotidePosition(allele);
            if (nucleotidePosition == null) return;

            Array.Copy(
                encodedScore,
                0,
                nucleotideArray,
                (ushort) nucleotidePosition,
                encodedScore.Length
            );
        }

        private (ushort chromosomeIndex, int chromosomeStartingPosition) AddNewChromosome(GenericScoreItem saItem)
        {
            ushort chromosomeIndex            = saItem.Chromosome.Index;
            int    chromosomeStartingPosition = saItem.Position;
            _refProvider.LoadChromosome(saItem.Chromosome);
            _index.AddChromosomeBlock(chromosomeIndex, chromosomeStartingPosition);
            return (chromosomeIndex, chromosomeStartingPosition);
        }

        private void WriteBlankBlocks(ushort chromosomeIndex, int blankBlockCount)
        {
            for (var i = 0; i < blankBlockCount; i++)
            {
                AddBlockToIndex(chromosomeIndex, -1, 0, 0);
            }
        }

        /// <summary>
        /// Write the memory buffer to disk,
        /// Add the block to index
        /// Clear out the memory buffer
        /// </summary>
        /// <param name="chromosomeIndex"></param>
        private void WriteToDiskAndUpdateIndex(ushort chromosomeIndex)
        {
            long filePosition = FilePosition;
            (uint uncompressedSize, int compressedSize) = _block.Write(_writer);
            AddBlockToIndex(chromosomeIndex, filePosition, compressedSize, uncompressedSize);
        }

        private void AddBlockToIndex(ushort chromosomeIndex, long fileStartingPosition, int compressedSize, uint uncompressedSize)
        {
            _index.Add(chromosomeIndex, fileStartingPosition, compressedSize, uncompressedSize);
        }

        private (uint blockNumber, uint localBlockIndex) PositionToBlockLocation(int position, int startingPosition)
        {
            // Position is less than start position
            if (position < startingPosition) throw new UserErrorException("The Positions are not in order");
            return ((uint blockNumber, uint localBlockIndex)) _index.PositionToBlockLocation(position, startingPosition);
        }

        public void Dispose()
        {
            if (_leaveOpen) return;
            _writer?.Dispose();
            _indexWriter?.Dispose();
        }
    }
}