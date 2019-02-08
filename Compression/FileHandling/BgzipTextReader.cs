using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Compression.FileHandling
{
    public sealed class BgzipTextReader : IDisposable
    {
        private readonly bool _leaveOpen;
        private readonly StreamReader _reader;
        private readonly FieldInfo _charPosInfo;
        private readonly FieldInfo _charLenInfo;

        public BgzipTextReader(BlockGZipStream stream, bool leaveOpen = false)
        {
            _leaveOpen = leaveOpen;
            _reader    = new StreamReader(stream, Encoding.UTF8, leaveOpen);

            Type readerType = _reader.GetType();
            _charPosInfo    = readerType.GetField("_charPos", BindingFlags.NonPublic | BindingFlags.Instance);
            _charLenInfo    = readerType.GetField("_charLen", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        public long Position
        {
            get
            {
                var bufferPos  = (int)_charPosInfo.GetValue(_reader);
                var bufferSize = (int)_charLenInfo.GetValue(_reader);
                return _reader.BaseStream.Position - bufferSize + bufferPos;
            }
        }

        public string ReadLine() => _reader.ReadLine();

        public void Dispose()
        {
            if (!_leaveOpen) _reader.Dispose();
        }
    }
}