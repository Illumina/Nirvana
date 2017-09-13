using System;
using System.IO;
using System.Text;

namespace Compression.FileHandling
{
    public class BgzipTextReader : IDisposable
    {
        readonly Stream _stream;
        private readonly char _newLineChar;
        private readonly byte[] _buffer;
        private int _bufferLength;
        private int _bufferIndex;
        private long _streamPosition;
        private const int BufferSize = 4 * 1024;

        public long Position => _streamPosition + _bufferIndex;

        
        public BgzipTextReader(Stream stream, char newLineChar = '\n')
        {
            _buffer = new byte[BufferSize];//4kb blocks
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
            var line = "";

            //fill the buffer if empty
            if (_bufferIndex >= _bufferLength)
                FillBuffer();

            if (_bufferLength == 0) return null;
            var startIndex = _bufferIndex;

            while (_buffer[_bufferIndex++] != _newLineChar)
            {
                if (_bufferIndex < _bufferLength) continue;
                // the lenght should be (_bufferIndex - startIndex +1 ) but since _buffer index is already incremented, it is (_bufferIndex - startIndex)
                line += Encoding.UTF8.GetString(SubArray(_buffer, startIndex, _bufferIndex - startIndex));
                FillBuffer();
                startIndex = _bufferIndex;
                if (_bufferLength == 0) break;
            }
            //we do not want the last char (new line), therefore, the  length of subarray is _bufferIndex - startIndex -1
            if (_buffer[_bufferIndex - 1] == _newLineChar)
                line += Encoding.UTF8.GetString(SubArray(_buffer, startIndex, _bufferIndex - startIndex - 1));
            else line += Encoding.UTF8.GetString(SubArray(_buffer, startIndex, _bufferIndex - startIndex));

            return string.IsNullOrEmpty(line) ? null : line;
        }

        private static T[] SubArray<T>(T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }
}