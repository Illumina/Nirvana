using System;
using System.IO;
using System.Text;
using ErrorHandling.Exceptions;

namespace VariantAnnotation.FileHandling
{
    public sealed class ExtendedBinaryReader : BinaryReader
    {
        #region members

        private readonly Stream _stream;

        // allows 9-digit precision for floating-point numbers
        internal const int PrecisionConst = 1000000000;

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public ExtendedBinaryReader(Stream s) : this(s, new UTF8Encoding()) { }

        /// <summary>
        /// constructor
        /// </summary>
        public ExtendedBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
            _stream = input;
        }

        /// <summary>
        /// returns a nullable (7-bit) byte from the binary reader
        /// </summary>
        public byte? ReadOptNullByte()
        {
            if (_stream == null) throw new GeneralException("File not open");

            int i = _stream.ReadByte();
            if (i == -1) throw new EndOfStreamException();

            var b = (byte)i;
            if ((b & 128) != 0) return null;
            return (byte)(b & 127);
        }

        /// <summary>
        /// returns a double-precision floating-point number from the binary reader
        /// </summary>
        public double ReadOptDouble()
        {
            if (_stream == null) throw new GeneralException("File not open");

            var i = ReadOptInt32();
            var f = 1.0 * i / PrecisionConst;
            return f;
        }

        /// <summary>
        /// returns a single-precision floating-point number from the binary reader
        /// </summary>
        public float ReadOptSingle()
        {
            return (float)ReadOptDouble();
        }

        /// <summary>
        /// returns an integer from the binary reader
        /// </summary>
        public int ReadOptInt32()
        {
            if (_stream == null) throw new GeneralException("File not open");

            int count = 0;
            int shift = 0;

            while (shift != 35)
            {
                byte b = ReadByte();
                count |= (b & sbyte.MaxValue) << shift;
                shift += 7;

                if ((b & 128) == 0) return count;
            }

            throw new FormatException("Unable to read the 7-bit encoded integer");
        }

        /// <summary>
        /// returns an integer from the binary reader
        /// </summary>
        public int? ReadOptNullableInt32()
        {
            if (_stream == null) throw new GeneralException("File not open");

            var value = ReadOptInt32();
            return value == -1 ? null : (int?)value;
        }

        /// <summary>
        /// returns an unsigned integer from the binary reader
        /// </summary>
        public uint ReadOptUInt32()
        {
            return (uint)ReadOptInt32();
        }

        /// <summary>
        /// returns a long from the binary reader
        /// </summary>
        public long ReadOptInt64()
        {
            if (_stream == null) throw new GeneralException("File not open");

            long count = 0;
            int shift = 0;

            while (shift != 70)
            {
                byte b = ReadByte();
                count |= (long)(b & sbyte.MaxValue) << shift;
                shift += 7;

                if ((b & 128) == 0) return count;
            }

            throw new FormatException("Unable to read the 7-bit encoded long");
        }

        public T[] ReadOptArray<T>(Func<T> readOptFunc)
        {
            if (_stream == null) throw new GeneralException("File not open");

            var count = ReadOptInt32();
            if (count == 0) return null;

            var values = new T[count];
            for (var i = 0; i < count; i++) values[i] = readOptFunc();
            return values;
        }

		/// <summary>
		/// returns an ASCII string from the binary reader
		/// </summary>
		public string ReadAsciiString()
        {
            if (_stream == null) throw new GeneralException("File not open");

            int numBytes = ReadOptInt32();

            // grab the ASCII characters
            // ReSharper disable once AssignNullToNotNullAttribute
            return numBytes == 0 ? null : Encoding.ASCII.GetString(ReadBytes(numBytes));
        }

        /// <summary>
        /// returns an UTF-8 string from the binary reader
        /// </summary>
        public string ReadUtf8String()
        {
            int numBytes = ReadOptInt32();

            // grab the UTF8 characters
            return numBytes == 0 ? null : Encoding.UTF8.GetString(ReadBytes(numBytes));
        }
    }
}