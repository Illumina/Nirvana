using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures.Mutable;

namespace CacheUtils.Genes.DataStores
{
    public sealed class UpdateHgncData : IUpdateHgncData
    {
        public Dictionary<ushort, List<MutableGene>> EnsemblGenesByRef { get; }
        public Dictionary<ushort, List<MutableGene>> RefSeqGenesByRef { get; }

        public UpdateHgncData(Dictionary<ushort, List<MutableGene>> ensemblGenesByRef,
            Dictionary<ushort, List<MutableGene>> refSeqGenesByRef)
        {
            EnsemblGenesByRef = ensemblGenesByRef;
            RefSeqGenesByRef  = refSeqGenesByRef;
        }
    }
}
