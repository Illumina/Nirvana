using System;
using System.IO;
using System.Text;

namespace Compression.FileHandling
{
    public sealed class BgzipTextWriter : StreamWriter, IDisposable
    {
        private readonly BlockGZipStream _stream;
        private readonly byte[] _buffer;
        private int _bufferIndex;
        private const int BufferSize = BlockGZipStream.BlockGZipFormatCommon.BlockSize;

        private static readonly UTF8Encoding Utf8WithoutBom = new UTF8Encoding(false);

        public long Position => _stream.Position + _bufferIndex;

        public BgzipTextWriter(BlockGZipStream stream) : base(stream, Utf8WithoutBom, BufferSize, true)
        {
            _buffer = new byte[BufferSize];
            _stream = stream;
        }

        public override void Flush()
        {
            if (_bufferIndex == 0) return;
            _stream.Write(_buffer, 0, _bufferIndex);
            _bufferIndex = 0;
        }

        public override void WriteLine() => Write("\n");

        public override void WriteLine(string value) => Write(value + "\n");

        public override void Write(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            var lineBytes = Encoding.UTF8.GetBytes(value);

            if (lineBytes.Length <= BufferSize - _bufferIndex)
            {
                Array.Copy(lineBytes, 0, _buffer, _bufferIndex, lineBytes.Length);
                _bufferIndex += lineBytes.Length;
            }
            else
            {
                // fill up the buffer
                Array.Copy(lineBytes, 0, _buffer, _bufferIndex, BufferSize - _bufferIndex);
                int lineIndex = BufferSize - _bufferIndex;

                // write it out to the stream
                _stream.Write(_buffer, 0, BufferSize);
                _bufferIndex = 0;

                while (lineIndex + BufferSize <= lineBytes.Length)
                {
                    _stream.Write(lineBytes, lineIndex, BufferSize);
                    lineIndex += BufferSize;
                }

                // the leftover bytes should be saved in buffer
                if (lineIndex >= lineBytes.Length) return;
                Array.Copy(lineBytes, lineIndex, _buffer, 0, lineBytes.Length - lineIndex);
                _bufferIndex = lineBytes.Length - lineIndex;
            }
        }

        public new void Dispose()
        {
            Flush();
            _stream.Dispose();
        }
    }
}