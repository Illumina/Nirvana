using System.IO;
using System.Text;

namespace IO
{
    public sealed class ExtendedBinaryWriter : BinaryWriter, IExtendedBinaryWriter
    {
        public ExtendedBinaryWriter(Stream output) : this(output, new UTF8Encoding(false, true)) { }

        public ExtendedBinaryWriter(Stream output, Encoding encoding, bool leaveOpen = false)
            : base(output, encoding, leaveOpen)
        {
        }

        /// <summary>
        /// writes an unsigned short to the binary writer
        /// </summary>
        public void WriteOpt(ushort value)
        {
            ushort num = value;

            while (num >= 128U)
            {
                Write((byte)(num | 128U));
                num >>= 7;
            }

            Write((byte)num);
        }

        /// <summary>
        /// writes an integer to the binary writer
        /// </summary>
        public void WriteOpt(int value)
        {
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
            int numBytes = s?.Length ?? 0;
            WriteOpt(numBytes);

            // sanity check: handle null strings
            if (s == null) return;

            // write the ASCII bytes
            Write(Encoding.ASCII.GetBytes(s));
        }
    }
}