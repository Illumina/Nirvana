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
	    private readonly Dictionary<string, string> _synonymToChrName;
		public string HeaderLine;

		// the json file might contain sections. We want to be able to index these sections too

		public JasixIndex()
		{
			_chrIndices = new Dictionary<string, JasixChrIndex>();
            _synonymToChrName = new Dictionary<string, string>();
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

		    int synonymCount = reader.ReadOptInt32();
		    if (synonymCount == 0) return;
		    for (var i = 0; i < synonymCount; i++)
		    {
		        string synonym = reader.ReadAsciiString();
		        string indexName = reader.ReadAsciiString();
		        _synonymToChrName[synonym] = indexName;
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

            writer.WriteOpt(_synonymToChrName.Count);
		    if (_synonymToChrName.Count == 0) return;
		    foreach (var pair in _synonymToChrName)
		    {
		        writer.Write(pair.Key);
		        writer.Write(pair.Value);
            }
		}

		public void Flush()
		{
			foreach (var chrIndex in _chrIndices.Values)
			{
				chrIndex.Flush();
			}
		}

		public void Add(string chr, int start, int end, long fileLoc, string chrSynonym=null)
		{
		    if (!string.IsNullOrEmpty(chrSynonym))
		    {
		        _synonymToChrName[chrSynonym] = chr;
		    }

		    if (_chrIndices.TryGetValue(chr, out var chrIndex))
		    {
                chrIndex.Add(start, end, fileLoc);
		    }
		    else
		    {
		        _chrIndices[chr] = new JasixChrIndex(chr);
		        _chrIndices[chr].Add(start, end, fileLoc);

            }

		}

		
		//returns file location of the first node that overlapping the given position chr:start-end
		public long GetFirstVariantPosition(string chr, int start, int end)
		{
			if (_chrIndices == null || _chrIndices.Count == 0) return -1;

		    if (_synonymToChrName.TryGetValue(chr, out string indexName))
		        chr = indexName;

		    if (_chrIndices.TryGetValue(chr, out var chrIndex))
		    {
		        return chrIndex.FindFirstSmallVariant(start, end);
		    }
		    return -1;

		}


		public long[] LargeVariantPositions(string chr, int begin, int end)
		{
			if (_chrIndices == null || _chrIndices.Count == 0) return null;

		    if (_synonymToChrName.TryGetValue(chr, out string indexName))
		        chr = indexName;

		    return _chrIndices.TryGetValue(chr, out var chrIndex) ? chrIndex.FindLargeVariants(begin, end) : null;
		}

		public IEnumerable<string> GetChromosomeList()
		{
			return _chrIndices.Keys;
		}

	    public bool ContainsChr(string chr)
	    {
	        if (_synonymToChrName.TryGetValue(chr, out string indexName))
	            return _chrIndices.Keys.Contains(indexName);
            return _chrIndices.Keys.Contains(chr);
	    }

	    public string GetIndexChromName(string chromName)
	    {
	        if (_chrIndices.ContainsKey(chromName)) return chromName;
	        return _synonymToChrName.TryGetValue(chromName, out string indexName) ? indexName : null;
	    }
	}
}
