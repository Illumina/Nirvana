using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using Compression.FileHandling;

namespace SAUtils.InputFileParsers.IntermediateAnnotation
{
	public class TabixReader
	{
		public  static Dictionary<string, ulong> ReadTabixIdex(Stream idxFileStream)
		{
			Dictionary<string, ulong> refNameOffsets = new Dictionary<string, ulong>();

			using (var reader = new BinaryReader(new BlockGZipStream(idxFileStream, CompressionMode.Decompress)))
			{
				var buf = new byte[4];
				reader.Read(buf, 0, 4); // read "TBI\1"
				var sequences = new string[ReadInt32(reader)];

				ReadInt32(reader);
				ReadInt32(reader);
				ReadInt32(reader);
				ReadInt32(reader);
				ReadInt32(reader);

				ReadInt32(reader); // Number of lines to skip at the beginning, not supported.
				// read sequence dictionary
				int i, j, k, l = ReadInt32(reader);
				buf = reader.ReadBytes(l);
				for (i = j = k = 0; i < buf.Length; ++i)
				{
					if (buf[i] == 0)
					{
						var b = new byte[i - j];
						Array.Copy(buf, j, b, 0, b.Length);
						var s = Encoding.UTF8.GetString(b);
						sequences[k++] = s;
						j = i + 1;
					}
				}
				// read the index
				for (i = 0; i < sequences.Length; ++i)
				{
					ulong minOffset = ulong.MaxValue;
					// the binning index
					var binCount = ReadInt32(reader);
					for (j = 0; j < binCount; ++j)
					{
						var bin = ReadUInt32(reader);
						var nchunks = ReadInt32(reader);
						for (k = 0; k < nchunks; ++k)
						{
							var u = ReadUInt64(reader);
							var v = ReadUInt64(reader);
							minOffset = u < minOffset ? u : minOffset;
						}
					}
					// the linear index
					var nLinerIndex = ReadInt32(reader);
					for (k = 0; k < nLinerIndex; ++k)
					{
						var offset = ReadUInt64(reader);
						//minOffset = offset < minOffset ? offset : minOffset;
					}

					refNameOffsets[sequences[i]] = minOffset;
				}
			}

			return refNameOffsets;
		}



	private static byte[] ReadBytes(BinaryReader inputStream, int numberOfBytesToRead)
		{
			var buffer = inputStream.ReadBytes(numberOfBytesToRead);
			if (!BitConverter.IsLittleEndian) Array.Reverse(buffer);
			return buffer;
		}

		private static int ReadInt32(BinaryReader inputStream)
		{
			var buffer = ReadBytes(inputStream, 4);
			return BitConverter.ToInt32(buffer, 0);
		}

		private static uint ReadUInt32(BinaryReader inputStream)
		{
			var buffer = ReadBytes(inputStream, 4);
			return BitConverter.ToUInt32(buffer, 0);
		}

		private static ulong ReadUInt64(BinaryReader inputStream)
		{
			var buffer = ReadBytes(inputStream, 8);
			return BitConverter.ToUInt64(buffer, 0);
		}

	}
}
