using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Utilities;
using SAUtils.DataStructures;

namespace SAUtils.InputFileParsers.IntermediateAnnotation
{
    public class SaTsvReader:IEnumerable<InterimSaItem>
	{
		private readonly FileInfo _inputFileInfo;
		private readonly Dictionary<string, long> _refNameOffsets;

		private InterimSaHeader _header;
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

		public SaTsvReader(FileInfo inputFileInfo)
		{
			_inputFileInfo = inputFileInfo;
			
			using (var tsvIndex = new TsvIndex(new BinaryReader(File.Open(inputFileInfo.FullName + ".tvi",FileMode.Open,FileAccess.Read,FileShare.Read))))
			{
				_refNameOffsets = tsvIndex.TagPositions;
			}
			

			//set the header information
			using (var reader = GZipUtilities.GetAppropriateStreamReader(_inputFileInfo.FullName))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					// Skip empty lines.
					if (string.IsNullOrWhiteSpace(line)) continue;
					
					if (!line.StartsWith("#")) break;

					ParseHeaderLine(line);
				}
			}
		}


		private IEnumerable<InterimSaItem> GetAnnotationItems(string refName)
		{
			if (!_refNameOffsets.ContainsKey(refName)) yield break;

			var offset = _refNameOffsets[refName];

			using (var reader = GZipUtilities.GetAppropriateStreamReader(_inputFileInfo.FullName))
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
				} while (line  != null);
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

		public InterimSaHeader GetHeader()
		{
			if (_header != null) return _header;

			if (string.IsNullOrEmpty(_name) ||
			    string.IsNullOrEmpty(_version) ||
			    string.IsNullOrEmpty(_releaseDate) ||
			    string.IsNullOrEmpty(_jsonKey)
				)
			{
				Console.WriteLine($"Insufficient version information for {_name}");
				return null;
			}

            _header = new InterimSaHeader( _name, _genomeAssembly, _version, _releaseDate, _description, _matchByAllele);
			return _header;
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
					if (schemaVersion != SaTSVCommon.SupplementarySchemaVersion)
						throw new InvalidDataException($"Expected Schema version:{SaTSVCommon.SupplementarySchemaVersion}, oberved: {value}");
					break;
			}
		}

		public IEnumerator<InterimSaItem> GetEnumerator()
		{
			throw new NotImplementedException();
		}


		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<InterimSaItem> GetEnumerator(string refName)
		{
			return GetAnnotationItems(refName).GetEnumerator();
		}

		public List<string> GetAllRefNames()
		{
			return _refNameOffsets.Keys.ToList();
		}


	}
}
