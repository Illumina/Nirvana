using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace SAUtils.InputFileParsers
{
	public sealed class GnomadReader 
	{
        private readonly StreamReader _reader;
        private readonly ISequenceProvider _sequenceProvider;

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

		public GnomadReader(StreamReader streamReader, ISequenceProvider sequenceProvider) 
		{
			_reader       = streamReader;
		    _sequenceProvider = sequenceProvider;
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
		}

		/// <summary>
		/// Parses a source file and return an enumeration object containing 
		/// all the data objects that have been extracted.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<GnomadItem> GetItems()
		{
			using (_reader)
			{
				string line;
				while ((line = _reader.ReadLine()) != null)
				{
					// Skip empty lines.
					if (string.IsNullOrWhiteSpace(line)) continue;

					// Skip comments.
					if (line.OptimizedStartsWith('#')) continue;
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
            var splitLine = vcfline.OptimizedSplit('\t');

            if (splitLine.Length < 8) return null;

			Clear();

			var chromosome = splitLine[VcfCommon.ChromIndex];
			if (!_sequenceProvider.RefNameToChromosome.ContainsKey(chromosome)) return null;

		    var chrom      = _sequenceProvider.RefNameToChromosome[chromosome];
			var position   = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
			var refAllele  = splitLine[VcfCommon.RefIndex];
			var altAlleles = splitLine[VcfCommon.AltIndex].OptimizedSplit(',');
		    var filters    = splitLine[VcfCommon.FilterIndex];
			var infoFields = splitLine[VcfCommon.InfoIndex];

		    var hasFailedFilters = !(filters.Equals("PASS") || filters.Equals("."));

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
                var (alignedPos, alignedRef, alignedAlt) =
                    VariantUtils.TrimAndLeftAlign(position, refAllele, altAlleles[i], _sequenceProvider.Sequence);

				gnomadItemsList.Add(new GnomadItem(
					chrom,
					alignedPos,
					alignedRef,
					alignedAlt,
                    _totalDepth,
					_anAll, _anAfr,_anAmr,_anEas,_anFin,_anNfe,_anOth, _anAsj, _anSas,
					GetCount(_acAll, i), GetCount(_acAfr, i), GetCount(_acAmr, i), GetCount(_acEas, i), 
					GetCount(_acFin, i), GetCount(_acNfe, i), GetCount(_acOth, i), GetCount(_acAsj, i),
			        GetCount(_acSas, i),
					GetCount(_hcAll, i), GetCount(_hcAfr, i), GetCount(_hcAmr, i), GetCount(_hcEas, i), GetCount(_hcFin, i),
					GetCount(_hcNfe, i), GetCount(_hcOth, i), GetCount(_hcAsj, i), GetCount(_hcSas, i),
				    hasFailedFilters)
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
		    var infoItems = infoFields.OptimizedSplit(';');

		    foreach (string infoItem in infoItems)
		    {
		        (string key, string value) = infoItem.OptimizedKeyValue();

		        // sanity check
		        if (value != null) SetInfoField(key, value);
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
					_acAll = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_afr":
					_acAfr = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_amr":
					_acAmr = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_eas":
					_acEas = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_fin":
					_acFin = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_nfe":
					_acNfe = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_oth":
					_acOth = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

				case "AC_asj":
					_acAsj = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;

			    case "AC_sas":
			        _acSas = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
			        break;

                case "AN":
			        _anAll = Convert.ToInt32(value);
			        break;

				case "AN_afr":
					_anAfr = Convert.ToInt32(value);
					break;

				case "AN_amr":
					_anAmr = Convert.ToInt32(value);
					break;

				case "AN_eas":
					_anEas = Convert.ToInt32(value);
					break;

				case "AN_fin":
					_anFin = Convert.ToInt32(value);
					break;

				case "AN_nfe":
					_anNfe = Convert.ToInt32(value);
					break;

				case "AN_oth":
					_anOth = Convert.ToInt32(value);
					break;

				case "AN_asj":
					_anAsj = Convert.ToInt32(value);
					break;

			    case "AN_sas":
			        _anSas = Convert.ToInt32(value);
			        break;

			    case "nhomalt":
			        _hcAll = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "nhomalt_afr":
			        _hcAfr = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "nhomalt_amr":
			        _hcAmr = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "nhomalt_eas":
			        _hcEas = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "nhomalt_fin":
			        _hcFin = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "nhomalt_nfe":
			        _hcNfe = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "nhomalt_oth":
			        _hcOth = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "nhomalt_asj":
			        _hcAsj = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

			    case "nhomalt_sas":
			        _hcSas = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

                case "DP":
					_totalDepth = Convert.ToInt32(value);
					break;

			}

		}

	}
}
