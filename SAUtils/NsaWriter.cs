using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine.Utilities;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using Genome;
using IO;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.SA;
using Variants;

namespace SAUtils
{
    public sealed class NsaWriter : IDisposable
    {
        private readonly ExtendedBinaryWriter _writer;
        private readonly ExtendedBinaryWriter _indexWriter;
        private readonly Stream _stream;
        private readonly Stream _indexStream;

        private readonly byte[] _memBuffer;
        private readonly MemoryStream _memStream;
        private readonly ExtendedBinaryWriter _memWriter;

        private readonly NsaBlock _block;
        private readonly NsaIndex _index;
        private readonly bool _isPositional;
        private readonly bool _skipIncorrectRefEntries;
        private readonly bool _throwErrorOnConflicts;
        private readonly ISequenceProvider _refProvider;
        private readonly bool _leaveOpen;
        private int _count;

        private HashSet<ushort> _completedChromosomes = new HashSet<ushort>();

        public NsaWriter(Stream nsaStream, Stream indexStream, IDataSourceVersion version, ISequenceProvider refProvider, string jsonKey, bool matchByAllele, bool isArray, int schemaVersion, bool isPositional, bool skipIncorrectRefEntries= true, bool throwErrorOnConflicts = false, int blockSize = SaCommon.DefaultBlockSize, GenomeAssembly assembly= GenomeAssembly.Unknown, bool leaveOpen=false)
        {
            _stream                  = nsaStream;
            _indexStream             = indexStream;
            _writer                  = new ExtendedBinaryWriter(_stream,System.Text.Encoding.Default, leaveOpen);
            _indexWriter             = new ExtendedBinaryWriter(_indexStream,System.Text.Encoding.Default, leaveOpen);
            _isPositional            = isPositional;
            _skipIncorrectRefEntries = skipIncorrectRefEntries;
            _throwErrorOnConflicts   = throwErrorOnConflicts;
            _refProvider             = refProvider;
            _leaveOpen = leaveOpen;

            assembly = _refProvider?.Assembly ?? assembly;

            _block     = new NsaBlock(new Zstandard(), blockSize);
            _index     = new NsaIndex(_indexWriter, assembly, version, jsonKey, matchByAllele, isArray, schemaVersion, isPositional);
            _memBuffer = new byte[blockSize];
            _memStream = new MemoryStream(_memBuffer);
            _memWriter = new ExtendedBinaryWriter(_memStream);
        }

        internal void Write(ushort chromIndex, NsaReader nsaReader)
        {
            if (nsaReader == null) return;

            var dataBlocks  = nsaReader.GetCompressedBlocks(chromIndex);
            var indexBlocks = nsaReader.GetIndexBlocks(chromIndex);

            var i = 0;//index of the index Blocks
            //cannot convert the dataBlocks into a list since that may take up GBs of memory (proportional to the nas file size)
            foreach (var dataBlock in dataBlocks) {
                if (i > indexBlocks.Count) throw new IndexOutOfRangeException("Nsa Index have less blocks than the Nsa file. They have to be the same.");

                var oldIndexBlock = indexBlocks[i];
                _index.Add(chromIndex, oldIndexBlock.Start, oldIndexBlock.End, _writer.BaseStream.Position, oldIndexBlock.Length);
                dataBlock.WriteCompressedBytes(_writer);
                i++;
            }
            if (i < indexBlocks.Count) throw new IndexOutOfRangeException("Nsa Index have more blocks than the Nsa file. They have to be the same.");
        }

