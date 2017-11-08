using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Compression.Utilities;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.InputFileParsers.ExAc
{
	public sealed class ExacReader 
	{
	    private readonly FileInfo _exacFileInfo;
        private readonly IDictionary<string,IChromosome> _refChromDict;

        private int[] _acAdj;
		private int[] _acAfr;
		private int[] _acAmr;
		private int[] _acEas;
		private int[] _acFin;
		private int[] _acNfe;
		private int[] _acOth;
		private int[] _acSas;

		private int _anAdj;
		private int _anAfr;
		private int _anAmr;
		private int _anEas;
		private int _anFin;
		private int _anNfe;
		private int _anOth;
		private int _anSas;

		private int _totalDepth;


		public ExacReader(FileInfo exacInfo, IDictionary<string, IChromosome> refChromDict) 
		{
			_exacFileInfo = exacInfo;
		    _refChromDict = refChromDict;
		}

        

		private void Clear()
		{
			_acAdj = null;
			_acAfr = null;
			_acAmr = null;
			_acEas = null;
			_acFin = null;
			_acNfe = null;
			_acOth = null;
			_acSas = null;

			_anAdj = 0;
			_anAfr = 0;
			_anAmr = 0;
			_anEas = 0;
			_anFin = 0;
			_anNfe = 0;
			_anOth = 0;
			_anSas = 0;

			_totalDepth = 0;
		}

		public IEnumerable<ExacItem> GetExacItems()
		{
			using (var reader = GZipUtilities.GetAppropriateStreamReader(_exacFileInfo.FullName))
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					// Skip empty lines.
					if (string.IsNullOrWhiteSpace(line)) continue;
					// Skip comments.
					if (line.StartsWith("#")) continue;
					var exacItemsList = ExtractItems(line);
					if (exacItemsList == null) continue;
					foreach (var exacItem in exacItemsList)
					{
						yield return exacItem;
					}

				}
			}
		}

	    /// <summary>
		/// Extracts a exac item(s) from the specified VCF line.
		/// </summary>
		/// <param name="vcfline"></param>
		/// <returns></returns>
		public List<ExacItem> ExtractItems(string vcfline)
		{
			if (vcfline == null) return null;
			var splitLine = vcfline.Split( '\t');// we don't care about the many fields after info field
			
			if (splitLine.Length < 8) return null;

			Clear();

			var chromosome = splitLine[VcfCommon.ChromIndex];
			if (!_refChromDict.ContainsKey(chromosome)) return null;

		    var chrom = _refChromDict[chromosome];
			var position   = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
			var refAllele  = splitLine[VcfCommon.RefIndex];
			var altAlleles = splitLine[VcfCommon.AltIndex].Split(',');
			var infoFields = splitLine[VcfCommon.InfoIndex];

			// parses the info fields and extract frequencies, coverage, num samples.
			ParseInfoField(infoFields);

			if (_anAdj == 0) return null;

			var exacItemsList = new List<ExacItem>();

			
			for (int i = 0; i < altAlleles.Length; i++)
			{
				exacItemsList.Add(new ExacItem(
					chrom,
					position,
					refAllele,
					altAlleles[i],
					ComputingUtilities.GetCoverage(_totalDepth, _anAdj / 2.0),
                    //(int) Math.Round(_totalDepth*1.0/ (_anAdj/2.0), MidpointRounding.AwayFromZero),//to be consistant with evs, we use integer values for coverage
					_anAdj, _anAfr,_anAmr,_anEas,_anFin,_anNfe,_anOth,_anSas,
					GetAlleleCount(_acAdj, i), GetAlleleCount(_acAfr, i), GetAlleleCount(_acAmr, i), GetAlleleCount(_acEas, i), 
					GetAlleleCount(_acFin, i), GetAlleleCount(_acNfe, i), GetAlleleCount(_acOth, i), GetAlleleCount(_acSas, i))
					);
			}
			return exacItemsList;
		}

		private static int? GetAlleleCount(int[] alleleCounts, int i)
		{
			if (alleleCounts == null) return null;
			if (i >= alleleCounts.Length) return null;
			return alleleCounts[i];
		}

		/// <summary>
		/// split up the info field and extract information from each of them.
		/// </summary>
		/// <param name="infoFields"></param>
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

		/// <summary>
		/// Get a key value pair and using the key, set appropriate values
		/// </summary>
		/// <param name="vcfId"></param>
		/// <param name="value"></param>
		private void SetInfoField(string vcfId, string value)
		{
			switch (vcfId)
			{
				case "AC_Adj":
					_acAdj = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_AFR":
					_acAfr = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_AMR":
					_acAmr = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_EAS":
					_acEas = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_FIN":
					_acFin = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_NFE":
					_acNfe = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_OTH":
					_acOth = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_SAS":
					_acSas = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AN_Adj":
					_anAdj = Convert.ToInt32(value);
					break;

				case "AN_AFR":
					_anAfr = Convert.ToInt32(value);
					break;

				case "AN_AMR":
					_anAmr = Convert.ToInt32(value);
					break;

				case "AN_EAS":
					_anEas = Convert.ToInt32(value);
					break;

				case "AN_FIN":
					_anFin = Convert.ToInt32(value);
					break;

				case "AN_NFE":
					_anNfe = Convert.ToInt32(value);
					break;

				case "AN_OTH":
					_anOth = Convert.ToInt32(value);
					break;

				case "AN_SAS":
					_anSas = Convert.ToInt32(value);
					break;

				case "DP":
					_totalDepth = Convert.ToInt32(value);
					break;

			}

		}

	}
}
