using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cache.Data;
using Compression.Utilities;
using IO;
using Versioning;

namespace Cache.IO;

public sealed class CacheWriter : IDisposable
{
    private readonly Stream               _stream;
    private readonly ExtendedBinaryWriter _writer;
    private readonly CacheIndexBuilder    _indexBuilder;

    public readonly int FilePairId;

    public CacheWriter(Stream stream, DataSourceVersion dataSourceVersion, CacheIndexBuilder indexBuilder,
        bool leaveOpen = false)
    {
        _stream       = stream;
        _writer       = new ExtendedBinaryWriter(stream, Encoding.UTF8, leaveOpen);
        _indexBuilder = indexBuilder;

        var random = new Random();
        FilePairId = random.Next();

        WriteHeader(dataSourceVersion);
    }

    private void WriteHeader(DataSourceVersion dataSourceVersion)
    {
        var header = new Header(FileType.Transcript, CacheReader.SupportedFileFormatVersion);
        header.Write(_writer);
        dataSourceVersion.Write(_writer);
        _writer.WriteOpt(FilePairId);
        _writer.Write(CacheReader.GuardInt);
    }

    public void Write(ReferenceCache?[] referenceCaches)
    {
        int numRefSeqs = referenceCaches.Length;
        _writer.WriteOpt(numRefSeqs);

        for (ushort refIndex = 0; refIndex < numRefSeqs; refIndex++)
        {
            WriteReference(refIndex, referenceCaches[refIndex]);
        }
    }

    private void WriteReference(ushort refIndex, ReferenceCache? referenceCache)
    {
        if (referenceCache == null)
        {
            _writer.Write((byte) 0);
            return;
        }

        long referencePosition = _stream.Position;
        _indexBuilder.Add(refIndex, referencePosition);

        var numBins = (byte) referenceCache.CacheBins.Length;
        _writer.Write(numBins);

        for (byte binIndex = 0; binIndex < numBins; binIndex++)
        {
            CacheBin cacheBin = referenceCache.CacheBins[binIndex];
            WriteCacheBin(refIndex, binIndex, cacheBin);
        }
    }

    private void WriteCacheBin(ushort refIndex, byte binIndex, CacheBin cacheBin)
    {
        long binPosition = _stream.Position;

        _indexBuilder.Add(refIndex, binIndex, binPosition);

        using var ms = new MemoryStream();
        using (var writer = new ExtendedBinaryWriter(ms, Encoding.UTF8, true))
        {
            writer.Write(cacheBin.EarliestTranscriptBin);
            writer.Write(cacheBin.EarliestRegulatoryRegionBin);
            WriteItems(writer, cacheBin.Genes);
            WriteItems(writer, cacheBin.TranscriptRegions);
            WriteStrings(writer, cacheBin.CdnaSeqs);
            WriteStrings(writer, cacheBin.ProteinSeqs);

            Dictionary<Gene, int>             geneIndices             = CreateIndex(cacheBin.Genes);
            Dictionary<TranscriptRegion, int> transcriptRegionIndices = CreateIndex(cacheBin.TranscriptRegions);
            Dictionary<string, int>           cdnaSeqIndices          = CreateIndex(cacheBin.CdnaSeqs);
            Dictionary<string, int>           proteinSeqIndices       = CreateIndex(cacheBin.ProteinSeqs);

            WriteTranscripts(writer, cacheBin.Transcripts, geneIndices, transcriptRegionIndices, cdnaSeqIndices,
                proteinSeqIndices);
            WriteItems(writer, cacheBin.RegulatoryRegions);
        }

        byte[] bytes = ms.ToArray();
        (int compressedSize, double percentCompression) = _writer.WriteCompressedByteArray(bytes, bytes.Length);

        // Console.WriteLine($"{refIndex}:{cacheBin.Bin}\tcompressed: {compressedSize:N0} bytes ({percentCompression:P1})");
    }

    private static void WriteTranscripts(ExtendedBinaryWriter writer, Transcript[]? transcripts,
        Dictionary<Gene, int> geneIndices, Dictionary<TranscriptRegion, int> transcriptRegionIndices,
        Dictionary<string, int> cdnaSeqIndices, Dictionary<string, int> proteinSeqIndices)
    {
        if (transcripts == null)
        {
            writer.WriteOpt(0);
            return;
        }

        writer.WriteOpt(transcripts.Length);
        foreach (Transcript transcript in transcripts)
            transcript.Write(writer, geneIndices, transcriptRegionIndices, cdnaSeqIndices, proteinSeqIndices);
    }

    private static void WriteItems<T>(ExtendedBinaryWriter writer, T[]? items) where T : IWritable
    {
        if (items == null)
        {
            writer.WriteOpt(0);
            return;
        }

        writer.WriteOpt(items.Length);
        foreach (T item in items) item.Write(writer);
    }

    private static void WriteStrings(ExtendedBinaryWriter writer, string[]? strings)
    {
        if (strings == null)
        {
            writer.WriteOpt(0);
            return;
        }

        writer.WriteOpt(strings.Length);
        foreach (string s in strings) writer.Write(s);
    }

    private static Dictionary<T, int> CreateIndex<T>(T[]? array) where T : notnull
    {
        if (array == null) return new Dictionary<T, int>();
        var index = new Dictionary<T, int>(array.Length);

        for (var currentIndex = 0; currentIndex < array.Length; currentIndex++)
            index[array[currentIndex]] = currentIndex;

        return index;
    }

    public void Dispose()
    {
        _writer.Write(Header.NirvanaFooter);
        _writer.Dispose();
    }
}