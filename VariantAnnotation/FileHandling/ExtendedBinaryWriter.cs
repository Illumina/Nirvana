using System.IO;
using System.Text;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;

namespace VariantAnnotation.FileHandling
{
    public class ExtendedBinaryWriter
    {
        #region members

        private readonly BinaryWriter _writer;

        #endregion

        // constructor
        public ExtendedBinaryWriter(BinaryWriter writer)
        {
            _writer = writer;
        }

        /// <summary>
        /// writes a boolean to the binary writer
        /// </summary>
        public void WriteBoolean(bool b)
        {
            _writer.Write(b);
        }

        /// <summary>
        /// writes a byte to the binary writer
        /// </summary>
        public void WriteByte(byte b)
        {
            _writer.Write(b);
        }

        public void WriteDouble(double value)
        {
            var i = (int)(value * SupplementaryAnnotation.PrecisionConst);
            WriteInt(i);
        }

        /// <summary>
        /// writes a byte array to the binary writer
        /// </summary>
        public void WriteBytes(byte[] buffer)
        {
            _writer.Write(buffer);
        }

        /// <summary>
        /// writes a byte array to the binary writer
        /// </summary>
        public void WriteBytes(byte[] buffer, int offset, int count)
        {
            _writer.Write(buffer, offset, count);
        }

        /// <summary>
        /// writes an integer to the binary writer
        /// </summary>
        public void WriteInt(int value)
        {
            uint num = (uint)value;

            while (num >= 128U)
            {
                _writer.Write((byte)(num | 128U));
                num >>= 7;
            }

            _writer.Write((byte)num);
        }

        /// <summary>
        /// writes a long to the binary writer
        /// </summary>
        public void WriteLong(long value)
        {
            ulong num = (ulong)value;

            while (num >= 128U)
            {
                _writer.Write((byte)(num | 128U));
                num >>= 7;
            }

            _writer.Write((byte)num);
        }

        /// <summary>
        /// writes an ASCII string to the binary writer
        /// </summary>
        public void WriteAsciiString(string s)
        {
            int numBytes = s?.Length ?? 0;
            WriteInt(numBytes);

            // sanity check: handle null strings
            if (s == null) return;

            // write the ASCII bytes
            WriteBytes(Encoding.ASCII.GetBytes(s));
        }

        /// <summary>
        /// writes an utf-8 string to the binary writer
        /// </summary>
        public void WriteUtf8String(string s)
        {
            if (s == null)
            {
                WriteInt(0);
                return;
            }
            var encodedBytes = Encoding.UTF8.GetBytes(s);

            WriteInt(encodedBytes.Length);

            // write the utf-8 bytes
            WriteBytes(encodedBytes);
        }
    }
}