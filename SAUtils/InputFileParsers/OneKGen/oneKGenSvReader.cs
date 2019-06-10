using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;

namespace SAUtils.InputFileParsers.OneKGen
{
	public sealed class OneKGenSvReader:IDisposable
	{
		#region members

		private readonly StreamReader _reader;
	    private readonly IDictionary<string, IChromosome> _refNameDict;

	    private string _svType;
	    private int? _svEnd;
	    private int? _svLen;

	    private int? _allAlleleNumber;
	    private int? _allAlleleCount;
        private double? _allAlleleFrequency;
	    private double? _afrAlleleFrequency;
	    private double? _amrAlleleFrequency;
	    private double? _eurAlleleFrequency;
	    private double? _easAlleleFrequency;
	    private double? _sasAlleleFrequency;

        #endregion

        public OneKGenSvReader(StreamReader reader, IDictionary<string, IChromosome> refNameDict)
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

	        _svEnd = null;
	        _svLen = null;
	        _svType = null;
	    }

        private OnekGenSvItem ExtractOneKGenSvItem(string line)
		{
		    var splitLine = line.OptimizedSplit('\t');// we don't care about the many fields after info field
		    if (splitLine.Length < 8) return null;

		    var altAlleles = splitLine[VcfCommon.AltIndex].OptimizedSplit(',');
		    var hasSymbolicAllele = altAlleles.Any(x => x.OptimizedStartsWith('<') && x.OptimizedEndsWith('>'));

            if (!hasSymbolicAllele)
		        return null;

            var chromosomeName = splitLine[VcfCommon.ChromIndex];
		    if (!_refNameDict.ContainsKey(chromosomeName)) return null;
		    var chromosome = _refNameDict[chromosomeName];
		    var position = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
		    var id = splitLine[VcfCommon.IdIndex];
		    //var refAllele = splitLine[VcfCommon.RefIndex];
		    var infoFields = splitLine[VcfCommon.InfoIndex];
            Clear();
		    ParseInfoField(infoFields);

		    if (_svEnd == null  && _svLen!=null)
		    {
		        _svEnd = position + _svLen;
		    }

		    if (_svEnd == null)
		        return null;

		    var variantType = SaParseUtilities.GetSequenceAlteration(_svType);
            return new OnekGenSvItem(chromosome, position+1, _svEnd.Value, variantType, id,  
				_allAlleleNumber, _allAlleleCount,
                _allAlleleFrequency, _afrAlleleFrequency, _amrAlleleFrequency, _easAlleleFrequency, _eurAlleleFrequency, _sasAlleleFrequency);
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
                case "SVLEN":
                    _svLen = Convert.ToInt32(value);
                    break;
                case "END":
	                _svEnd = Convert.ToInt32(value);
	                break;
                case "AN":
	                _allAlleleNumber = Convert.ToInt32(value);
	                break;
	            case "AC":
	                _allAlleleCount = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).Sum();
	                break;
                case "AF":
	                _allAlleleFrequency = value.OptimizedSplit(',').Select(Convert.ToDouble).ToArray().Sum();
	                break;
	            case "AMR_AF":
	                _amrAlleleFrequency = value.OptimizedSplit(',').Select(Convert.ToDouble).ToArray().Sum();
                    break;
	            case "AFR_AF":
	                _afrAlleleFrequency = value.OptimizedSplit(',').Select(Convert.ToDouble).ToArray().Sum();
                    break;
	            case "EUR_AF":
	                _eurAlleleFrequency = value.OptimizedSplit(',').Select(Convert.ToDouble).ToArray().Sum();
                    break;
	            case "EAS_AF":
	                _easAlleleFrequency = value.OptimizedSplit(',').Select(Convert.ToDouble).ToArray().Sum();
                    break;
	            case "SAS_AF":
	                _sasAlleleFrequency = value.OptimizedSplit(',').Select(Convert.ToDouble).ToArray().Sum();
                    break;
	        }
	    }

	    public void Dispose()
	    {
	        _reader?.Dispose();
	    }
	}
}