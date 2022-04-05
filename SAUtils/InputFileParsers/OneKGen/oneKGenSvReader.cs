using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;

namespace SAUtils.InputFileParsers.OneKGen
{
	public sealed class OneKGenSvReader:IDisposable
    {
        private const int ChromIndex = 0;
        private const int StartIndex = 1;
        private const int EndIndex = 2;
        private const int IdIndex = 3;
        private const int AltIndex = 4;
        private const int InfoIndex = 5;

        private readonly StreamReader _reader;
	    private readonly Dictionary<string, Chromosome> _refNameDict;

	    private string _svType;

        private int? _allAlleleNumber;
	    private int? _allAlleleCount;
        private double? _allAlleleFrequency;
	    private double? _afrAlleleFrequency;
	    private double? _amrAlleleFrequency;
	    private double? _eurAlleleFrequency;
	    private double? _easAlleleFrequency;
	    private double? _sasAlleleFrequency;


        public OneKGenSvReader(StreamReader reader, Dictionary<string, Chromosome> refNameDict)
		{
			_reader = reader;
		    _refNameDict  = refNameDict;
		}

		public IEnumerable<OnekGenSvItem> GetItems()
		{
		    string line;
		    while ((line = _reader.ReadLine()) != null)
		    {
                // Skip empty lines.
                if (string.IsNullOrWhiteSpace(line)) continue;

                // Skip comments.
                if (line.OptimizedStartsWith('#')) continue;
		        var oneKSvGenItem = ExtractOneKGenSvItem(line);
		        if (oneKSvGenItem == null) continue;
		        yield return oneKSvGenItem;

		    }
        }
	    private void Clear()
	    {
	        _allAlleleNumber = null;
	        _allAlleleFrequency = null;
	        _afrAlleleFrequency = null;
	        _amrAlleleFrequency = null;
	        _eurAlleleFrequency = null;
	        _easAlleleFrequency = null;
	        _sasAlleleFrequency = null;

	        _svType = null;
	    }

        private OnekGenSvItem ExtractOneKGenSvItem(string line)
		{
		    var splitLine = line.OptimizedSplit('\t');
            string altAllele = splitLine[AltIndex];
            if (altAllele.StartsWith("<INS:ME:")) return null;

            string chromosomeName = splitLine[ChromIndex];
		    if (!_refNameDict.ContainsKey(chromosomeName)) return null;
		    var chromosome = _refNameDict[chromosomeName];
		    int start = int.Parse(splitLine[StartIndex]) + 1; // start is 0-based in BED format
            int end = int.Parse(splitLine[EndIndex]);
		    string id = RemoveMissingValues(splitLine[IdIndex]);

            string infoFields = splitLine[InfoIndex];
            Clear();
		    ParseInfoField(infoFields);

		    var variantType = SaParseUtilities.GetSequenceAlteration(_svType);
            return new OnekGenSvItem(chromosome, start, end, variantType, id,  
				_allAlleleNumber, _allAlleleCount,
                _allAlleleFrequency, _afrAlleleFrequency, _amrAlleleFrequency, _easAlleleFrequency, _eurAlleleFrequency, _sasAlleleFrequency);
		}

        private static string RemoveMissingValues(string idField)
        {
            var ids = idField.OptimizedSplit(';');
            return string.Join(';', ids.Where(id => id != "."));
        }

        private void ParseInfoField(string infoFields)
	    {
	        if (infoFields == "" || infoFields == ".") return;
	        var infoItems = infoFields.OptimizedSplit(';');

	        foreach (string infoItem in infoItems)
	        {
	            (string key, string value) = infoItem.OptimizedKeyValue();

	            // sanity check
	            if (value != null) SetInfoField(key, value);
	        }
	    }
        //1       668630  esv3584976      G       <CN2>   100     PASS    AC=64;AF=0.0127796;AN=5008;CIEND=-150,150;CIPOS=-150,150;CS=DUP_delly;END=850204;NS=2504;SVTYPE=DUP;IMPRECISE;DP=22135;EAS_AF=0.0595;AMR_AF=0;AFR_AF=0.0015;EUR_AF=0.001;SAS_AF=0.001;VT=SV;EX_TARGET
        private void SetInfoField(string vcfAfId, string value)
	    {
	        switch (vcfAfId)
	        {
	            case "SVTYPE":
	                _svType = value;// for SVs there is only one value in SVTYPE
	                break;
                case "AN":
	                _allAlleleNumber = Convert.ToInt32(value);
	                break;
	            case "AC":
	                _allAlleleCount = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).Sum();
	                break;
                case "AF":
	                _allAlleleFrequency = value.OptimizedSplit(',').Select(Convert.ToDouble).Sum();
	                break;
	            case "AMR_AF":
	                _amrAlleleFrequency = value.OptimizedSplit(',').Select(Convert.ToDouble).Sum();
                    break;
	            case "AFR_AF":
	                _afrAlleleFrequency = value.OptimizedSplit(',').Select(Convert.ToDouble).Sum();
                    break;
	            case "EUR_AF":
	                _eurAlleleFrequency = value.OptimizedSplit(',').Select(Convert.ToDouble).Sum();
                    break;
	            case "EAS_AF":
	                _easAlleleFrequency = value.OptimizedSplit(',').Select(Convert.ToDouble).Sum();
                    break;
	            case "SAS_AF":
	                _sasAlleleFrequency = value.OptimizedSplit(',').Select(Convert.ToDouble).Sum();
                    break;
	        }
	    }

	    public void Dispose()
	    {
	        _reader?.Dispose();
	    }
	}
}