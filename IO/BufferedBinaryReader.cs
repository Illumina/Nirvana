using System;
using System.IO;
using System.Text;

namespace IO
{
    public sealed class BufferedBinaryReader : IBufferedBinaryReader
    {
        private const int MinBufferSize = 128;
        private const int BufferSize    = 10_485_760;

        private const int ShortLen  = 2;
        private const int IntLen    = 4;
        private const int LongLen   = 8;
        private const int DoubleLen = 8;

        private readonly Stream _stream;

        private readonly Encoding _encoding = Encoding.UTF8;
        private readonly Decoder _decoder   = Encoding.UTF8.GetDecoder();

        private readonly byte[] _buffer;
        private char[] _charBuffer = new char[MinBufferSize];

        private bool _foundEof;

        private int _bufferLen;
        private int _bufferPos;

        private readonly bool _leaveOpen;

        public BufferedBinaryReader(Stream stream, bool leaveOpen = false, int bufferSize = BufferSize)
        {
            if (stream == null)  throw new ArgumentNullException(nameof(stream));
            if (!stream.CanRead) throw new ArgumentException("A non-readable stream was supplied.", nameof(stream));
            if (bufferSize <= 0) throw new ArgumentOutOfRangeException(nameof(bufferSize));

            _stream    = stream;
            _buffer    = new byte[bufferSize];
            _leaveOpen = leaveOpen;

            FillBuffer();
        }

        private void FillBuffer()
        {
            int numRemainingBytes = _bufferLen - _bufferPos;
            if (numRemainingBytes > 0) Buffer.BlockCopy(_buffer, _bufferPos, _buffer, 0, numRemainingBytes);

            _bufferPos = 0;
            _bufferLen = numRemainingBytes;

            int numBytesRead = _stream.Read(_buffer, numRemainingBytes, _buffer.Length - numRemainingBytes);
            _bufferLen       = numRemainingBytes + numBytesRead;

            if (_bufferPos == 0 && _bufferLen == 0) _foundEof = true;
        }

        public long BufferPosition
        {
            get => _bufferPos;
            set => _bufferPos = (int)value;
        }

        public void Reset()
        {
            _bufferLen = 0;
            _bufferPos = 0;
            FillBuffer();
        }

        public string ReadAsciiString()
        {
            int numBytes = ReadOptInt32();
            return numBytes == 0 ? null : Encoding.ASCII.GetString(ReadBytes(numBytes));
        }

        public bool ReadBoolean()
        {
            if (_bufferPos == _bufferLen) FillBuffer();
            return _buffer[_bufferPos++] != 0;
        }

        public byte ReadByte()
        {
            if (_bufferPos == _bufferLen) FillBuffer();
            return _buffer[_bufferPos++];
        }

        public byte[] ReadBytes(int numBytes)
        {
            if (numBytes == 1) return new[] { ReadByte() };

            var values = new byte[numBytes];
            Read(values, numBytes);
            return values;
        }

        public void Read(byte[] buffer, int numBytes)
        {
            var offset            = 0;
            int numBytesRemaining = numBytes;

            while (numBytesRemaining > 0)
            {
                if (_bufferPos == _bufferLen)
                {
                    FillBuffer();
                    if (_foundEof) break;
                }

                int numBytesAvailable = _bufferLen - _bufferPos;
                int copyLength        = numBytesRemaining < numBytesAvailable ? numBytesRemaining : numBytesAvailable;

                Buffer.BlockCopy(_buffer, _bufferPos, buffer, offset, copyLength);

                offset            += copyLength;
                _bufferPos        += copyLength;
                numBytesRemaining -= copyLength;
            }
        }

        public unsafe double ReadDouble()
        {
            if (_bufferPos > _bufferLen - DoubleLen) FillBuffer();

            double value;
            fixed (byte* pBuffer = &_buffer[_bufferPos])
            {
                var lo = (uint)(pBuffer[0] | pBuffer[1] << 8 | pBuffer[2] << 16 | pBuffer[3] << 24);
                var hi = (uint)(pBuffer[4] | pBuffer[5] << 8 | pBuffer[6] << 16 | pBuffer[7] << 24);
                ulong tmpBuffer = (ulong)hi << 32 | lo;
                value = *(double*)&tmpBuffer;
                _bufferPos += DoubleLen;
            }

            return value;
        }

        public unsafe short ReadInt16()
        {
            if (_bufferPos > _bufferLen - ShortLen) FillBuffer();

            short value;
            fixed (byte* pBuffer = &_buffer[_bufferPos])
            {
                value = (short)(pBuffer[0] | pBuffer[1] << 8);
                _bufferPos += ShortLen;
            }

            return value;
        }

