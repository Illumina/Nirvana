using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Utilities;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.InputFileParsers.OneKGen
{
    public sealed class OneKGenReader 
    {
        private readonly FileInfo _oneKGenFile;
        private readonly IDictionary<string,IChromosome> _refNameDictionary;

        private  string _ancestralAllele;
	    private  string _svType;
	    private  int _svEnd;
        private  string _refAllele;
		private  string[] _altAlleles;

	    private int? _allAlleleNumber;
	    private int? _afrAlleleNumber;
	    private int? _amrAlleleNumber;
	    private int? _eurAlleleNumber;
	    private int? _easAlleleNumber;
	    private int? _sasAlleleNumber;

		private int[] _allAlleleCounts;
		private int[] _afrAlleleCounts;
		private int[] _amrAlleleCounts;
		private int[] _eurAlleleCounts;
		private int[] _easAlleleCounts;
		private int[] _sasAlleleCounts;




		// grch38 liftover related fields
#pragma warning disable 414
		private  string _minorAllele;
	    private  double _minorAlleleFreq;
	    private  int _minorAlleleCount;
#pragma warning disable 169
	    private bool _hasSymbolicAllele;
#pragma warning restore 169
#pragma warning restore 414

        // empty constructor for onekg reader for unit tests.
        internal OneKGenReader(IDictionary<string,IChromosome> refNameDict)
        {
            _refNameDictionary = refNameDict;
        }

	    public OneKGenReader(FileInfo oneKGenFile, IDictionary<string, IChromosome> refNameDict) : this(refNameDict)
        {
            _oneKGenFile = oneKGenFile;
        }

        private void Clear()
	    {
		    _ancestralAllele = null;
		    _refAllele = null;
		    _altAlleles = null;

			_allAlleleNumber = null;
			_afrAlleleNumber = null;
			_amrAlleleNumber = null;
			_eurAlleleNumber = null;
			_easAlleleNumber = null;
			_sasAlleleNumber = null;

			_allAlleleCounts = null;
			_afrAlleleCounts = null;
			_amrAlleleCounts = null;
			_eurAlleleCounts = null;
			_easAlleleCounts = null;
			_sasAlleleCounts = null;

			// SV fields
			_svEnd  = -1;
			_svType = null;

	        // grch38 fields
			_minorAlleleCount = 0;
			_minorAlleleFreq  = 0;
			_minorAllele      = null;

	    }

	    public IEnumerable<OneKGenItem> GetOneKGenItems()
        {
            using (var reader = GZipUtilities.GetAppropriateStreamReader(_oneKGenFile.FullName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    // Skip comments.
                    if (line.StartsWith("#")) continue;
                    var oneKGenItemsList = ExtractItems(line);
	                if (oneKGenItemsList == null) continue;
	                foreach (var oneKGenItem in oneKGenItemsList)
	                {
						yield return oneKGenItem;
	                }
					
                }
            }
        }

        internal List<OneKGenItem> ExtractItems(string vcfline)
        {
            var splitLine = vcfline.Split(new[]{'\t'}, 9);// we don't care about the many fields after info field
            if (splitLine.Length < 8) return null;

			Clear();
			
            var chromosomeName  = splitLine[VcfCommon.ChromIndex];
            if (!_refNameDictionary.ContainsKey(chromosomeName)) return null;
            var chromosome = _refNameDictionary[chromosomeName];
			var position    = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
            var rsId        = splitLine[VcfCommon.IdIndex];
            _refAllele      = splitLine[VcfCommon.RefIndex];
			_altAlleles     = splitLine[VcfCommon.AltIndex].Split(',');
			var infoFields  = splitLine[VcfCommon.InfoIndex];

			// parses the info fields and extract frequencies, ancestral allele, allele counts, etc.
			var hasSymbolicAllele = _altAlleles.Any(x => x.StartsWith("<") && x.EndsWith(">"));
	        if (hasSymbolicAllele) return null;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
			ParseInfoField(infoFields, hasSymbolicAllele);

			var okgItemsList = new List<OneKGenItem>();
	        
			for (var i = 0; i < _altAlleles.Length; i++)
			{
				okgItemsList.Add(new OneKGenItem(
					chromosome,
					position,
					rsId,
					_refAllele,
					_altAlleles[i],
					_ancestralAllele,
					GetAlleleCount(_allAlleleCounts, i),
					GetAlleleCount(_afrAlleleCounts,i),
					GetAlleleCount(_amrAlleleCounts,i),
					GetAlleleCount(_eurAlleleCounts,i),
					GetAlleleCount(_easAlleleCounts,i),
					GetAlleleCount(_sasAlleleCounts, i),
					_allAlleleNumber,
					_afrAlleleNumber,
					_amrAlleleNumber,
					_eurAlleleNumber,
					_easAlleleNumber,
					_sasAlleleNumber,
					_svType,
					_svEnd
					));
			}
			
			return okgItemsList;
           
        }

	    private static int? GetAlleleCount(int[] alleleCounts, int i)
	    {
		    if (alleleCounts == null) return null;
		    if (i >= alleleCounts.Length) return null;
		    return alleleCounts[i];
	    }

	    private  void ParseInfoField(string infoFields, bool hasSymbolicAllele)
		{
			if (infoFields == "" || infoFields == ".") return;

			var infoItems = infoFields.Split(';');
			foreach (var infoItem in infoItems)
			{
				var infoKeyValue = infoItem.Split('=');
				if (infoKeyValue.Length == 2)//sanity check
				{
					var key = infoKeyValue[0];
					var value = infoKeyValue[1];

					SetInfoField(key, value, hasSymbolicAllele);
				}

			}

		}

		private  void SetInfoField(string vcfAfId, string value, bool hasSymbolicAllele)
		{
			switch (vcfAfId)
			{
				case "AA":
					_ancestralAllele = GetAncestralAllele(value);
					break;
				// the following are for SVs
				case "SVTYPE":
					if (hasSymbolicAllele)
						_svType = value;// for SVs there is only one value in SVTYPE
					break;
				case "END":
					if (hasSymbolicAllele)
						_svEnd = Convert.ToInt32(value);
					break;
				case "CIEND":
					/*if (hasSymbolicAllele)
					{
						var endBoundaries = value.Split(',');
						Tuple.Create(Convert.ToInt32(endBoundaries[0]), Convert.ToInt32(endBoundaries[1]));
					}
					break;*/
				case "CIPOS":
					/*if (hasSymbolicAllele)
					{
						var beginBoundaries = value.Split(',');
						Tuple.Create(Convert.ToInt32(beginBoundaries[0]), Convert.ToInt32(beginBoundaries[1]));
					}*/
					break;
				case "AN":
					_allAlleleNumber = Convert.ToInt32(value);
					break;
				case "AFR_AN":
					_afrAlleleNumber = Convert.ToInt32(value);
					break;
				case "AMR_AN":
					_amrAlleleNumber = Convert.ToInt32(value);
					break;
				case "EUR_AN":
					_eurAlleleNumber = Convert.ToInt32(value);
					break;
				case "EAS_AN":
					_easAlleleNumber = Convert.ToInt32(value);
					break;
				case "SAS_AN":
					_sasAlleleNumber = Convert.ToInt32(value);
					break;
				case "AC":
					_allAlleleCounts = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;
				case "AMR_AC":
					_amrAlleleCounts = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;
				case "AFR_AC":
					_afrAlleleCounts = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;
				case "EUR_AC":
					_eurAlleleCounts = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;
				case "EAS_AC":
					_easAlleleCounts = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;
				case "SAS_AC":
					_sasAlleleCounts = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

			}

		}

		private static string GetAncestralAllele(string value)
		{
			if (value == "" || value == ".") return null;

			var ancestralAllele = value.Split('|')[0];
			if (string.IsNullOrEmpty(ancestralAllele)) return null;
			return ancestralAllele.All(IsNucleotide) ? ancestralAllele : null;
		}
		private static bool IsNucleotide(char c)
		{
			c = char.ToUpper(c);
			return c == 'A' || c == 'C' || c == 'G' || c == 'T' || c == 'N';
		}
		
    }
}
