using System;
using System.Collections.Generic;
using CommandLine.Utilities;
using SAUtils.DataStructures;
using SAUtils.TsvWriters;

namespace SAUtils
{
    public static class TsvWriterUtilities
    {
        private static void WriteToPosition(ISaItemTsvWriter writer, MinHeap<SupplementaryDataItem> itemsHeap, int position)
        {
            if (itemsHeap.Count() == 0) return;
            var bufferMin = itemsHeap.GetMin();

            while (bufferMin.Start < position)
            {
                var itemsAtMinPosition = new List<SupplementaryDataItem>();

                while (itemsHeap.Count() > 0 && bufferMin.CompareTo(itemsHeap.GetMin()) == 0)
                    itemsAtMinPosition.Add(itemsHeap.ExtractMin());

                writer.WritePosition(itemsAtMinPosition);

                if (itemsHeap.Count() == 0) break;

                bufferMin = itemsHeap.GetMin();
            }

        }

        public static void WriteCompleteInfo(string dataSourceDescription, string version, string timeSpan)
        {

            Console.Write($"{dataSourceDescription,-20}    {version,-20}");
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine($"{timeSpan,-20}");
            Console.ResetColor();

        }
        public static void WriteSortedItems(IEnumerable<SupplementaryDataItem> saItems, ISaItemTsvWriter writer)
        {
            var itemsMinHeap = new MinHeap<SupplementaryDataItem>();
            var currentRefIndex = int.MaxValue;

            var benchmark = new Benchmark();

            foreach (var saItem in saItems)        
            {
                if (currentRefIndex != saItem.Chromosome.Index)
                {
                    if (currentRefIndex != int.MaxValue)
                    {
                        //flushing out the remaining items in buffer
                        WriteToPosition(writer, itemsMinHeap, int.MaxValue);
                        //Console.WriteLine($"Wrote out chr{currentRefIndex} items in {benchmark.GetElapsedTime()}");
                        benchmark.Reset();
                    }
                    currentRefIndex = saItem.Chromosome.Index;
                    //Console.WriteLine("Writing items from chromosome:" + currentRefIndex);
                }

                //the items come in sorted order of the pre-trimmed position. 
                //So when writing out, we have to make sure that we do not write past this position. 
                //Once a position has been seen in the stream, we can safely write all positions before that.
                var writeToPos = saItem.Start;

                saItem.Trim();
                itemsMinHeap.Add(saItem);

                WriteToPosition(writer, itemsMinHeap, writeToPos);
            }

            //flushing out the remaining items in buffer
            WriteToPosition(writer, itemsMinHeap, int.MaxValue);
        }

    }
}