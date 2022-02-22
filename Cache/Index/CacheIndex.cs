using System;
using System.Collections.Generic;
using System.Linq;
using Genome;
using IO;

namespace Cache.Index;

public sealed class CacheIndex : IEquatable<CacheIndex>
{
    private readonly IndexReference[] _references;

    // not serialized
    private readonly Dictionary<ushort, IndexReference> _refIndexToIndexReference;

    public CacheIndex(IndexReference[] references)
    {
        _references               = references;
        _refIndexToIndexReference = new Dictionary<ushort, IndexReference>(references.Length);

        foreach (var reference in references) _refIndexToIndexReference[reference.RefIndex] = reference;
    }

    public void Write(ExtendedBinaryWriter writer)
    {
        writer.WriteOpt(_references.Length);
        foreach (IndexReference reference in _references) reference.Write(writer);
    }

    public static CacheIndex Read(ExtendedBinaryReader reader)
    {
        int numReferences = reader.ReadOptInt32();
        var references    = new IndexReference[numReferences];

        for (var i = 0; i < numReferences; i++) references[i] = IndexReference.Read(reader);
        return new CacheIndex(references);
    }

    public long? GetReferencePosition(Chromosome chromosome) =>
        _refIndexToIndexReference.TryGetValue(chromosome.Index, out var reference) ? reference.Position : null;

    public IndexReference? GetIndexReference(Chromosome chromosome) =>
        _refIndexToIndexReference.TryGetValue(chromosome.Index, out var reference) ? reference : null;

    public bool Equals(CacheIndex? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return _references.SequenceEqual(other._references);
    }

    public override int GetHashCode() => _references.GetHashCode();
}