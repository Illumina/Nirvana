using System;
using System.IO;
using System.Text;
using CommonUtilities;

namespace Compression.FileHandling
{
    public sealed class BgzipTextReader : IDisposable
    {
        private readonly Stream _stream;
        private readonly char _newLineChar;
        private readonly byte[] _buffer;
        private int _bufferLength;
        private int _bufferIndex;
        private long _streamPosition;
        private const int BufferSize = BlockGZipStream.BlockGZipFormatCommon.BlockSize;

        public long Position => _streamPosition + _bufferIndex;

        
        public BgzipTextReader(Stream stream, char newLineChar = '\n')
        {
            _buffer = new byte[BufferSize];
            _stream = stream;
            _newLineChar = newLineChar;

            FillBuffer();
        }

        private void FillBuffer()
        {
            _streamPosition = _stream.Position;
            _bufferLength = _stream.Read(_buffer, 0, _buffer.Length);
            _bufferIndex = 0;
        }

        public string ReadLine()
        {
            var sb = StringBuilderCache.Acquire();

            //fill the buffer if empty
            if (_bufferIndex >= _bufferLength)
                FillBuffer();

            if (_bufferLength == 0) return null;
            int startIndex = _bufferIndex;

            while (_buffer[_bufferIndex++] != _newLineChar)
            {
                if (_bufferIndex < _bufferLength) continue;
                // the lenght should be (_bufferIndex - startIndex +1 ) but since _buffer index is already incremented, it is (_bufferIndex - startIndex)
                sb.Append(Encoding.UTF8.GetString(SubArray(_buffer, startIndex, _bufferIndex - startIndex)));
                FillBuffer();
                startIndex = _bufferIndex;
                if (_bufferLength == 0) break;
            }

            // we do not want the last char (new line), therefore, the  length of subarray is _bufferIndex - startIndex -1
            sb.Append(_buffer[_bufferIndex - 1] == _newLineChar
                ? Encoding.UTF8.GetString(SubArray(_buffer, startIndex, _bufferIndex - startIndex - 1))
                : Encoding.UTF8.GetString(SubArray(_buffer, startIndex, _bufferIndex - startIndex)));

            string result = StringBuilderCache.GetStringAndRelease(sb);
            return result.Length == 0 ? null : result;
        }

        private static T[] SubArray<T>(T[] data, int index, int length)
        {
            var result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}