using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Utilities;
using Compression.Algorithms;
using ErrorHandling.Exceptions;
using IO;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;
using Variants;

namespace SAUtils
{
    public sealed class NsaWriter : IDisposable
    {
        private readonly ExtendedBinaryWriter _writer;
        private readonly Stream _stream;

        private readonly byte[] _memBuffer;
        private readonly MemoryStream _memStream;
        private readonly ExtendedBinaryWriter _memWriter;

        private readonly NsaBlock _block;
        private readonly ChunkedIndex _index;
        private readonly bool _isPositional;
        private readonly bool _skipIncorrectRefEntries;
        private readonly bool _throwErrorOnConflicts;
        private readonly ISequenceProvider _refProvider;
        private int _count;


        //todo: filter chromIndex=ushort.Max
        public NsaWriter(ExtendedBinaryWriter writer, ExtendedBinaryWriter indexWriter, DataSourceVersion version, ISequenceProvider refProvider, string jsonKey, bool matchByAllele, bool isArray, int schemaVersion, bool isPositional, bool skipIncorrectRefEntries= true, bool throwErrorOnConflicts = false, int blockSize = SaCommon.DefaultBlockSize)
        {
            _stream            = writer.BaseStream;
            _writer            = writer;
            _isPositional      = isPositional;
            _skipIncorrectRefEntries = skipIncorrectRefEntries;
            _throwErrorOnConflicts = throwErrorOnConflicts;
            _block = new NsaBlock(new Zstandard(), blockSize);
            _refProvider = refProvider;

            _index = new ChunkedIndex(indexWriter, refProvider.Assembly, version, jsonKey, matchByAllele, isArray, schemaVersion, isPositional);
            _memBuffer = new byte[short.MaxValue * 2];
            _memStream = new MemoryStream(_memBuffer);
            _memWriter = new ExtendedBinaryWriter(_memStream);
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

                // the items come in sorted order of the pre-trimmed position. 
                // So when writing out, we have to make sure that we do not write past this position. 
                // Once a position has been seen in the stream, we can safely write all positions before that.
                var writeToPos = saItem.Position;
                
                string refSequence = _refProvider.Sequence.Substring(saItem.Position - 1, saItem.RefAllele.Length);
                if (!string.IsNullOrEmpty(saItem.RefAllele) && saItem.RefAllele != refSequence)
                {
                    if (_skipIncorrectRefEntries) continue;
                    throw new UserErrorException($"The provided reference allele {saItem.RefAllele} at {saItem.Chromosome.UcscName}:{saItem.Position} is different from {refSequence} in the reference genome sequence.");
                }

                itemsMinHeap.Add(saItem);
                // in order to allow room for left shifted variants, we hold off on removing them from the heap
                WriteUptoPosition(itemsMinHeap, writeToPos- VariantUtils.MaxUpstreamLength);
            }

            //flushing out the remaining items in buffer
            WriteUptoPosition(itemsMinHeap, int.MaxValue);
            Flush(chromIndex);
            Console.WriteLine($"Chromosome {currentEnsemblName} completed in {Benchmark.ToHumanReadable(benchmark.GetElapsedTime())}");

            _index.Write();
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

        private void WritePosition(List<ISupplementaryDataItem> saItems)
        {
            int position = saItems[0].Position;
            _memStream.Position = 0;
            if (_isPositional)
            {
                var positionalItem = SuppDataUtilities.GetPositionalAnnotation(saItems);
                if (positionalItem == null) return;
                _memWriter.Write(positionalItem.GetJsonString());
            }
            else
            {
                // any data source that is reported by allele and is not an array (e.g. allele frequencies) need this filtering step
                if (_index.MatchByAllele && !_index.IsArray)
                    saItems = SuppDataUtilities.RemoveConflictingAlleles(saItems, _throwErrorOnConflicts);

                _memWriter.WriteOpt(saItems.Count);

                foreach (ISupplementaryDataItem saItem in saItems)
                {
                    _memWriter.WriteOptAscii(saItem.RefAllele);
                    _memWriter.WriteOptAscii(saItem.AltAllele);
                    _memWriter.Write(saItem.GetJsonString());
                }
            }

            int numBytes = (int)_memStream.Position;
            if (!_block.HasSpace(numBytes)) Flush(saItems[0].Chromosome.Index);
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
            _writer?.Dispose();
            _stream?.Dispose();
            _memWriter?.Dispose();
            _memStream?.Dispose();
        }
    }
}