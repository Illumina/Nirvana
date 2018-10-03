using System.Collections.Generic;
using Compression.Utilities;
using Genome;
using OptimizedCore;
using SAUtils.DataStructures;

namespace SAUtils.InputFileParsers.OneKGen
{
	public sealed class OneKGenSvReader
	{
		#region members

		private readonly string _inputVcfName;
	    private readonly IDictionary<string, IChromosome> _refNameDict;
        #endregion

        public OneKGenSvReader(string inputVcfName, IDictionary<string, IChromosome> refNameDict)
		{
			_inputVcfName = inputVcfName;
		    _refNameDict  = refNameDict;
		}

		public IEnumerable<OnekGenSvItem> GetItems()
		{
			using (var reader = GZipUtilities.GetAppropriateStreamReader(_inputVcfName))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					// Skip empty lines.
					if (line.IsWhiteSpace()) continue;
					// Skip comments.
					if (line.OptimizedStartsWith('#')) continue;
					var oneKSvGenItem = ExtractOneKGenSvItem(line, _refNameDict);
					if (oneKSvGenItem == null ) continue;
					yield return oneKSvGenItem;

				}
			}
		}

		private static OnekGenSvItem ExtractOneKGenSvItem(string line, IDictionary<string,IChromosome> refNameDict)
		{
			var cols = line.OptimizedSplit('\t');
			if (cols.Length < 8) return null;

			string id = cols[0];
		    if (!refNameDict.ContainsKey(cols[1])) return null;
		    var chromosome = refNameDict[cols[1]];


			int start = int.Parse(cols[2]);
			int end = int.Parse(cols[3]);
			string variantType = cols[4];

			int observedGains =  int.Parse(cols[6]);
			int observedLosses = int.Parse(cols[7]);

			string allFrequency = cols[8].Equals("0")? null:cols[8];
			string easFrequency = cols[62].Equals("0") ? null : cols[62];
			string eurFrequency = cols[64].Equals("0") ? null : cols[64];
			string afrFrequency = cols[66].Equals("0") ? null : cols[66];
			string amrFrequency = cols[68].Equals("0") ? null : cols[68];
			string sasFrequency = cols[70].Equals("0") ? null : cols[70];

			int allAlleleNumber = int.Parse(cols[5]);
			int easAlleleNumber = int.Parse(cols[61]);
			int eurAlleleNumber = int.Parse(cols[63]);
			int afrAlleleNumber = int.Parse(cols[65]);
			int amrAlleleNumber = int.Parse(cols[67]);
			int sasAlleleNumber = int.Parse(cols[69]);

            var svType = SaParseUtilities.GetSequenceAlteration(variantType, observedGains, observedLosses);
            return new OnekGenSvItem(chromosome, start, end, svType, id,  
				afrFrequency, allFrequency, amrFrequency,easFrequency, eurFrequency, sasFrequency,
				allAlleleNumber, afrAlleleNumber, amrAlleleNumber, eurAlleleNumber, easAlleleNumber, sasAlleleNumber,
				observedGains, observedLosses);
		}
        
	}
}