using System;
using System.Collections.Generic;
using System.IO;
using Compression.Utilities;
using SAUtils.DataStructures;
using SAUtils.Interface;
using VariantAnnotation.Utilities;

namespace SAUtils.InputFileParsers.IntermediateAnnotation
{
    // making this class a disposable is not recommneded for the following reasons
    // multiple threads access different parts of a iTSV file simultaneously. So having one stream doesn't work.
    // instead, each thread is handed an enumerator which has its own stream that it disposes upon use
    public sealed class ParallelSaTsvReader:ITsvReader
    {
        public SaHeader SaHeader => GetHeader();
        public IEnumerable<string> RefNames => _refNameOffsets.Keys;

		private readonly Dictionary<string, long> _refNameOffsets;
        private readonly string _fileName;
        
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

	    public ParallelSaTsvReader(string fileName)
	    {
	        _fileName = fileName;
	        using (var tsvIndex = new TsvIndex(new BinaryReader(FileUtilities.GetReadStream(_fileName + TsvIndex.FileExtension))))
	        {
	            _refNameOffsets = tsvIndex.TagPositions;
	        }
        }

        private SaHeader GetHeader()
        {
            SaHeader header;
            using (var reader = GZipUtilities.GetAppropriateStreamReader(_fileName))
            {
                header = ReadHeader(reader);
            }

            return header;
        }

        private SmallAnnotationHeader ReadHeader(StreamReader reader)
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
	            return new SmallAnnotationHeader(_name, _genomeAssembly, _version, _releaseDate, _description, _matchByAllele);

            Console.WriteLine($"Insufficient version information for {_name}");
	        return null;
	    }

        public IEnumerable<InterimSaItem> GetItems(string refName)
        {
            if (!_refNameOffsets.ContainsKey(refName)) yield break;

            var offset = _refNameOffsets[refName];

            using (var reader = GZipUtilities.GetAppropriateStreamReader(_fileName))
            {
                reader.BaseStream.Position = offset;
                string line;
                while ((line = reader.ReadLine()) != null)
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
                        line = reader.ReadLine();
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

        
	}
}
