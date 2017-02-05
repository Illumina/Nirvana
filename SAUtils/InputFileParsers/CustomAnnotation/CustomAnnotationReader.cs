using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;

namespace SAUtils.InputFileParsers.CustomAnnotation
{
	public sealed class CustomAnnotationReader: IEnumerable<CustomItem>
	{
		private readonly FileInfo _customFileInfo;
		private readonly Dictionary<string, string> _stringFields;
		private readonly Dictionary<string, string> _stringValues;
		private readonly Dictionary<string, string> _boolFields;
		private readonly List<string> _boolValues;
		private string _topKey;
		private bool _isPositional;

	    private readonly ChromosomeRenamer _renamer;
		private readonly List<CustomItem> _customItemList;

		public CustomAnnotationReader(FileInfo customFileInfo, ChromosomeRenamer renamer)
		{
            _customFileInfo = customFileInfo;
            _stringFields   = new Dictionary<string, string>();
            _stringValues   = new Dictionary<string, string>();
            _boolFields     = new Dictionary<string, string>();
            _boolValues     = new List<string>();
            _customItemList = new List<CustomItem>();
            _isPositional   = false;
            _renamer        = renamer;
        }

        private void Clear()
		{
			_stringValues.Clear();
			_boolValues.Clear();
			_customItemList.Clear();
		}
	
		public IEnumerator<CustomItem> GetEnumerator()
		{
			return GetCustomItems().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		private IEnumerable<CustomItem> GetCustomItems()
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
						_isPositional = value == "Position";
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
					_stringFields[info] = json;
					break;
				case "Boolean":
					_boolFields[info] = json;
					break;
				default:
					throw new Exception("Unsupported data type: "+type+ " in:\n"+ line);
			}
			
		}

		void ParseHeaderLine(string line)
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

			var chromosome = splitLine[VcfCommon.ChromIndex];
			if (!InputFileParserUtilities.IsDesiredChromosome(chromosome, _renamer)) return null;
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
					stringValues[keyValue.Key] = keyValue.Value.Split(',')[i];
				}

				var boolValues   = _boolValues.Select(value => value.Split(',')[i]).ToList();

				_customItemList.Add(new CustomItem(
					chromosome,
					position,
					refAllele,
					altAlleles[i],
					_topKey,
					id,
					_isPositional,
					stringValues,
					boolValues
					));
			}
			return _customItemList;
		}

		void ParseInfoField(string infoFields)
		{
			// 1       69345   COSM911918      C       A       .       .       GENE=OR4F5;STRAND=+;CDS=c.255C>A;AA=p.I85I;CNT=1;EX_TARGET
			foreach (var infoField in infoFields.Split(';'))
			{
				var keyValue = infoField.Split('=');
				var key = keyValue[0];
				if (keyValue.Length == 1)
				{
					// undefined boolean keys will be skipped
					if (_boolFields.ContainsKey(key))
						_boolValues.Add(_boolFields[key]);
					continue;
				}

				var value = keyValue[1];

				// undefined string keys will be skipped
				if (_stringFields.ContainsKey(key))
					_stringValues[_stringFields[key]] = value;
			}
		}

	}
}
