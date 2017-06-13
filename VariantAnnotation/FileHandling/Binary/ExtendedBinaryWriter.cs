using System;
using System.IO;
using System.Text;
using ErrorHandling.Exceptions;
using VariantAnnotation.Interface;

namespace VariantAnnotation.FileHandling.Binary
{
    public sealed class ExtendedBinaryWriter : BinaryWriter, IExtendedBinaryWriter
    {
        #region members

        private readonly Stream _stream;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public ExtendedBinaryWriter(Stream output) : this(output, new UTF8Encoding(false, true)) { }

        /// <summary>
        /// constructor
        /// </summary>
        public ExtendedBinaryWriter(Stream output, Encoding encoding, bool leaveOpen = false)
            : base(output, encoding, leaveOpen)
        {
            _stream = output;
        }

        /// <summary>
        /// writes a nullable byte to the binary writer
        /// </summary>
        public void WriteOpt(byte? b)
        {
            if (_stream == null) throw new GeneralException("File not open");
            Write(b == null ? (byte)128 : (byte)(b.Value & 127));
        }

        /// <summary>
        /// writes an integer to the binary writer
        /// </summary>
        public void WriteOpt(int value)
        {
            if (_stream == null) throw new GeneralException("File not open");
            uint num = (uint)value;

            while (num >= 128U)
            {
                Write((byte)(num | 128U));
                num >>= 7;
            }

            Write((byte)num);
        }

        /// <summary>
        /// writes a long to the binary writer
        /// </summary>
        public void WriteOpt(long value)
        {
            if (_stream == null) throw new GeneralException("File not open");
            ulong num = (ulong)value;

            while (num >= 128U)
            {
                Write((byte)(num | 128U));
                num >>= 7;
            }

            Write((byte)num);
        }

        /// <summary>
        /// writes an ASCII string to the binary writer
        /// </summary>
        public void WriteOptAscii(string s)
        {
            if (_stream == null) throw new GeneralException("File not open");
            int numBytes = s?.Length ?? 0;
            WriteOpt(numBytes);

            // sanity check: handle null strings
            if (s == null) return;

            // write the ASCII bytes
            Write(Encoding.ASCII.GetBytes(s));
        }

        /// <summary>
        /// writes an UTF8 string to the binary writer
        /// </summary>
        public void WriteOptUtf8(string s)
        {
            if (_stream == null) throw new GeneralException("File not open");
            // sanity check: handle null strings
            if (s == null)
            {
                WriteOpt(0);
                return;
            }

            var encodedBytes = Encoding.UTF8.GetBytes(s);
            WriteOpt(encodedBytes.Length);

            // write the UTF8 bytes
            Write(encodedBytes);
        }

        public void WriteOptArray<T>(T[] values, Action<T> writeOptAction)
        {
            if (_stream == null) throw new GeneralException("File not open");

            if (values == null)
            {
                WriteOpt(0);
                return;
            }

            WriteOpt(values.Length);
            foreach (var v in values) writeOptAction(v);
        }
    }
}