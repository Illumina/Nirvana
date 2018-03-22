using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.IO;

namespace Jasix.DataStructures
{
	public sealed class JasixIndex
	{
		private readonly Dictionary<string, JasixChrIndex> _chrIndices;
		public string HeaderLine;

		// the json file might contain sections. We want to be able to index these sections too

		public JasixIndex()
		{
			_chrIndices = new Dictionary<string, JasixChrIndex>();
		}

		private JasixIndex(IExtendedBinaryReader reader):this()
		{
			var version = reader.ReadOptInt32();
			if (version != JasixCommons.Version)
				throw new InvalidDataException($"Invalid Jasix version: Observed {version}, expected{JasixCommons.Version}");

			HeaderLine = reader.ReadAsciiString();
			var count = reader.ReadOptInt32();

			for (var i = 0; i < count; i++)
			{
				var chrIndex = new JasixChrIndex(reader);
				_chrIndices[chrIndex.ReferenceSequence]= chrIndex;
			}
		}

		public JasixIndex(Stream stream) : this(new ExtendedBinaryReader(stream))
		{
		}

		public void Write(Stream writeStream)
		{
			var writer = new ExtendedBinaryWriter(writeStream);
			writer.WriteOpt(JasixCommons.Version);

			writer.WriteOptAscii(HeaderLine);

			writer.WriteOpt(_chrIndices.Count);
			if (_chrIndices.Count == 0) return;
			
			foreach (var chrIndex in _chrIndices.Values)
			{
				chrIndex.Write(writer);
			}
		}

		public void Flush()
		{
			foreach (var chrIndex in _chrIndices.Values)
			{
				chrIndex.Flush();
			}
		}

		public void Add(string chr, int start, int end, long fileLoc)
		{

			if (!_chrIndices.ContainsKey(chr))
			{
				_chrIndices[chr] = new JasixChrIndex(chr);
			}

			_chrIndices[chr].Add(start, end, fileLoc);
		}

		
		//returns file location of the first node that overlapping the given position chr:start-end
		public long GetFirstVariantPosition(string chr, int start, int end)
		{
			if (_chrIndices == null || _chrIndices.Count == 0) return -1;

			if (!_chrIndices.ContainsKey(chr)) return -1;
			
			return _chrIndices[chr].FindFirstSmallVariant(start, end);

		}


		public long[] LargeVariantPositions(string chr, int begin, int end)
		{
			if (_chrIndices == null || _chrIndices.Count == 0) return null;

			return !_chrIndices.ContainsKey(chr) ? null : _chrIndices[chr].FindLargeVariants(begin, end);
		}

		public IEnumerable<string> GetChromosomeList()
		{
			return _chrIndices.Keys;
		}

	    public bool ContainsChr(string chr)
	    {
	        return _chrIndices.Keys.Contains(chr);
	    }
	}
}
