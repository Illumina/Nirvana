using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.DataStructures;
using SAUtils.Interface;

namespace SAUtils.InputFileParsers.IntermediateAnnotation
{
    public sealed class SaTsvReader:ITsvReader
    {
        public SaHeader SaHeader => _header;
        public IEnumerable<string> RefNames => _refNameOffsets.Keys;

        private readonly StreamReader _tsvReader;
		private readonly Dictionary<string, long> _refNameOffsets;

        private readonly SmallAnnotationsHeader _header;
        private string _name;
		private string _genomeAssembly;
		private string _version;
		private string _releaseDate;
		private string _description;
		private string _jsonKey;
		private string _vcfKey;
		private bool _matchByAllele;
		private bool _isArray;

	    private const int ChrIndex        = 0;
	    private const int PositionIndex   = 1;
	    private const int RefAlleleIndex  = 2;
	    private const int AltAlleleIndex  = 3;
	    private const int VcfStringIndex  = 4;
	    private const int JsonStringIndex = 5;

	    private const int MinNoOfColumns  = 5;

	    public SaTsvReader(StreamReader reader, Stream indexFileStream)
	    {
            using (var tsvIndex = new TsvIndex(new BinaryReader(indexFileStream)))
            {
                _refNameOffsets = tsvIndex.TagPositions;
            }
	        _tsvReader = reader;
	        _header = ReadHeader(_tsvReader);
	    }

	    private SmallAnnotationsHeader ReadHeader(StreamReader reader)
	    {
	        string line;
	        while ((line = reader.ReadLine()) != null)
	        {
	            // Skip empty lines.
	            if (string.IsNullOrWhiteSpace(line)) continue;

	            if (!line.StartsWith("#")) break;

	            ParseHeaderLine(line);
	        }

	        if (!string.IsNullOrEmpty(_name) 
                && !string.IsNullOrEmpty(_version) 
                && !string.IsNullOrEmpty(_releaseDate) 
                && !string.IsNullOrEmpty(_jsonKey))
	            return new SmallAnnotationsHeader(_name, _genomeAssembly, _version, _releaseDate, _description, _matchByAllele);

            Console.WriteLine($"Insufficient version information for {_name}");
	        return null;
	    }

	   public IEnumerable<InterimSaItem> GetAnnotationItems(string refName)
		{
			if (!_refNameOffsets.ContainsKey(refName)) yield break;

			var offset = _refNameOffsets[refName];

		    _tsvReader.BaseStream.Position = offset;
		    string line;
		    while ((line = _tsvReader.ReadLine()) != null)
		    {
		        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
		        // finding desired chromosome. We need this because the GetLocation for GZipStream may return a position a few lines before the start of the chromosome
		        if (line.StartsWith(refName + "\t")) break;
		    }
		    if (line == null) yield break;
		    string lastLine = line;
		    do
		    {
		        //next chromosome
		        if (!line.StartsWith(refName + "\t")) yield break;

		        var annotationItem = ExtractItem(line);
		        if (annotationItem == null) continue;

		        yield return annotationItem;
		        try
		        {
		            line = _tsvReader.ReadLine();
		        }
		        catch (Exception)
		        {
		            Console.WriteLine("error while reading line in while loop. Last line read:");
		            Console.WriteLine(lastLine);
		            throw;
		        }
		        lastLine = line;
		    } while (line != null);
        }


		private InterimSaItem ExtractItem(string line)
		{
			var columns = line.Split('\t');
			if ( columns.Length < MinNoOfColumns)
				throw new InvalidDataException("Line contains too few columns:\n"+line);

			var chromosome = columns[ChrIndex];
            // position, ref and alt are in VCF style. We need to reduce them to our internal representation.
            var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(int.Parse(columns[PositionIndex]), columns[RefAlleleIndex], columns[AltAlleleIndex]);

            var position  = newAlleles.Item1;
            var refAllele = newAlleles.Item2;
            var altAllele = newAlleles.Item3;

            var vcfString = columns[VcfStringIndex];

		    var jsonStrings = new List<string>();
		    for (int i = JsonStringIndex; i < columns.Length; i++)
		        jsonStrings.Add(columns[i]);

		    return new InterimSaItem(_jsonKey, _vcfKey, chromosome, position, refAllele, altAllele, _matchByAllele, _isArray,
		        vcfString, jsonStrings.ToArray());
		}

		private void ParseHeaderLine(string line)
		{
			var words = line.Split('=');
			if (words.Length < 2) return;

			var key = words[0];
			var value = words[1];

			switch (key)
			{
				case "#name":
					_name = value;
					break;
				case "#assembly":
					_genomeAssembly = value;
					break;
				case "#version":
					_version = value;
					break;
				case "#releaseDate":
					_releaseDate = value;
					break;
				case "#description":
					_description = value;
					break;
				case "#jsonKey":
					_jsonKey = value;
					break;
				case "#vcfKeys":
					_vcfKey = value;
					break;
				case "#matchByAllele":
					_matchByAllele = bool.Parse(value);
					break;
				case "#isArray":
					_isArray = bool.Parse(value);
					break;
				case "#schemaVersion":
					var schemaVersion = int.Parse(value);
					if (schemaVersion != SaTsvCommon.SupplementarySchemaVersion)
						throw new InvalidDataException($"Expected Schema version:{SaTsvCommon.SupplementarySchemaVersion}, oberved: {value}");
					break;
			}
		}

		public List<string> GetAllRefNames()
		{
			return _refNameOffsets.Keys.ToList();
		}


	    public void Dispose()
	    {
	        _tsvReader.Dispose();
	    }

	    
	}
}
