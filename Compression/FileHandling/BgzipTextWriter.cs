using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.Utilities;

namespace Compression.FileHandling
{
    public sealed class BgzipTextWriter : StreamWriter, IDisposable
    {
        readonly BlockGZipStream _bgzipStream;
        private readonly byte[] _buffer;
        private int _bufferIndex;
        private const int BufferSize = BlockGZipStream.BlockGZipFormatCommon.BlockSize;

        public long Position => _bgzipStream.Position + _bufferIndex;


        
        public BgzipTextWriter(string path) : this(new BlockGZipStream(FileUtilities.GetCreateStream(path), CompressionMode.Compress))
        {
        }
        // the stream writer needs to have a stream but we cannot provide it with 
        public BgzipTextWriter(BlockGZipStream bgzipStream):base(Console.OpenStandardError())
        {
            _buffer = new byte[BufferSize];//4kb blocks
            _bgzipStream = bgzipStream;
        }

        public override void Flush()
        {
            if (_bufferIndex == 0) return;
            _bgzipStream.Write(_buffer, 0, _bufferIndex);
            _bufferIndex = 0;
        }

        public override void WriteLine()
        {
            Write("\n");
        }

        public override void WriteLine(string s)
        {
            Write(s+"\n");
        }

        public override void Write(string line)
        {
            if (string.IsNullOrEmpty(line)) return;
            var lineBytes = Encoding.UTF8.GetBytes(line);

            if (lineBytes.Length <= BufferSize - _bufferIndex)
            {
                Array.Copy(lineBytes, 0, _buffer, _bufferIndex, lineBytes.Length);
                _bufferIndex += lineBytes.Length;
            }
            else
            {
                //fill up the buffer
                Array.Copy(lineBytes, 0, _buffer, _bufferIndex, BufferSize - _bufferIndex);
                var lineIndex = BufferSize - _bufferIndex;

                //write it out to the stream
                _bgzipStream.Write(_buffer, 0, BufferSize);
                _bufferIndex = 0;

                while (lineIndex + BufferSize <= lineBytes.Length)
                {
                    _bgzipStream.Write(lineBytes, lineIndex, BufferSize);
                    lineIndex += BufferSize;
                }
                //the leftover bytes should be saved in buffer
                if (lineIndex < lineBytes.Length)
                {
                    Array.Copy(lineBytes, lineIndex, _buffer, 0, lineBytes.Length - lineIndex);
                    _bufferIndex = lineBytes.Length - lineIndex;
                }

            }
        }

        public new void Dispose()
        {
            Flush();
            _bgzipStream.Dispose();
        }
    }
}