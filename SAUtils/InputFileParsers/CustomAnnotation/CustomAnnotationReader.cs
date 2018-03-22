using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Utilities;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.InputFileParsers.CustomAnnotation
{
	public sealed class CustomAnnotationReader
	{
		private readonly FileInfo _customFileInfo;
		private readonly Dictionary<string, string> _desiredStringFields;
		private readonly Dictionary<string, string> _desiredBoolFields;
		private readonly Dictionary<string, string> _desiredNumberFields;

		private readonly Dictionary<string, List<string>> _stringValues;
		private readonly Dictionary<string, List<double>> _numberValues;
		private readonly List<string> _boolValues;

		private string _topKey;
	    public bool IsPositional { get; private set; }

	    private readonly List<CustomItem> _customItemList;
	    private readonly IDictionary<string, IChromosome> _refChromDict;


        public CustomAnnotationReader(FileInfo customFileInfo, IDictionary<string, IChromosome> refChromDict)
		{
			_customFileInfo       = customFileInfo;
		    _refChromDict         = refChromDict;
			_desiredStringFields  = new Dictionary<string, string>();
			_desiredNumberFields  = new Dictionary<string, string>();
			_desiredBoolFields    = new Dictionary<string, string>();

			_stringValues         = new Dictionary<string, List<string>>();
			_numberValues         = new Dictionary<string, List<double>>();
			_boolValues           = new List<string>();

			_customItemList       = new List<CustomItem>();
			IsPositional          = false;

		    ReadHeader();
		}

	    private void ReadHeader()
	    {
            using (var reader = GZipUtilities.GetAppropriateStreamReader(_customFileInfo.FullName))
            {
                string line;

                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    if (line.StartsWith("#"))
                    {
                        ParseHeaderLine(line);
                    }
                    else break;
                }
            }
        }

	    private void Clear()
		{
			_stringValues.Clear();
			_numberValues.Clear();
			_boolValues.Clear();
			_customItemList.Clear();
		}
	
		public IEnumerable<CustomItem> GetCustomItems()
		{
			using (var reader = GZipUtilities.GetAppropriateStreamReader(_customFileInfo.FullName))
			{
				string line;
				
				while ((line = reader.ReadLine()) != null)
				{
					// Skip empty lines.
					if (string.IsNullOrWhiteSpace(line)) continue;
					if (line.StartsWith("#"))
					{
						continue;
					}
					var customItemsList = ExtractCustomItems(line);
					if (customItemsList == null) continue;
					foreach (var customItem in customItemsList)
					{
						yield return customItem;
					}

				}
			}
		}

		private void GetTopLevelKey(string line)
		{
			//##IAE_TOP=<KEY=cosmic,MATCH=Allele>
			line = line.Substring(11);//removing ##IAE_TOP=<
			line = line.Substring(0, line.Length - 1);//removing the last '>'
			var fields= line.Split(',');

			foreach (var field in fields)
			{
				var keyValue = field.Split('=');
				var key = keyValue[0];
				var value = keyValue[1];
				switch (key)
				{
					case "KEY":
						_topKey = value;
						break;
					case "MATCH":
						// default is allele specific
						IsPositional = value == "Position";
						break;
					default:
						throw new Exception("Unknown field in top level key line :\n "+ line);

				}
			}
			
		}

		private void AddInfoField(string line)
		{
			//##IAE_INFO=<INFO=GENE,Type=String,JSON=gene>
			line = line.Substring(12);//removing ##IAE_INFO=<
			line = line.Substring(0, line.Length - 1);//removing the last '>'

			var fields = line.Split(',');

			string info=null, type=null, json=null;
				
			foreach (var field in fields)
			{
				var keyValue = field.Split('=');

				if (keyValue.Length!=2)
					throw new Exception("Invalid info field: "+field);

				var key   = keyValue[0];
				var value = keyValue[1];

				switch (key)
				{
					case "INFO":
						info = value;
						break;
					case "Type":
						type = value;
						break;
					case "JSON":
						json = value;
						break;
					default:
						throw new Exception("Unknown field in info field line :\n" + line);

				}
			}

			if (type==null || info==null || json ==null)
				throw new Exception("Missing mandatory field from IAE_INFO:\n"+line);

			switch (type)
			{
				case "String":
					_desiredStringFields[info] = json;
					break;
				case "Number":
					_desiredNumberFields[info] = json;
					break;
				case "Boolean":
					_desiredBoolFields[info] = json;
					break;
				default:
					throw new Exception("Unsupported data type: "+type+ " in:\n"+ line);
			}
			
		}

	    private void ParseHeaderLine(string line)
		{
			//##IAE_TOP=<KEY=cosmic,MATCH=Allele>
			if (line.StartsWith("##IAE_TOP=")) GetTopLevelKey(line);

			//##IAE_INFO=<INFO=GENE,Type=String,JSON=gene>
			if (line.StartsWith("##IAE_INFO=")) AddInfoField(line);
		}

	    private List<CustomItem> ExtractCustomItems(string vcfline)
		{
			if (vcfline == null) return null;
			var splitLine = vcfline.Split('\t');// we don't care about the many fields after info field

			if (splitLine.Length < 8) return null;

		    var chromosomeName = splitLine[VcfCommon.ChromIndex];
		    if (!_refChromDict.ContainsKey(chromosomeName)) return null;

		    var chromosome = _refChromDict[chromosomeName];

            var position   = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
			var refAllele  = splitLine[VcfCommon.RefIndex];
			var altAlleles = splitLine[VcfCommon.AltIndex].Split(',');
			var id         = splitLine[VcfCommon.IdIndex];
			var infoFields = splitLine[VcfCommon.InfoIndex];

			// parses the info fields and extract frequencies, coverage, num samples.
			Clear();//clearing the entry specific values from dictionary
			ParseInfoField(infoFields);

			if (_stringValues.Count == 0 && _boolValues.Count == 0)
				return null;
			
			for (int i = 0; i < altAlleles.Length; i++)
			{
				var stringValues = new Dictionary<string, string>();
				foreach (var keyValue in _stringValues)
				{
					stringValues[keyValue.Key] = keyValue.Value[i];
				}

				var numberValues = new Dictionary<string, double>();
				foreach (var keyValue in _numberValues)
				{
					numberValues[keyValue.Key] = keyValue.Value[i];
				}

				var boolValues   = _boolValues.Select(value => value.Split(',')[i]).ToList();

				_customItemList.Add(new CustomItem(
					chromosome,
					position,
					refAllele,
					altAlleles[i],
					_topKey,
					id,
					stringValues,
					numberValues,
					boolValues
					));
			}
			return _customItemList;
		}

	    private void ParseInfoField(string infoFields)
		{
			// 1       69345   COSM911918      C       A       .       .       GENE=OR4F5;STRAND=+;CDS=c.255C>A;AA=p.I85I;CNT=1;EX_TARGET
			foreach (var infoField in infoFields.Split(';'))
			{
				var keyValue = infoField.Split('=');
				var key = keyValue[0];
				if (keyValue.Length == 1)
				{
					// undefined boolean keys will be skipped
					if (_desiredBoolFields.ContainsKey(key))
						_boolValues.Add(_desiredBoolFields[key]);
					continue;
				}

				var value = keyValue[1];

				// undefined string keys will be skipped
				if (_desiredStringFields.ContainsKey(key))
					_stringValues[_desiredStringFields[key]] = value.Split(',').ToList();

				// undefined number keys will be skipped
				if (_desiredNumberFields.ContainsKey(key))
					_numberValues[_desiredNumberFields[key]] = value.Split(',').Select(double.Parse).ToList();
			}
		}

	}
}
