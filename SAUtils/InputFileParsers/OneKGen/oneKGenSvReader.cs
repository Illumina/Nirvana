using System.Collections.Generic;
using System.IO;
using Compression.Utilities;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.InputFileParsers.OneKGen
{
	public sealed class OneKGenSvReader
	{
		#region members

		private readonly FileInfo _oneKGenSvFile;
	    private readonly IDictionary<string, IChromosome> _refNameDict;
        #endregion

        public OneKGenSvReader(FileInfo oneKGenSvFile, IDictionary<string, IChromosome> refNameDict)
		{
			_oneKGenSvFile = oneKGenSvFile;
		    _refNameDict = refNameDict;
		}

		public IEnumerable<OneKGenItem> GetOneKGenSvItems()
		{
			using (var reader = GZipUtilities.GetAppropriateStreamReader(_oneKGenSvFile.FullName))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					// Skip empty lines.
					if (string.IsNullOrWhiteSpace(line)) continue;
					// Skip comments.
					if (line.StartsWith("#")) continue;
					var oneKSvGenItem = ExtractOneKGenSvItem(line, _refNameDict);
					if (oneKSvGenItem == null ) continue;
					yield return oneKSvGenItem;

				}
			}
		}

		private static OneKGenItem ExtractOneKGenSvItem(string line, IDictionary<string,IChromosome> refNameDict)
		{
			var cols = line.Split('\t');
			if (cols.Length < 8) return null;

			var id = cols[0];
		    if (!refNameDict.ContainsKey(cols[1])) return null;
		    var chromosome = refNameDict[cols[1]];


			var start = int.Parse(cols[2]);
			var end = int.Parse(cols[3]);
			var variantType = cols[4];

			var observedGains =  int.Parse(cols[6]);
			var observedLosses = int.Parse(cols[7]);

			var allFrequency = cols[8].Equals("0")? null:cols[8];
			var easFrequency = cols[62].Equals("0") ? null : cols[62];
			var eurFrequency = cols[64].Equals("0") ? null : cols[64];
			var afrFrequency = cols[66].Equals("0") ? null : cols[66];
			var amrFrequency = cols[68].Equals("0") ? null : cols[68];
			var sasFrequency = cols[70].Equals("0") ? null : cols[70];

			var allAlleleNumber = int.Parse(cols[5]);
			var easAlleleNumber = int.Parse(cols[61]);
			var eurAlleleNumber = int.Parse(cols[63]);
			var afrAlleleNumber = int.Parse(cols[65]);
			var amrAlleleNumber = int.Parse(cols[67]);
			var sasAlleleNumber = int.Parse(cols[69]);

			return new OneKGenItem(chromosome, start, id, null, null, null, 
				afrFrequency, allFrequency, amrFrequency,easFrequency, eurFrequency, sasFrequency,
				null,null,null,null,null,null,
				allAlleleNumber, afrAlleleNumber, amrAlleleNumber, eurAlleleNumber, easAlleleNumber, sasAlleleNumber,
				variantType, end, observedGains, observedLosses);
		}
        
	}
}