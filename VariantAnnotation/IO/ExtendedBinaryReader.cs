using System;
using System.IO;
using System.Text;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.IO
{
	public sealed class ExtendedBinaryReader : BinaryReader, IExtendedBinaryReader
	{
		public ExtendedBinaryReader(Stream s) : this(s, new UTF8Encoding()) { }

		public ExtendedBinaryReader(Stream input, Encoding encoding, bool leaveOpen = false)
			: base(input, encoding, leaveOpen) {}


	    /// <summary>
	    /// returns an unsigned short from the binary reader
	    /// </summary>
	    public ushort ReadOptUInt16()
	    {
	        ushort count = 0;
	        int shift = 0;

	        while (shift != 21)
	        {
	            byte b = ReadByte();
	            count |= (ushort)((b & sbyte.MaxValue) << shift);
	            shift += 7;

	            if ((b & 128) == 0) return count;
	        }

	        throw new FormatException("Unable to read the 7-bit encoded unsigned short");
	    }

        /// <summary>
        /// returns an integer from the binary reader
        /// </summary>
        public int ReadOptInt32()
		{
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
		/// returns a long from the binary reader
		/// </summary>
		public long ReadOptInt64()
		{
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
			int numBytes = ReadOptInt32();

			// grab the ASCII characters
			// ReSharper disable once AssignNullToNotNullAttribute
			return numBytes == 0 ? null : Encoding.ASCII.GetString(ReadBytes(numBytes));
		}
	}
}