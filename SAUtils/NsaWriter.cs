using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Utilities;
using Compression.Algorithms;
using Genome;
using IO;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils
{
    public sealed class NsaWriter:IDisposable
    {
        private readonly ExtendedBinaryWriter _writer;
        private readonly Stream _stream;

        private readonly byte[] _memBuffer;
        private readonly MemoryStream _memStream;
        private readonly ExtendedBinaryWriter _memWriter;

        private readonly SaWriteBlock _block;
        private readonly ChunkedIndex _index;
        private readonly bool _isPositional;
        private readonly ISequenceProvider _refProvider;


        //todo: filter chromIndex=ushort.Max
        public NsaWriter(ExtendedBinaryWriter writer, ExtendedBinaryWriter indexWriter, DataSourceVersion version, ISequenceProvider refProvider, string jsonKey, bool matchByAllele, bool isArray, int schemaVersion, bool isPositional, int blockSize= SaCommon.DefaultBlockSize)
        {
            _stream = writer.BaseStream;
            _writer = writer;
            _isPositional = isPositional;
            _block  = new SaWriteBlock(new Zstandard(), blockSize);
            _refProvider = refProvider;
        
            _index     = new ChunkedIndex(indexWriter, refProvider.Assembly, version, jsonKey, matchByAllele, isArray, schemaVersion, isPositional);
            _memBuffer = new byte[short.MaxValue*2];
            _memStream = new MemoryStream(_memBuffer);
            _memWriter = new ExtendedBinaryWriter(_memStream);
        }
        
        public void Write(IEnumerable<ISupplementaryDataItem> saItems)
        {
            var itemsMinHeap = new MinHeap<ISupplementaryDataItem>(SuppDataUtilities.CompareTo);
            var chromIndex = ushort.MaxValue;
            var currentEnsemblName = "";

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

                //if (saItem.Position== 8021911)
                //    Console.WriteLine("clinvar bug");
                if (!string.IsNullOrEmpty(saItem.RefAllele) && saItem.RefAllele != _refProvider.Sequence.Substring(saItem.Position-1, saItem.RefAllele.Length)) continue;
                //the items come in sorted order of the pre-trimmed position. 
                //So when writing out, we have to make sure that we do not write past this position. 
                //Once a position has been seen in the stream, we can safely write all positions before that.
                var writeToPos = saItem.Position;

                saItem.Trim();
                itemsMinHeap.Add(saItem);
                WriteUptoPosition(itemsMinHeap, writeToPos);

            }
            //flushing out the remaining items in buffer
            WriteUptoPosition(itemsMinHeap, int.MaxValue);
            Flush(chromIndex);
            Console.WriteLine($"Chromosome {currentEnsemblName} completed in {Benchmark.ToHumanReadable(benchmark.GetElapsedTime())}");

            _index.Write();
            
        }

        private void WriteUptoPosition(MinHeap<ISupplementaryDataItem> itemsHeap, int position)
        {
            if (itemsHeap.Count() == 0) return;
            var bufferMin = itemsHeap.GetMin();

            while (bufferMin.Position < position)
            {
                var itemsAtMinPosition = new List<ISupplementaryDataItem>();

                while (itemsHeap.Count() > 0 && SuppDataUtilities.CompareTo(bufferMin, itemsHeap.GetMin()) == 0)
                    itemsAtMinPosition.Add(itemsHeap.ExtractMin());

                if (itemsAtMinPosition.Count > 0) WritePosition(itemsAtMinPosition);
                if (itemsHeap.Count() == 0) break;

                bufferMin = itemsHeap.GetMin();
            }

        }

        private void WritePosition(List<ISupplementaryDataItem> saItems)
        {
            int position = saItems[0].Position;
            //if (position == 16558315)
            //    Console.WriteLine("bug");

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
                    SuppDataUtilities.RemoveConflictingAlleles(saItems);
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
            (int firstPosition, int lastPosition, int numBytes)= _block.Write(_stream);
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