        public int Write(IEnumerable<ISupplementaryDataItem> saItems)
        { 
            var itemsMinHeap = new MinHeap<ISupplementaryDataItem>(SuppDataUtilities.CompareTo);
            var chromIndex = ushort.MaxValue;
            var currentEnsemblName = "";
            _count = 0;
            var benchmark = new Benchmark();

            foreach (var saItem in saItems)
            {
                if (chromIndex != saItem.Chromosome.Index)
                {
                    if (chromIndex != ushort.MaxValue)
                    {
                        _completedChromosomes.Add(chromIndex); // this chrom is done
                        //flushing out the remaining items in buffer
                        WriteUptoPosition(itemsMinHeap, int.MaxValue);
                        Flush(chromIndex);
                        Console.WriteLine($"Chromosome {currentEnsemblName} completed in {Benchmark.ToHumanReadable(benchmark.GetElapsedTime())}");
                        benchmark.Reset();
                    }
                    chromIndex = saItem.Chromosome.Index;
                    currentEnsemblName = saItem.Chromosome.EnsemblName;
                    _refProvider.LoadChromosome(saItem.Chromosome);
                }

                if (_completedChromosomes.Contains(saItem.Chromosome.Index))
                {
                    throw new UserErrorException(
                        $"The input file is not sorted by chromosomes. {saItem.Chromosome.UcscName} is observed in multiple segments." +
                        $"\nInput Line:\n{saItem.InputLine}");
                }

                // the items come in sorted order of the pre-trimmed position. 
                // So when writing out, we have to make sure that we do not write past this position. 
                // Once a position has been seen in the stream, we can safely write all positions before that.
                var writeToPos = saItem.Position;
                
                // if variant is in par region, we allow N's in ref
                if (RegionUtilities.OverlapsParRegion(saItem, _refProvider.Assembly)
                    && !string.IsNullOrEmpty(saItem.RefAllele) 
                    && saItem.RefAllele.All(x=> x=='N' || x=='n'))
                {
                    itemsMinHeap.Add(saItem);
                    // in order to allow room for left shifted variants, we hold off on removing them from the heap
                    WriteUptoPosition(itemsMinHeap, writeToPos - VariantUtils.MaxUpstreamLength);
                    continue;
                }
                string refSequence = _refProvider.Sequence.Substring(saItem.Position - 1, saItem.RefAllele.Length);
                if (!string.IsNullOrEmpty(saItem.RefAllele) && saItem.RefAllele != refSequence)
                {
                    if (_skipIncorrectRefEntries) continue;
                    throw new UserErrorException($"The provided reference allele {saItem.RefAllele} at {saItem.Chromosome.UcscName}:{saItem.Position} is different from {refSequence} in the reference genome sequence." +
                                                 $"\nInput Line:\n {saItem.InputLine}");
                }

                itemsMinHeap.Add(saItem);
                // in order to allow room for left shifted variants, we hold off on removing them from the heap
                WriteUptoPosition(itemsMinHeap, writeToPos- VariantUtils.MaxUpstreamLength);
            }

            //flushing out the remaining items in buffer
            WriteUptoPosition(itemsMinHeap, int.MaxValue);
            Flush(chromIndex);
            Console.WriteLine($"Chromosome {currentEnsemblName} completed in {Benchmark.ToHumanReadable(benchmark.GetElapsedTime())}");
            
            
            return _count;
        }

        private void WriteUptoPosition(MinHeap<ISupplementaryDataItem> itemsHeap, int position)
        {
            if (position < 1) return;
            if (itemsHeap.Count() == 0) return;
            var bufferMin = itemsHeap.GetMin();

            while (bufferMin.Position < position)
            {
                var itemsAtMinPosition = new List<ISupplementaryDataItem>();

                while (itemsHeap.Count() > 0 && SuppDataUtilities.CompareTo(bufferMin, itemsHeap.GetMin()) == 0)
                    itemsAtMinPosition.Add(itemsHeap.ExtractMin());

                if (itemsAtMinPosition.Count > 0)
                {
                    _count += itemsAtMinPosition.Count;
                    WritePosition(itemsAtMinPosition);
                }
                if (itemsHeap.Count() == 0) break;

                bufferMin = itemsHeap.GetMin();
            }

        }

        private void WritePosition(List<ISupplementaryDataItem> items)
        {
            int position = items[0].Position;
            _memStream.Position = 0;
            if (_isPositional)
            {
                var positionalItem = SuppDataUtilities.GetPositionalAnnotation(items);
                if (positionalItem == null) return;
                _memWriter.Write(positionalItem.GetJsonString());
            }
            else
            {
                // any data source that is reported by allele and is not an array (e.g. allele frequencies) need this filtering step
                if (_index.MatchByAllele && !_index.IsArray)
                    items = SuppDataUtilities.RemoveConflictingAlleles(items, _throwErrorOnConflicts);

                if (_index.JsonKey == SaCommon.PrimateAiTag)
                    items = SuppDataUtilities.DeDuplicatePrimateAiItems(items);

                _memWriter.WriteOpt(items.Count);

                foreach (ISupplementaryDataItem saItem in items)
                {
                    _memWriter.WriteOptAscii(saItem.RefAllele);
                    _memWriter.WriteOptAscii(saItem.AltAllele);
                    _memWriter.Write(saItem.GetJsonString());
                }
            }

            int numBytes = (int)_memStream.Position;
            if (!_block.HasSpace(numBytes)) Flush(items[0].Chromosome.Index);
            _block.Add(_memBuffer, numBytes, position);
        }

        private void Flush(ushort chromIndex)
        {

            if (_block.BlockOffset == 0) return;

            long fileOffset = _stream.Position;
            (int firstPosition, int lastPosition, int numBytes) = _block.Write(_writer);
            _block.Clear();
            _index.Add(chromIndex, firstPosition, lastPosition, fileOffset, numBytes);
        }

        public void Dispose()
        {
            _index.Write();

            if (!_leaveOpen)
            {
                _writer?.Dispose();
                _indexWriter?.Dispose();
                _stream?.Dispose();
                _indexStream?.Dispose();
                _block?.Dispose();
            }
            
            _memWriter?.Dispose();
            _memStream?.Dispose();
        }
    }
}