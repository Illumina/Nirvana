using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.Sequence;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
    public sealed class MitoMapVarTsvWriter : ISaItemTsvWriter
    {

        #region members
        private readonly SaTsvWriter _mitoMapVarWriter;

        #endregion

        #region IDisposable
        bool _disposed;

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
                _mitoMapVarWriter.Dispose();
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
            // Free any other managed objects here.

        }
        #endregion

        public MitoMapVarTsvWriter(DataSourceVersion version, string outputDirectory, string mitoMapDataType, ISequenceProvider sequenceProvider)
        {
            Console.WriteLine(version.ToString());
            _mitoMapVarWriter = new SaTsvWriter(outputDirectory, version, GenomeAssembly.rCRS.ToString(), SaTsvCommon.MitoMapSchemaVersion, mitoMapDataType, null, false, sequenceProvider, true);
        }


        public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
        {
            if (saItems == null) return;
            var mitoMapMutItems = saItems.Select(x => x as MitoMapItem).ToList();
            var uniqueMutations = GetUniqueMutations(mitoMapMutItems);
            foreach (var mutation in uniqueMutations)
            {
                _mitoMapVarWriter.AddEntry(
                           mutation.Value[0].Chromosome.EnsemblName,
                           mutation.Value[0].Start,
                           mutation.Key.Item1,
                           mutation.Key.Item2,
                           null,
                           mutation.Value.Select(x => x.GetVariantJsonString()).Distinct().ToList());
            }
        }

        private Dictionary<(string, string), List<MitoMapItem>> GetUniqueMutations(List<MitoMapItem> mitoMapMutItems)
        {
            var uniqueMutations = new Dictionary<(string, string), List<MitoMapItem>>();

            foreach (var mitoMapMutItem in mitoMapMutItems)
            {
                var mutation = (mitoMapMutItem.ReferenceAllele, mitoMapMutItem.AlternateAllele);
                if (uniqueMutations.ContainsKey(mutation))
                    uniqueMutations[mutation].Add(mitoMapMutItem);
                else uniqueMutations[mutation] = new List<MitoMapItem> { mitoMapMutItem };
            }
            return uniqueMutations;
        }
    }
}