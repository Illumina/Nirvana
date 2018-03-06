using System;
using System.Collections.Generic;
using System.IO;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
    public sealed class ExacTsvWriter : ISaItemTsvWriter
    {
        #region members
        private readonly SaTsvWriter _writer;
        #endregion

        #region IDisposable

        private bool _disposed;

        /// <summary>
        /// public implementation of Dispose pattern callable by consumers. 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// protected implementation of Dispose pattern. 
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                _writer.Dispose();
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
            // Free any other managed objects here.

        }

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
    }
}
