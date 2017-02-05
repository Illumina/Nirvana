using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;

namespace SAUtils.InputFileParsers.EVS
{
    public sealed class EvsReader : IEnumerable<EvsItem>
    {
        private readonly FileInfo _evsFileInfo;
		private double[] _allFrequencies;
	    private double[] _europeanFrequencies;
		private double[] _africanFrequencies;
	    private string _coverage;
	    private string _numSamples;
        private readonly ChromosomeRenamer _renamer;

        public EvsReader(FileInfo evsInfo, ChromosomeRenamer renamer) : this(renamer)
        {
            _evsFileInfo = evsInfo;
        }

        // for unit tests to check methods
        public EvsReader(ChromosomeRenamer renamer)
		{
		    _renamer = renamer;
		}

		public IEnumerator<EvsItem> GetEnumerator()
		{
			return GetEvsItems().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
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

	    /// <summary>
        /// Parses a OneKGen file and return an enumeration object containing 
        /// all the OneKGen objects that have been extracted.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<EvsItem> GetEvsItems()
        {
            using (var reader = GZipUtilities.GetAppropriateStreamReader(_evsFileInfo.FullName))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    // Skip comments.
                    if (line.StartsWith("#")) continue;
                    var evsItemsList = ExtractItems(line);
	                if (evsItemsList == null) continue;
	                foreach (var evsItem in evsItemsList)
	                {
						yield return evsItem;   
	                }
					
                }
            }
        }

        /// <summary>
        /// Extracts a OneKGen item from the specified VCF line.
        /// </summary>
        /// <param name="vcfline"></param>
        /// <returns></returns>
        public List<EvsItem> ExtractItems(string vcfline)
        {
            var splitLine = vcfline.Split(new[]{'\t'}, 9);// we don't care about the many fields after info field
            if (splitLine.Length < 8) return null;

	        Clear();

            var chromosome  = splitLine[VcfCommon.ChromIndex];
			if (!InputFileParserUtilities.IsDesiredChromosome(chromosome, _renamer)) return null;
			var position    = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
            var rsId        = splitLine[VcfCommon.IdIndex];
            var refAllele   = splitLine[VcfCommon.RefIndex];
            var altAlleles  = splitLine[VcfCommon.AltIndex].Split(',');
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

			var infoItems = infoFields.Split(';');
			foreach (var infoItem in infoItems)
			{
				var infoKeyValue = infoItem.Split('=');
				if (infoKeyValue.Length == 2)//sanity check
				{
					var key = infoKeyValue[0];
					var value = infoKeyValue[1];

					SetInfoField(key, value);
				}

			}

		}

		private void SetInfoField(string vcfId, string value)
		{

			switch (vcfId)
			{
				case "EA_AC":
					var europeanAlleleCounts = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					var totalEurAlleleCount = europeanAlleleCounts.Sum();

					_europeanFrequencies = europeanAlleleCounts.Select(val => 1.0*val/totalEurAlleleCount).ToArray();
					break;
				case "AA_AC":
					var africanAlleleCounts = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					var totalAfrAlleleCount = africanAlleleCounts.Sum();

					_africanFrequencies = africanAlleleCounts.Select(val => 1.0*val/totalAfrAlleleCount).ToArray();
					break;
				case "TAC":
					var alleleCounts = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					var totalAlleleCount = alleleCounts.Sum();

					_allFrequencies = alleleCounts.Select(val => 1.0*val/totalAlleleCount).ToArray();
					break;
				case "DP":
					_coverage = value;
					break;
				case "GTC":
					int count = value.Split(',').Sum(Convert.ToInt32);
					_numSamples = count.ToString(CultureInfo.InvariantCulture);
					break;
			}

		}

    }
}
