using System;
using System.Collections.Generic;
using System.Linq;
using IO;
using VariantAnnotation.Algorithms;
using VariantAnnotation.SA;

namespace VariantAnnotation.PSA;

public sealed class PsaIndex
{
    private readonly Dictionary<ushort, List<PsaIndexBlock>> _geneIndexBlocks;
    public readonly  SaHeader                                Header;
    private readonly SaSignature                             _signature;

    public PsaIndex(SaHeader header, SaSignature signature,
        Dictionary<ushort, List<PsaIndexBlock>> geneIndexBlocks = null)
    {
        Header           = header;
        _signature       = signature;
        _geneIndexBlocks = geneIndexBlocks ?? new Dictionary<ushort, List<PsaIndexBlock>>();
    }

    public void Write(ExtendedBinaryWriter writer)
    {
        _signature.Write(writer);
        Header.Write(writer);

        writer.WriteOpt(_geneIndexBlocks.Count);
        foreach ((ushort index, List<PsaIndexBlock> indexBlocks) in _geneIndexBlocks)
        {
            writer.Write(SaCommon.GuardInt);
            writer.WriteOpt(index);
            writer.WriteOpt(indexBlocks.Count);
            foreach (PsaIndexBlock indexBlock in indexBlocks.OrderBy(x => x.GeneName))
            {
                indexBlock.Write(writer);
            }
        }

        writer.Write(SaCommon.GuardInt);
        writer.Flush();
    }

    public static PsaIndex Read(ExtendedBinaryReader reader)
    {
        var signature = SaSignature.Read(reader);
        var header    = SaHeader.Read(reader);

        int chromCount      = reader.ReadOptInt32();
        var geneIndexBlocks = new Dictionary<ushort, List<PsaIndexBlock>>(chromCount);

        for (var i = 0; i < chromCount; i++)
        {
            PsaUtilities.CheckGuardInt(reader, "chromosome blocks");
            ushort index       = reader.ReadOptUInt16();
            int    blockCount  = reader.ReadOptInt32();
            var    indexBlocks = new List<PsaIndexBlock>(blockCount);
            for (var j = 0; j < blockCount; j++)
            {
                var block = PsaIndexBlock.Read(reader);
                indexBlocks.Add(block);
            }

            geneIndexBlocks.Add(index, indexBlocks);
        }

        PsaUtilities.CheckGuardInt(reader, "end of chrom blocks");

        return new PsaIndex(header, signature, geneIndexBlocks);
    }

    public long GetGeneBlockPosition(ushort index, string geneName)
    {
        if (!_geneIndexBlocks.TryGetValue(index, out List<PsaIndexBlock> geneBlocks)) return -1;

        int i = Search.BinarySearch(geneBlocks, geneName);
        if (i < 0) return -1;

        return geneBlocks[i].FilePosition;
    }

    public void AddGeneBlock(ushort chromIndex, string geneName, int start, int end, long filePosition)
    {
        if (!_geneIndexBlocks.ContainsKey(chromIndex))
            _geneIndexBlocks.Add(chromIndex, new List<PsaIndexBlock>());

        _geneIndexBlocks[chromIndex].Add(new PsaIndexBlock(geneName, start, end, filePosition));
    }

    public void ValidateSignature(SaSignature signature)
    {
        if (_signature.Identifier != SaCommon.PsaIdentifier)
            throw new DataMisalignedException(
                $"The PsaIndex does not contain expected identifier: {SaCommon.PsaIdentifier}");

        if (_signature != signature)
            throw new DataMisalignedException("The index and .psa file signatures do not match.\n" +
                $"Index signature: {_signature}\n"                                                 +
                $".psa signature: {signature}"                                                     +
                "This index file is not the one corresponding to this .psa file.");
    }
}