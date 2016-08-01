using System;
using System.IO;
using System.Text;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;

namespace VariantAnnotation.FileHandling
{
    public class ExtendedBinaryReader
    {
        #region members

        private readonly BinaryReader _reader;

        #endregion

        // constructor
        public ExtendedBinaryReader(BinaryReader reader)
        {
            _reader = reader;
        }

        /// <summary>
        /// returns a boolean from the binary reader
        /// </summary>
        public bool ReadBoolean()
        {
            return _reader.ReadBoolean();
        }

        /// <summary>
        /// returns a byte from the binary reader
        /// </summary>
        public byte ReadByte()
        {
            return _reader.ReadByte();
        }

        public double ReadDouble()
        {
            //note that our float is limited to 9 digit precision
            var i = ReadInt();
            var f = 1.0 * i / SupplementaryAnnotation.PrecisionConst;
            return f;
        }

        /// <summary>
        /// returns a byte array from the binary reader
        /// </summary>
        public byte[] ReadBytes(int numBytes)
        {
            var buffer    = new byte[numBytes];
            int byteCount = _reader.Read(buffer, 0, numBytes);

            if (byteCount != numBytes) throw new EndOfStreamException();

            return buffer;
        }

        /// <summary>
        /// returns an integer from the binary reader
        /// </summary>
        public int ReadInt()
        {
            int num1 = 0;
            int num2 = 0;

            while (num2 != 35)
            {
                byte num3 = _reader.ReadByte();
                num1 |= (num3 & sbyte.MaxValue) << num2;
                num2 += 7;

                if ((num3 & 128) == 0) return num1;
            }

            throw new FormatException("Unable to read the 7-bit encoded integer");
        }

        /// <summary>
        /// returns a long from the binary reader
        /// </summary>
        public long ReadLong()
        {
            long num1 = 0;
            int num2 = 0;

            while (num2 != 70)
            {
                byte num3 = _reader.ReadByte();
                num1 |= (long)(num3 & sbyte.MaxValue) << num2;
                num2 += 7;

                if ((num3 & 128) == 0) return num1;
            }

            throw new FormatException("Unable to read the 7-bit encoded long");
        }

        /// <summary>
        /// returns an ASCII string from the binary reader
        /// </summary>
        public string ReadAsciiString()
        {
            int numBytes = ReadInt();

            // sanity check: handle null strings
            if (numBytes == 0) return null;

            // grab the ASCII characters
            return Encoding.ASCII.GetString(ReadBytes(numBytes));
        }

        /// <summary>
        /// returns an UTF-8 string from the binary reader
        /// </summary>
        public string ReadUtf8String()
        {
            int numBytes = ReadInt();

            // sanity check: handle null strings
            if (numBytes == 0) return null;

            // grab the utf-8 characters
            return Encoding.UTF8.GetString(ReadBytes(numBytes));
        }
    }
}