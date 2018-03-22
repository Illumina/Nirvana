using System;
using System.Collections.Generic;
using System.IO;
using SAUtils.DataStructures;
using SAUtils.InputFileParsers.TOPMed;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
    public sealed class TopMedTsvWriter : ISaItemTsvWriter
    {
        private readonly SaTsvWriter _writer;
        public void Dispose()
        {
            _writer?.Dispose();
        }
        public TopMedTsvWriter(DataSourceVersion version, string outputFileName, GenomeAssembly genomeAssembly, ISequenceProvider sequenceProvider)
        {
            Console.WriteLine(version.ToString());

            _writer = new SaTsvWriter(outputFileName, version, genomeAssembly.ToString(),
                SaTsvCommon.SchemaVersion, InterimSaCommon.TopMedTag, null, true, sequenceProvider);

        }

        public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
        {
            if (saItems == null) return;

            var topMedItems = new List<TopMedItem>();
            foreach (var item in saItems)
            {
                if (!(item is TopMedItem topMedItem))
                    throw new InvalidDataException("Expected TopMedItem list!!");
                topMedItems.Add(topMedItem);
            }

            SupplementaryDataItem.RemoveConflictingAlleles(topMedItems);

            foreach (var topMedItem in topMedItems)
            {
                _writer.AddEntry(topMedItem.Chromosome.EnsemblName, topMedItem.Start, topMedItem.ReferenceAllele, topMedItem.AlternateAllele, null, new List<string> { topMedItem.GetJsonString() });
            }
        }
    }
}