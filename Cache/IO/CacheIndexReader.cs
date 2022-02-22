using System;
using System.IO;
using System.Text;
using Cache.Index;
using ErrorHandling;
using IO;

namespace Cache.IO;

public sealed class CacheIndexReader : IDisposable
{
    private readonly ExtendedBinaryReader _reader;
    public readonly  int                  FilePairId;

    public const ushort SupportedFileFormatVersion = 1;
    public const uint   GuardInt                   = 4041327495; // 87c3e1f0

    public CacheIndexReader(Stream stream)
    {
        _reader = new ExtendedBinaryReader(stream, Encoding.UTF8);
        Header     header     = Header.Read(_reader);
        FilePairId = _reader.ReadOptInt32();
        uint       guardInt   = _reader.ReadUInt32();
        CheckHeader(header.FileType, header.FileFormatVersion, guardInt);
    }

    public static void CheckHeader(FileType fileType, ushort fileFormatVersion, uint guardInt)
    {
        if (fileType != FileType.TranscriptIndex)
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

    public CacheIndex GetCacheIndex() => CacheIndex.Read(_reader);

    public void Dispose() => _reader.Dispose();
}