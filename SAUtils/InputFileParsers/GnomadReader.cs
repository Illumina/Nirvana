using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.InputFileParsers
{
	public sealed class GnomadReader 
	{
	    private readonly StreamReader _reader;
        private readonly IDictionary<string,IChromosome> _refChromDict;

        private int[] _acAll;
		private int[] _acAfr;
		private int[] _acAmr;
		private int[] _acEas;
		private int[] _acFin;
		private int[] _acNfe;
		private int[] _acOth;
		private int[] _acAsj;
	    private int[] _acSas;

	    private int _anAll;
        private int _anAfr;
		private int _anAmr;
		private int _anEas;
		private int _anFin;
		private int _anNfe;
		private int _anOth;
		private int _anAsj;
	    private int _anSas;

	    private int[] _hcAll;
	    private int[] _hcAfr;
	    private int[] _hcAmr;
	    private int[] _hcEas;
	    private int[] _hcFin;
	    private int[] _hcNfe;
	    private int[] _hcOth;
	    private int[] _hcAsj;
	    private int[] _hcSas;

        private int? _totalDepth;
	    private bool _hasFailedFilters;


		public GnomadReader(StreamReader streamReader, IDictionary<string, IChromosome> refChromDict) 
		{
			_reader = streamReader;
		    _refChromDict = refChromDict;
		}

        

		private void Clear()
		{
			_acAll = null;
			_acAfr = null;
			_acAmr = null;
			_acEas = null;
			_acFin = null;
			_acNfe = null;
			_acOth = null;
			_acAsj = null;
		    _acSas = null;

		    _anAll = 0;
			_anAfr = 0;
			_anAmr = 0;
			_anEas = 0;
			_anFin = 0;
			_anNfe = 0;
			_anOth = 0;
			_anAsj = 0;
		    _anSas = 0;

		    _hcAll = null;
		    _hcAfr = null;
		    _hcAmr = null;
		    _hcEas = null;
		    _hcFin = null;
		    _hcNfe = null;
		    _hcOth = null;
		    _hcAsj = null;
		    _hcSas = null;

            _totalDepth = null;
		    _hasFailedFilters = false;
		}

		/// <summary>
		/// Parses a source file and return an enumeration object containing 
		/// all the data objects that have been extracted.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<GnomadItem> GetGnomadItems()
		{
			using (_reader)
			{
				string line;
				while ((line = _reader.ReadLine()) != null)
				{
					// Skip empty lines.
					if (string.IsNullOrWhiteSpace(line)) continue;
					// Skip comments.
					if (line.StartsWith("#")) continue;
					var gnomadItemsList = ExtractItems(line);
					if (gnomadItemsList == null) continue;
					foreach (var gnomadItem in gnomadItemsList)
					{
						yield return gnomadItem;
					}

				}
			}
		}

	    /// <summary>
		/// Extracts a gnomad item(s) from the specified VCF line.
		/// </summary>
		/// <param name="vcfline"></param>
		/// <returns></returns>
	    private List<GnomadItem> ExtractItems(string vcfline)
		{
			if (vcfline == null) return null;
			var splitLine = vcfline.Split( '\t');// we don't care about the many fields after info field
			
			if (splitLine.Length < 8) return null;

			Clear();

			var chromosome = splitLine[VcfCommon.ChromIndex];
			if (!_refChromDict.ContainsKey(chromosome)) return null;

		    var chrom      = _refChromDict[chromosome];
			var position   = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
			var refAllele  = splitLine[VcfCommon.RefIndex];
			var altAlleles = splitLine[VcfCommon.AltIndex].Split(',');
		    var filters    = splitLine[VcfCommon.FilterIndex];
			var infoFields = splitLine[VcfCommon.InfoIndex];

		    _hasFailedFilters = !(filters.Equals("PASS") || filters.Equals("."));
            // parses the info fields and extract frequencies, coverage, num samples.
		    try
		    {
		        ParseInfoField(infoFields);
            }
		    catch (Exception e)
		    {
		        Console.WriteLine(vcfline);
		        Console.WriteLine(e);
		        throw;
		    }
            

		    if (_anAll == 0) return null;

            var gnomadItemsList = new List<GnomadItem>();

			
			for (int i = 0; i < altAlleles.Length; i++)
			{
				gnomadItemsList.Add(new GnomadItem(
					chrom,
					position,
					refAllele,
					altAlleles[i],
                    _totalDepth,
					_anAll, _anAfr,_anAmr,_anEas,_anFin,_anNfe,_anOth, _anAsj, _anSas,
					GetCount(_acAll, i), GetCount(_acAfr, i), GetCount(_acAmr, i), GetCount(_acEas, i), 
					GetCount(_acFin, i), GetCount(_acNfe, i), GetCount(_acOth, i), GetCount(_acAsj, i),
			        GetCount(_acSas, i),
					GetCount(_hcAll, i), GetCount(_hcAfr, i), GetCount(_hcAmr, i), GetCount(_hcEas, i), GetCount(_hcFin, i),
					GetCount(_hcNfe, i), GetCount(_hcOth, i), GetCount(_hcAsj, i), GetCount(_hcSas, i),
                    _hasFailedFilters)
					);
			}
			return gnomadItemsList;
		}

		private static int? GetCount(int[] counts, int i)
		{
			if (counts == null) return null;
			if (i >= counts.Length) return null;
			return counts[i];
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
				case "AC":
					_acAll = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
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

				case "AC_ASJ":
					_acAsj = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

			    case "AC_SAS":
			        _acSas = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
			        break;

                case "AN":
			        _anAll = Convert.ToInt32(value);
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

				case "AN_ASJ":
					_anAsj = Convert.ToInt32(value);
					break;

			    case "AN_SAS":
			        _anSas = Convert.ToInt32(value);
			        break;

			    case "Hom":
			        _hcAll = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "Hom_AFR":
			        _hcAfr = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "Hom_AMR":
			        _hcAmr = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "Hom_EAS":
			        _hcEas = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "Hom_FIN":
			        _hcFin = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "Hom_NFE":
			        _hcNfe = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "Hom_OTH":
			        _hcOth = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "Hom_ASJ":
			        _hcAsj = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "Hom_SAS":
			        _hcSas = value.Split(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

                case "DP":
					_totalDepth = Convert.ToInt32(value);
					break;

			}

		}

	}
}
