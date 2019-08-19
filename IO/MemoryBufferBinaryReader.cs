using System;
using System.Text;

namespace IO
{
    public sealed class MemoryBufferBinaryReader
    {
        private readonly byte[] _buffer;
        private int _bufferPos;

        public MemoryBufferBinaryReader(byte[] buffer) => _buffer = buffer;

        public string ReadAsciiString()
        {
            int numBytes = ReadOptInt32();
            return numBytes == 0 ? null : Encoding.ASCII.GetString(ReadBytes(numBytes));
        }

        public byte[] ReadBytes(int numBytes)
        {
            var values = new byte[numBytes];
            Array.Copy(_buffer, _bufferPos, values, 0, numBytes);
            _bufferPos += numBytes;
            return values;
        }

        public int ReadOptInt32()
        {
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
    }
}
