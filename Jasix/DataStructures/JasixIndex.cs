using System.Collections.Generic;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using IO;

namespace Jasix.DataStructures
{
    public struct FileRange
    {
        public readonly long Begin;
        public long End;

        public FileRange(long begin, long end = long.MaxValue)
        {
            Begin = begin;
            End = end;
        }
    }

    public sealed class JasixIndex
	{
		private readonly Dictionary<string, JasixChrIndex> _chrIndices;
	    private readonly Dictionary<string, string> _synonymToChrName;
	    private readonly Dictionary<string, FileRange> _sectionRanges;

		// the json file might contain sections. We want to be able to index these sections too

		public JasixIndex()
		{
			_chrIndices = new Dictionary<string, JasixChrIndex>();
            _synonymToChrName = new Dictionary<string, string>();
            _sectionRanges = new Dictionary<string, FileRange>();
		}

		private JasixIndex(ExtendedBinaryReader reader):this()
		{
			int version = reader.ReadOptInt32();
			if (version != JasixCommons.Version)
				throw new InvalidDataException($"Invalid Jasix version: Observed {version}, expected{JasixCommons.Version}");

			int count = reader.ReadOptInt32();

			for (var i = 0; i < count; i++)
			{
				var chrIndex = new JasixChrIndex(reader);
				_chrIndices[chrIndex.ReferenceSequence]= chrIndex;
			}

		    int synonymCount = reader.ReadOptInt32();
		    for (var i = 0; i < synonymCount; i++)
		    {
		        string synonym             = reader.ReadAsciiString();
		        string indexName           = reader.ReadAsciiString();
		        _synonymToChrName[synonym] = indexName;
		    }

		    int sectionCount = reader.ReadOptInt32();
		    for (var i = 0; i < sectionCount; i++)
		    {
		        string sectionName = reader.ReadAsciiString();
		        long begin         = reader.ReadOptInt64();
		        long end           = reader.ReadOptInt64();
                _sectionRanges[sectionName] = new FileRange(begin, end);
		    }

		}

		public JasixIndex(Stream stream) : this(new ExtendedBinaryReader(stream))
		{
		}

		public void Write(Stream writeStream)
		{
			var writer = new ExtendedBinaryWriter(writeStream);
			writer.WriteOpt(JasixCommons.Version);

			writer.WriteOpt(_chrIndices.Count);
			foreach (var chrIndex in _chrIndices.Values)
			{
				chrIndex.Write(writer);
			}

            writer.WriteOpt(_synonymToChrName.Count);
		    foreach ((string key, string value) in _synonymToChrName)
		    {
		        writer.Write(key);
		        writer.Write(value);
            }

            writer.WriteOpt(_sectionRanges.Count);
		    foreach ((string name, FileRange sectionRange) in _sectionRanges)
		    {
		        writer.WriteOptAscii(name);
                writer.WriteOpt(sectionRange.Begin);
                writer.WriteOpt(sectionRange.End);
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

	    public void BeginSection(string section, long fileLoc)
	    {
	        if (_sectionRanges.ContainsKey(section)) 
                throw new UserErrorException($"Multiple beginning for section:{section}!!");

            _sectionRanges[section] = new FileRange(fileLoc);
	    }

	    public void EndSection(string section, long fileLoc)
	    {
	        if (!_sectionRanges.TryGetValue(section, out var fileRange))
	            return;
	        //    throw new UserErrorException($"Attempting to close section:{section} before opening it!!");

            if (fileRange.End!=long.MaxValue)
                throw new UserErrorException($"Multiple closing for section{section} !!");

            fileRange.End = fileLoc;
	        _sectionRanges[section] = fileRange;
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
	        return _chrIndices.Keys.Contains(_synonymToChrName.TryGetValue(chr, out string indexName) ? indexName : chr);
	    }

	    public string GetIndexChromName(string chromName)
	    {
	        if (_chrIndices.ContainsKey(chromName)) return chromName;
	        return _synonymToChrName.TryGetValue(chromName, out string indexName) ? indexName : null;
	    }

	    public long GetSectionBegin(string section)
	    {
	        return _sectionRanges[section].Begin;
	    }
	    public long GetSectionEnd(string section)
	    {
	        return _sectionRanges[section].End;
	    }
    }
}
