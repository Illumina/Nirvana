using System.Collections.Generic;
using System.Linq;
using Cache.Index;

namespace Cache.IO;

public sealed class CacheIndexBuilder
{
    private readonly IntermediateReference[] _references;

    public CacheIndexBuilder(int numRefSeqs)
    {
        _references = new IntermediateReference[numRefSeqs];
        for (ushort i = 0; i < numRefSeqs; i++) _references[i] = new IntermediateReference(i);
    }

    public void Add(int refIndex, long position) => _references[refIndex].Position = position;
    public void Add(int refIndex, byte bin, long position) => _references[refIndex].Add(bin, position);

    private sealed class IntermediateReference
    {
        public readonly ushort                 RefIndex;
        public          long                   Position;
        public readonly Dictionary<byte, long> BinToPosition;

        public IntermediateReference(ushort refIndex)
        {
            RefIndex      = refIndex;
            BinToPosition = new Dictionary<byte, long>();
        }

        public void Add(byte bin, long position) => BinToPosition[bin] = position;

        public BinPosition[] CreateBinPositions()
        {
            var binPositions = new List<BinPosition>();
            foreach ((byte bin, long position) in BinToPosition.OrderBy(x => x.Key))
            {
                binPositions.Add(new BinPosition(bin, position));
            }

            return binPositions.ToArray();
        }
    }

    public CacheIndex Build()
    {
        var indexReferences = new List<IndexReference>();

        foreach (IntermediateReference reference in _references)
        {
            if (reference.BinToPosition.Count == 0) continue;

            BinPosition[] binPositions   = reference.CreateBinPositions();
            var           indexReference = new IndexReference(reference.RefIndex, reference.Position, binPositions);
            indexReferences.Add(indexReference);
        }

        return new CacheIndex(indexReferences.ToArray());
    }
}