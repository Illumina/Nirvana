using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cache.Data;
using Compression.Utilities;
using ErrorHandling;
using Genome;
using IO;
using Versioning;

namespace Cache.IO;

public sealed class CacheReader : IDisposable
{
    private readonly Chromosome[]            _chromosomes;
    private readonly Dictionary<int, string> _hgncIdToSymbol;

    private readonly Stream               _stream;
    private readonly ExtendedBinaryReader _reader;

    public readonly int               FilePairId;
    public readonly DataSourceVersion DataSourceVersion;

    public const ushort SupportedFileFormatVersion = 1;
    public const uint   GuardInt                   = 4041327495; // 87c3e1f0

    public CacheReader(Stream stream, Chromosome[] chromosomes, Dictionary<int, string> hgncIdToSymbol)
    {
        _chromosomes    = chromosomes;
        _hgncIdToSymbol = hgncIdToSymbol;

        _stream = stream;
        _reader = new ExtendedBinaryReader(stream, Encoding.UTF8);

        Header header = Header.Read(_reader);
        DataSourceVersion = DataSourceVersion.Read(_reader);

        FilePairId = _reader.ReadOptInt32();
        uint guardInt = _reader.ReadUInt32();

        CheckHeader(header.FileType, header.FileFormatVersion, guardInt);
    }

    public static void CheckHeader(FileType fileType, ushort fileFormatVersion, uint guardInt)
    {
        if (fileType != FileType.Transcript)
            throw new InvalidDataException(
                    $"Found an invalid file type ({fileType}) while reading the cache file.")
                .MakeUserError();

        if (fileFormatVersion != SupportedFileFormatVersion)
            throw new InvalidDataException(
                    $"The cache reader currently supports v{SupportedFileFormatVersion} files, but found v{fileFormatVersion} instead.")
                .MakeUserError();

        if (guardInt != GuardInt)
            throw new InvalidDataException(
                    $"The guard integer is different ({guardInt}) than what was expected ({GuardInt}).")
                .MakeUserError();
    }

    public void SetPosition(long position) => _stream.Position = position;

    public ReferenceCache?[] GetReferenceCaches()
    {
        int numReferenceCaches = _reader.ReadOptInt32();
        var referenceCaches    = new ReferenceCache?[numReferenceCaches];

        for (ushort refIndex = 0; refIndex < numReferenceCaches; refIndex++)
            referenceCaches[refIndex] = GetReferenceCache(refIndex);
        return referenceCaches;
    }

    public ReferenceCache? GetReferenceCache(ushort refIndex)
    {
        Chromosome chromosome = _chromosomes[refIndex];

        byte numCacheBins = _reader.ReadByte();
        if (numCacheBins == 0) return null;

        var cacheBins = new CacheBin[numCacheBins];

        for (byte binIndex = 0; binIndex < numCacheBins; binIndex++) cacheBins[binIndex] = GetCacheBin(chromosome);
        return new ReferenceCache(_chromosomes[refIndex], cacheBins);
    }

    public CacheBin GetCacheBin(Chromosome chromosome)
    {
        ArrayPool<byte>    bytePool = ArrayPool<byte>.Shared;
        byte[]             bytes    = _reader.ReadCompressedByteArray(bytePool);
        ReadOnlySpan<byte> byteSpan = bytes.AsSpan();

        byte earliestTranscriptBin       = SpanBufferBinaryReader.ReadByte(ref byteSpan);
        byte earliestRegulatoryRegionBin = SpanBufferBinaryReader.ReadByte(ref byteSpan);

        Gene[]?             genes             = ReadGenes(ref byteSpan);
        TranscriptRegion[]? transcriptRegions = ReadTranscriptRegions(ref byteSpan);
        string[]?           cdnaSeqs          = ReadStrings(ref byteSpan);
        string[]?           proteinSeqs       = ReadStrings(ref byteSpan);
        Transcript[]?       transcripts       = ReadTranscripts(ref byteSpan, chromosome, genes, transcriptRegions, cdnaSeqs, proteinSeqs);
        RegulatoryRegion[]? regulatoryRegions = ReadRegulatoryRegions(ref byteSpan, chromosome);

        var cacheBin = new CacheBin(earliestTranscriptBin, earliestRegulatoryRegionBin, genes, transcriptRegions,
            cdnaSeqs, proteinSeqs, transcripts, regulatoryRegions);
        
        bytePool.Return(bytes);
        return cacheBin;
    }

    private Gene[]? ReadGenes(ref ReadOnlySpan<byte> byteSpan)
    {
        int numGenes = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        if (numGenes == 0) return null;
        var genes = new Gene[numGenes];

        for (var i = 0; i < numGenes; i++) genes[i] = Gene.Read(ref byteSpan, _hgncIdToSymbol);
        return genes;
    }
    
    private static TranscriptRegion[]? ReadTranscriptRegions(ref ReadOnlySpan<byte> byteSpan)
    {
        int numTranscriptRegions = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        if (numTranscriptRegions == 0) return null;
        var transcriptRegions = new TranscriptRegion[numTranscriptRegions];

        for (var i = 0; i < numTranscriptRegions; i++) transcriptRegions[i] = TranscriptRegion.Read(ref byteSpan);
        return transcriptRegions;
    }

    private static string[]? ReadStrings(ref ReadOnlySpan<byte> byteSpan)
    {
        int numStrings = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        if (numStrings == 0) return null;
        var strings = new string[numStrings];

        for (var i = 0; i < numStrings; i++) strings[i] = SpanBufferBinaryReader.ReadUtf8String(ref byteSpan);
        return strings;
    }

    private static Transcript[]? ReadTranscripts(ref ReadOnlySpan<byte> byteSpan, Chromosome chromosome, Gene[]? genes,
        TranscriptRegion[]? transcriptRegions, string[]? cdnaSeqs, string[]? proteinSeqs)
    {
        int numTranscripts = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        if (numTranscripts == 0) return null;
        var transcripts = new Transcript[numTranscripts];

        for (var i = 0; i < numTranscripts; i++)
            transcripts[i] = Transcript.Read(ref byteSpan, chromosome, genes!, transcriptRegions!, cdnaSeqs!, proteinSeqs!);
        return transcripts;
    }

    private static RegulatoryRegion[]? ReadRegulatoryRegions(ref ReadOnlySpan<byte> byteSpan, Chromosome chromosome)
    {
        int numRegulatoryRegions = SpanBufferBinaryReader.ReadOptInt32(ref byteSpan);
        if (numRegulatoryRegions == 0) return null;
        var regulatoryRegions = new RegulatoryRegion[numRegulatoryRegions];

        for (var i = 0; i < numRegulatoryRegions; i++) regulatoryRegions[i] = RegulatoryRegion.Read(ref byteSpan, chromosome);
        return regulatoryRegions;
    }
    
    public void Dispose() => _reader.Dispose();
}