        public unsafe int ReadInt32()
        {
            if (_bufferPos > _bufferLen - IntLen) FillBuffer();

            int value;
            fixed (byte* pBuffer = &_buffer[_bufferPos])
            {
                value = pBuffer[0] | pBuffer[1] << 8 | pBuffer[2] << 16 | pBuffer[3] << 24;
            }

            _bufferPos += IntLen;
            return value;
        }

        public unsafe long ReadInt64()
        {
            if (_bufferPos > _bufferLen - LongLen) FillBuffer();

            long value;
            fixed (byte* pBuffer = &_buffer[_bufferPos])
            {
                var lo = (uint)(pBuffer[0] | pBuffer[1] << 8 | pBuffer[2] << 16 | pBuffer[3] << 24);
                var hi = (uint)(pBuffer[4] | pBuffer[5] << 8 | pBuffer[6] << 16 | pBuffer[7] << 24);
                value= (long)hi << 32 | lo;
                _bufferPos += LongLen;
            }

            return value;
        }

        public int ReadOptInt32()
        {
            if (_bufferPos > _bufferLen - 5) FillBuffer();

            var count = 0;
            var shift = 0;

            while (shift != 35)
            {
                byte b = _buffer[_bufferPos++];
                count |= (b & sbyte.MaxValue) << shift;
                shift += 7;

                if ((b & 128) == 0) return count;
            }

            throw new FormatException("Unable to read the 7-bit encoded integer");
        }

        public long ReadOptInt64()
        {
            if (_bufferPos > _bufferLen - 9) FillBuffer();

            long count = 0;
            var shift  = 0;

            while (shift != 70)
            {
                byte b = _buffer[_bufferPos++];
                count |= (long)(b & sbyte.MaxValue) << shift;
                shift += 7;

                if ((b & 128) == 0) return count;
            }

            throw new FormatException("Unable to read the 7-bit encoded long");
        }

        public ushort ReadOptUInt16()
        {
            if (_bufferPos > _bufferLen - 3) FillBuffer();

            ushort count = 0;
            var shift    = 0;

            while (shift != 21)
            {
                byte b = ReadByte();
                count |= (ushort)((b & sbyte.MaxValue) << shift);
                shift += 7;

                if ((b & 128) == 0) return count;
            }

            throw new FormatException("Unable to read the 7-bit encoded unsigned short");
        }

        public string ReadString()
        {
            int numBytes = ReadOptInt32();

            if (numBytes < 0) throw new IOException();
            if (numBytes == 0) return string.Empty;

            if (_bufferPos > _bufferLen - numBytes) FillBuffer();

            int maxBufferSize = _encoding.GetMaxCharCount(numBytes);
            if (maxBufferSize > _charBuffer.Length) _charBuffer = new char[maxBufferSize];

            int numChars = _decoder.GetChars(_buffer, _bufferPos, numBytes, _charBuffer, 0);
            var value = new string(_charBuffer, 0, numChars);
            _bufferPos += numBytes;

            return value;
        }

        public unsafe ushort ReadUInt16()
        {
            if (_bufferPos > _bufferLen - ShortLen) FillBuffer();

            ushort value;
            fixed (byte* pBuffer = &_buffer[_bufferPos])
            {
                value = (ushort)(pBuffer[0] | pBuffer[1] << 8);
                _bufferPos += ShortLen;
            }

            return value;
        }

        public unsafe uint ReadUInt32()
        {
            if (_bufferPos > _bufferLen - IntLen) FillBuffer();

            uint value;
            fixed (byte* pBuffer = &_buffer[_bufferPos])
            {
                value = (uint)(pBuffer[0] | pBuffer[1] << 8 | pBuffer[2] << 16 | pBuffer[3] << 24);
                _bufferPos += IntLen;
            }

            return value;
        }

        public unsafe ulong ReadUInt64()
        {
            if (_bufferPos > _bufferLen - LongLen) FillBuffer();

            ulong value;
            fixed (byte* pBuffer = &_buffer[_bufferPos])
            {
                var lo = (uint)(pBuffer[0] | pBuffer[1] << 8 | pBuffer[2] << 16 | pBuffer[3] << 24);
                var hi = (uint)(pBuffer[4] | pBuffer[5] << 8 | pBuffer[6] << 16 | pBuffer[7] << 24);
                value = (ulong)hi << 32 | lo;
                _bufferPos += LongLen;
            }

            return value;
        }

        public void Dispose()
        {
            if (!_leaveOpen) _stream?.Dispose();
        }
    }
}
