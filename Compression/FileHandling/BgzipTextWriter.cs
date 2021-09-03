using System;
using System.Buffers;
using System.IO;
using System.Text;
using OptimizedCore;

namespace Compression.FileHandling
{
    public sealed class BgzipTextWriter : StreamWriter, IDisposable
    {
        private readonly BlockGZipStream _stream;
        private readonly byte[] _buffer;
        private int _bufferIndex;
        
        private const int             CharBufferSize = 8 * 1024 * 1024;
        private       char[]          _charBuffer;
        private       byte[]          _byteBuffer;

        private const int BufferSize = BlockGZipStream.BlockGZipFormatCommon.BlockSize;

        private static readonly UTF8Encoding Utf8WithoutBom = new UTF8Encoding(false);

        public long Position => _stream.Position + _bufferIndex;

        public BgzipTextWriter(BlockGZipStream stream) : base(stream, Utf8WithoutBom, BufferSize, true)
        {
            _buffer = new byte[BufferSize];
            _stream = stream;

            _charBuffer = ExpandableArray<char>.Get(CharBufferSize);
            _byteBuffer = ExpandableArray<byte>.Get(CharBufferSize * 2);
        }

        public override void Flush()
        {
            if (_bufferIndex == 0) return;
            _stream.Write(_buffer, 0, _bufferIndex);
            //here we want to close the gzip blockB
            _stream.CloseBlock();
            _bufferIndex = 0;
        }

        public override void WriteLine() => Write("\n");

        public override void WriteLine(string value) => Write(value + "\n");

        public override void Write(string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            var lineBytes = Encoding.UTF8.GetBytes(value);

            WriteBytes(lineBytes, lineBytes.Length);
        }

        public override void Write(StringBuilder sb)
        {
            if (sb == null || sb.Length == 0) return;
            
            if (sb.Length > _charBuffer.Length)
            {
                _charBuffer = ExpandableArray<char>.Resize(_charBuffer, sb.Length * 2);
                _byteBuffer = ExpandableArray<byte>.Resize(_byteBuffer, _charBuffer.Length     * 2);
            }

            sb.CopyTo(0, _charBuffer, 0, sb.Length);
            var length = Encoding.UTF8.GetBytes(_charBuffer, 0, sb.Length, _byteBuffer, 0);

            WriteBytes(_byteBuffer, length);
        }

        private void WriteBytes(byte[] lineBytes, int length)
        {
            if (length <= BufferSize - _bufferIndex)
            {
                Array.Copy(lineBytes, 0, _buffer, _bufferIndex, length);
                _bufferIndex += length;
            }
            else
            {
                // fill up the buffer
                Array.Copy(lineBytes, 0, _buffer, _bufferIndex, BufferSize - _bufferIndex);
                int lineIndex = BufferSize                                 - _bufferIndex;

                // write it out to the stream
                _stream.Write(_buffer, 0, BufferSize);
                _bufferIndex = 0;

                while (lineIndex + BufferSize <= length)
                {
                    _stream.Write(lineBytes, lineIndex, BufferSize);
                    lineIndex += BufferSize;
                }

                // the leftover bytes should be saved in buffer
                if (lineIndex >= length) return;
                Array.Copy(lineBytes, lineIndex, _buffer, 0, length - lineIndex);
                _bufferIndex = length - lineIndex;
            }
        }

        public new void Dispose()
        {
            Flush();
            _stream.Dispose();
            ExpandableArray<char>.Return(_charBuffer);
            ExpandableArray<byte>.Return(_byteBuffer);
        }
    }
}