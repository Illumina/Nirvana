using System;
using System.IO;
using System.Text;
using Cache.Index;
using IO;

namespace Cache.IO;

public sealed class CacheIndexWriter : IDisposable
{
    private readonly ExtendedBinaryWriter _writer;

    public CacheIndexWriter(Stream stream, int filePairId, bool leaveOpen = false)
    {
        _writer = new ExtendedBinaryWriter(stream, Encoding.UTF8, leaveOpen);
        WriteHeader(filePairId);
    }

    private void WriteHeader(int filePairId)
    {
        var header = new Header(FileType.TranscriptIndex, CacheIndexReader.SupportedFileFormatVersion);
        header.Write(_writer);
        _writer.WriteOpt(filePairId);
        _writer.Write(CacheReader.GuardInt);
    }

    public void Write(CacheIndex cacheIndex) => cacheIndex.Write(_writer);

    public void Dispose()
    {
        _writer.Write(Header.NirvanaFooter);
        _writer.Dispose();
    }
}