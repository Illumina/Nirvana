using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;

namespace SAUtils.InputFileParsers.EVS
{
    public sealed class EvsReader
    {
        private readonly StreamReader _evsFileReader;
		private double[] _allFrequencies;
	    private double[] _europeanFrequencies;
		private double[] _africanFrequencies;
	    private string _coverage;
	    private string _numSamples;
        private readonly IDictionary<string, IChromosome> _refChromDict;

        public EvsReader(StreamReader evsStream, IDictionary<string, IChromosome> refChromDict)
        {
            _evsFileReader = evsStream;
            _refChromDict = refChromDict;
        }


		private void Clear()
	    {
		    _europeanFrequencies = null;
		    _africanFrequencies  = null;
		    _allFrequencies      = null;
		    _europeanFrequencies = null;
		    _africanFrequencies  = null;
		    _coverage            = null;
		    _numSamples          = null;
	    }

	    public IEnumerable<EvsItem> GetEvsItems()
        {
            using (_evsFileReader)
            {
                string line;
                while ((line = _evsFileReader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (line.IsWhiteSpace()) continue;
                    // Skip comments.
                    if (line.OptimizedStartsWith('#')) continue;
                    var evsItemsList = ExtractItems(line);
	                if (evsItemsList == null) continue;
	                foreach (var evsItem in evsItemsList)
	                {
						yield return evsItem;   
	                }
					
                }
            }
        }

        internal List<EvsItem> ExtractItems(string vcfline)
        {
            var splitLine = vcfline.Split(new[]{'\t'}, 9);// we don't care about the many fields after info field
            if (splitLine.Length < 8) return null;

	        Clear();

            var chromosomeName  = splitLine[VcfCommon.ChromIndex];
			if (!_refChromDict.ContainsKey(chromosomeName)) return null;
            var chromosome = _refChromDict[chromosomeName];
			var position    = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
            var rsId        = splitLine[VcfCommon.IdIndex];
            var refAllele   = splitLine[VcfCommon.RefIndex];
            var altAlleles  = splitLine[VcfCommon.AltIndex].OptimizedSplit(',');
			var infoFields  = splitLine[VcfCommon.InfoIndex];

			//return null if the position is -1. This happens for entries in GRCh38
	        if (position < 0) return null;
			// parses the info fields and extract frequencies, coverage, num samples.
			ParseInfoField(infoFields);
	        var evsItemsList = new List<EvsItem>();

	        for (int i = 0; i < altAlleles.Length; i++)
	        {
				evsItemsList.Add(new EvsItem(
					chromosome, 
					position, 
					rsId, 
					refAllele, 
					altAlleles[i],
					$"{_allFrequencies[i]:0.000000}",
					$"{_africanFrequencies[i]:0.000000}",
					$"{_europeanFrequencies[i]:0.000000}", 
					_coverage, 
					_numSamples)
					
					);
	        }
	        return evsItemsList;
        }

        private void ParseInfoField(string infoFields)
        {
            if (infoFields == "" || infoFields == ".") return;
            var infoItems = infoFields.OptimizedSplit(';');

            foreach (string infoItem in infoItems)
            {
                (string key, string value) = infoItem.OptimizedKeyValue();

                //sanity check
                if (value != null) SetInfoField(key, value);
            }
        }

        private void SetInfoField(string vcfId, string value)
		{

			switch (vcfId)
			{
				case "EA_AC":
					var europeanAlleleCounts = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					var totalEurAlleleCount = europeanAlleleCounts.Sum();

					_europeanFrequencies = europeanAlleleCounts.Select(val => 1.0*val/totalEurAlleleCount).ToArray();
					break;
				case "AA_AC":
					var africanAlleleCounts = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					var totalAfrAlleleCount = africanAlleleCounts.Sum();

					_africanFrequencies = africanAlleleCounts.Select(val => 1.0*val/totalAfrAlleleCount).ToArray();
					break;
				case "TAC":
					var alleleCounts = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					var totalAlleleCount = alleleCounts.Sum();

					_allFrequencies = alleleCounts.Select(val => 1.0*val/totalAlleleCount).ToArray();
					break;
				case "DP":
					_coverage = value;
					break;
				case "GTC":
					int count = value.OptimizedSplit(',').Sum(Convert.ToInt32);
					_numSamples = count.ToString(CultureInfo.InvariantCulture);
					break;
			}

		}

    }
}
