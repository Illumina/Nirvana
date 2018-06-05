using System;
using System.Collections.Generic;
using System.Linq;
using Genome;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Providers;

namespace SAUtils.TsvWriters
{
    public sealed class MitoMapVarTsvWriter : ISaItemTsvWriter
    {

        #region members
        private readonly SaTsvWriter _mitoMapVarWriter;

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
            var uniqueMutations = MitoMapItem.AggregatedMutationsSomePosition(mitoMapMutItems);
            foreach (var mutation in uniqueMutations)
            {
                _mitoMapVarWriter.AddEntry(
                           mutation.Value.Chromosome.EnsemblName,
                           mutation.Value.Start,
                           mutation.Key.Item1,
                           mutation.Key.Item2,
                           null,
                           new List<string> {mutation.Value.GetVariantJsonString()});
            }
        }

        public void Dispose() => _mitoMapVarWriter.Dispose();
    }
}