using System.Collections.Generic;
using CacheUtils.DataDumperImport.DataStructures.Mutable;

namespace CacheUtils.Genes.DataStores
{
    public interface IUpdateHgncData
    {
        Dictionary<ushort, List<MutableGene>> EnsemblGenesByRef { get; }
        Dictionary<ushort, List<MutableGene>> RefSeqGenesByRef { get; }
    }
}
