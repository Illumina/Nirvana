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
    public class MitoMapMutationTsvWriter : ISaItemTsvWriter
    {

        #region members
        private readonly SaTsvWriter _mitoMapMutWriter;

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
                _mitoMapMutWriter.Dispose();
            }

            // Free any unmanaged objects here.
            //
            _disposed = true;
            // Free any other managed objects here.

        }
        #endregion

        public MitoMapMutationTsvWriter(DataSourceVersion version, string outputDirectory, string mitoMapDataType, ISequenceProvider sequenceProvider)
        {
            Console.WriteLine(version.ToString());
            _mitoMapMutWriter = new SaTsvWriter(outputDirectory, version, GenomeAssembly.Unknown.ToString(), SaTSVCommon.MitoMapSchemaVersion, mitoMapDataType, null, false, sequenceProvider, true);
        }


        public void WritePosition(IEnumerable<SupplementaryDataItem> saItems)
        {
            if (saItems == null) return;
            var mitoMapMutItems = saItems.Select(x => x as MitoMapMutItem).ToList();
            var uniqueMutations = GetUniqueMutations(mitoMapMutItems);
            foreach (var mutation in uniqueMutations)
            {
                _mitoMapMutWriter.AddEntry(
                           mutation.Value[0].Chromosome.EnsemblName,
                           mutation.Value[0].Start,
                           mutation.Key.Item1,
                           mutation.Key.Item2,
                           null,
                           mutation.Value.Select(x => x.GetJsonString()).Distinct().ToList());
            }
        }

        private Dictionary<(string, string), List<MitoMapMutItem>> GetUniqueMutations(List<MitoMapMutItem> mitoMapMutItems)
        {
            var uniqueMutations = new Dictionary<(string, string), List<MitoMapMutItem>>();

            foreach (var mitoMapMutItem in mitoMapMutItems)
            {
                var mutation = (mitoMapMutItem.ReferenceAllele, mitoMapMutItem.AlternateAllele);
                if (uniqueMutations.ContainsKey(mutation))
                    uniqueMutations[mutation].Add(mitoMapMutItem);
                else uniqueMutations[mutation] = new List<MitoMapMutItem> { mitoMapMutItem };
            }
            return uniqueMutations;
        }
    }
}