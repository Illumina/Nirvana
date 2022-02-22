using System;
using System.Collections.Generic;
using System.Linq;
using IO;
using VariantAnnotation.SA;

namespace VariantAnnotation.PSA;

public sealed class PsaIndex
{
    public readonly  SaHeader                                                 Header;
    private readonly SaSignature                                              _signature;
    private readonly Dictionary<ushort, List<(string id, long fileLocation)>> _transcriptBlockLocations;

    public PsaIndex(SaHeader header, SaSignature signature,
        Dictionary<ushort, List<(string id, long fileLocation)>> transcriptBlockLocations = null)
    {
        Header     = header;
        _signature = signature;
        _transcriptBlockLocations =
            transcriptBlockLocations ?? new Dictionary<ushort, List<(string id, long fileLocation)>>();
    }

    public static PsaIndex Read(ExtendedBinaryReader reader)
    {
        var signature = SaSignature.Read(reader);
        var header    = SaHeader.Read(reader);

        var chromCount               = reader.ReadOptInt32();
        var transcriptBlockLocations = new Dictionary<ushort, List<(string id, long fileLocation)>>(chromCount);

        for (int i = 0; i < chromCount; i++)
        {
            SaCommon.CheckGuardInt(reader, "chromosome blocks");
            var index               = reader.ReadOptUInt16();
            var transcriptCount     = reader.ReadOptInt32();
            var transcriptLocations = new List<(string id, long location)>(transcriptCount);
            for (int j = 0; j < transcriptCount; j++)
            {
                var transcriptId = reader.ReadAsciiString();
                var fileLocation = reader.ReadOptInt64();
                transcriptLocations.Add((transcriptId, fileLocation));
            }

            transcriptBlockLocations.Add(index, transcriptLocations);
        }

        SaCommon.CheckGuardInt(reader, "end of chrom blocks");

        return new PsaIndex(header, signature, transcriptBlockLocations);
    }

    public void Write(ExtendedBinaryWriter writer)
    {
        _signature.Write(writer);
        Header.Write(writer);

        var chromCount = _transcriptBlockLocations.Count;
        writer.WriteOpt(chromCount);

        foreach (var (chrIndex, transcriptLocations) in _transcriptBlockLocations)
        {
            writer.Write(SaCommon.GuardInt);
            writer.WriteOpt(chrIndex);
            writer.WriteOpt(transcriptLocations.Count);

            foreach (var (id, location) in transcriptLocations.OrderBy(x => x.id))
            {
                writer.WriteOptAscii(id);
                writer.WriteOpt(location);
            }
        }

        writer.Write(SaCommon.GuardInt);
        writer.Flush();
    }

    public void Add(ushort chrIndex, string transcriptId, long fileLocation)
    {
        if (!_transcriptBlockLocations.ContainsKey(chrIndex))
            _transcriptBlockLocations[chrIndex] = new List<(string id, long fileLocation)>();
        _transcriptBlockLocations[chrIndex].Add((transcriptId, fileLocation));
    }

    public long GetFileLocation(ushort chrIndex, string transcriptId)
    {
        if (!_transcriptBlockLocations.ContainsKey(chrIndex)) return -1;
        var transcriptLocations = _transcriptBlockLocations[chrIndex];

        var index = BinarySearch(transcriptLocations, transcriptId);
        return index < 0 ? -1 : transcriptLocations[index].fileLocation;
    }

    private int BinarySearch(List<(string id, long fileLocation)> items, string value)
    {
        var begin = 0;
        int end   = items.Count - 1;

        while (begin <= end)
        {
            int index = begin + (end - begin >> 1);

            int ret = string.Compare(items[index].id, value, StringComparison.Ordinal);
            if (ret == 0) return index;
            if (ret < 0) begin = index + 1;
            else end           = index - 1;
        }

        return ~begin;
    }

    public void ValidateSignature(SaSignature signature)
    {
        if (_signature.Identifier != SaCommon.PsaIdentifier)
            throw new DataMisalignedException(
                $"The PsaIndex does not contain expected identifier: {SaCommon.PsaIdentifier}");

        if (_signature != signature)
            throw new DataMisalignedException($"The index and .psa file signatures do not match.\n" +
                $"Index signature: {_signature}\n"                                                  +
                $".psa signature: {signature}"                                                      +
                $"This index file is not the one corresponding to this .psa file.");
    }
}