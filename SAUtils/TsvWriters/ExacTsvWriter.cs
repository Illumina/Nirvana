using System;
using System.Collections.Generic;
using System.IO;
using Genome;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
    public sealed class ExacTsvWriter : ISaItemTsvWriter
    {
        #region members
        private readonly SaTsvWriter _writer;
        #endregion

        public ExacTsvWriter(DataSourceVersion version, string outputDirectory, GenomeAssembly genomeAssembly, ISequenceProvider sequenceProvider)
        {
            Console.WriteLine(version.ToString());

            _writer = new SaTsvWriter(outputDirectory, version, genomeAssembly.ToString(),
                SaTsvCommon.OneKgenSchemaVersion, InterimSaCommon.ExacTag, null, true, sequenceProvider);
        }

        public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
        {
            if (saItems == null) return;

            var exacItems = new List<ExacItem>();
            foreach (var item in saItems)
            {
                if (!(item is ExacItem exacItem)) throw new InvalidDataException("Expected ExacItems list!!");
                exacItems.Add(exacItem);
            }

            SupplementaryDataItem.RemoveConflictingAlleles(exacItems);


            foreach (var exacItem in exacItems)
            {
                _writer.AddEntry(exacItem.Chromosome.EnsemblName, exacItem.Start, exacItem.ReferenceAllele, exacItem.AlternateAllele, null, new List<string> { exacItem.GetJsonString() });
            }
        }

        public void Dispose() => _writer.Dispose();
    }
}
