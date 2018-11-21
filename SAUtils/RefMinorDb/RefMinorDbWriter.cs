using System;
using System.Collections.Generic;
using System.IO;
using CommandLine.Utilities;
using IO;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using VariantAnnotation.NSA;
using VariantAnnotation.Providers;
using VariantAnnotation.SA;

namespace SAUtils.RefMinorDb
{
    public sealed class RefMinorDbWriter:IDisposable
    {
        private readonly ExtendedBinaryWriter _writer;
        private readonly Stream _stream;

        private readonly ISequenceProvider _refProvider;
        private readonly RefMinorIndex _refMinorIndex;

        public RefMinorDbWriter(ExtendedBinaryWriter writer, ExtendedBinaryWriter indexWriter, DataSourceVersion version, ISequenceProvider refProvider, int schemaVersion)
        {
            _stream = writer.BaseStream;
            _writer = writer;
            _refProvider = refProvider;
            _refMinorIndex  = new RefMinorIndex(indexWriter, _refProvider.Assembly, version, schemaVersion);
            
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
                        Console.WriteLine($"Chromosome {currentEnsemblName} completed in {Benchmark.ToHumanReadable(benchmark.GetElapsedTime())}");
                        benchmark.Reset();
                    }
                    chromIndex = saItem.Chromosome.Index;
                    currentEnsemblName = saItem.Chromosome.EnsemblName;
                    _refProvider.LoadChromosome(saItem.Chromosome);
                }

                if (saItem.RefAllele != _refProvider.Sequence.Substring(saItem.Position-1, saItem.RefAllele.Length)) continue;
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
            Console.WriteLine($"Chromosome {currentEnsemblName} completed in {Benchmark.ToHumanReadable(benchmark.GetElapsedTime())}");

            _refMinorIndex.Write(_stream.Position);
            
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
                WritePosition(itemsAtMinPosition);
                if (itemsHeap.Count() == 0) break;

                bufferMin = itemsHeap.GetMin();
            }

        }

        private void WritePosition(List<ISupplementaryDataItem> saItems)
        {
            var refMinorItem = (RefMinorItem)GetRefMinorItem(saItems);

            if (refMinorItem == null) return;

            _refMinorIndex.Add(refMinorItem.Chromosome.Index, _stream.Position);
            _writer.WriteOpt(refMinorItem.Position);
            _writer.WriteOptAscii(refMinorItem.GlobalMajor);
            
        }

        private static ISupplementaryDataItem GetRefMinorItem(IList<ISupplementaryDataItem> saItems)
        {
            var totalAltAlleleFreq = 0.0;
            var alleleFrequencies = new Dictionary<string, double>();
            string refAllele = null;
            foreach (var supplementaryDataItem in saItems)
            {
                var item = (AlleleFrequencyItem) supplementaryDataItem;
                if (!IsSnv(item.RefAllele) || !IsSnv(item.AltAllele)) continue;

                refAllele = item.RefAllele;
                totalAltAlleleFreq += item.AltFrequency;
                alleleFrequencies[item.AltAllele] = item.AltFrequency;

            }
            var isRefMinor = totalAltAlleleFreq >= SaCommon.RefMinorThreshold;

            if (!isRefMinor) return null;
            string globalMajor = SuppDataUtilities.GetMostFrequentAllele(alleleFrequencies, refAllele);

            return new RefMinorItem(saItems[0].Chromosome, saItems[0].Position, globalMajor);
        }

        private static bool IsSnv(string allele)
        {
            if (allele.Length != 1) return false;

            allele = allele.ToUpper();
            return allele == "A" || allele == "C" || allele == "G" || allele == "T";
        }


        public void Dispose()
        {
            _writer?.Dispose();
            _stream?.Dispose();
        }
    }
}