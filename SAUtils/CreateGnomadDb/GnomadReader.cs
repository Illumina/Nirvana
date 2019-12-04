using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using VariantAnnotation.Interface.SA;
using Variants;

namespace SAUtils.CreateGnomadDb
{
	public sealed class GnomadReader 
	{
        private readonly StreamReader _genomeReader;
        private readonly StreamReader _exomeReader;
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

        //male numbers
        private int[] _acMale;
        private int _anMale;
        private int[] _hcMale;
        
        //female numbers
        private int[] _acFemale;
        private int _anFemale;
        private int[] _hcFemale;
        
        private int[] _hcAll;
	    private int[] _hcAfr;
	    private int[] _hcAmr;
	    private int[] _hcEas;
	    private int[] _hcFin;
	    private int[] _hcNfe;
	    private int[] _hcOth;
	    private int[] _hcAsj;
	    private int[] _hcSas;

        // controls
        private int[] _control_acAll;
        private int _control_anAll;

        private bool _isLowComplexityRegion;
        private int? _totalDepth;
        private GnomadDataType _dataType;

		public GnomadReader(StreamReader genomeReader, StreamReader exomeReader, ISequenceProvider sequenceProvider) 
		{
			_genomeReader     = genomeReader;
            _exomeReader      = exomeReader;
            _sequenceProvider = sequenceProvider;
		}

		private void Clear()
        {
            _isLowComplexityRegion = false;
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

            _acMale = null;
            _anMale = 0;
            _hcMale = null;

            _acFemale = null;
            _anFemale = 0;
            _hcFemale = null;
            
            _hcAll = null;
		    _hcAfr = null;
		    _hcAmr = null;
		    _hcEas = null;
		    _hcFin = null;
		    _hcNfe = null;
		    _hcOth = null;
		    _hcAsj = null;
		    _hcSas = null;

            //control
            _control_acAll = null;
            _control_anAll = 0;

            _totalDepth = null;
            _dataType = GnomadDataType.Unknown;
        }

        /// <summary>
        /// Merging genomic an exomic items to create one stream of gnomad entries
        /// </summary>
        /// <returns></returns>
        public IEnumerable<GnomadItem> GetCombinedItems()
        {
            using (var genomeEnumerator = GetItems(_genomeReader, GnomadDataType.Genome).GetEnumerator())
            using (var exomeEnumerator  = GetItems(_exomeReader, GnomadDataType.Exome).GetEnumerator())
            {
                var hasGenomicItem = genomeEnumerator.MoveNext();
                var hasExomeItem = exomeEnumerator.MoveNext();

                var minHeap = new MinHeap<GnomadItem>(GnomadItem.CompareTo);
                while (hasExomeItem && hasGenomicItem)
                {
                    var genomeItem = genomeEnumerator.Current;
                    var exomeItem  = exomeEnumerator.Current;
                    var position = Math.Min(genomeItem.Position, exomeItem.Position);

                    while (hasGenomicItem && genomeItem.Position == position)
                    {
                        //all items for a position should be gathered so as to resolve conflicts properly
                        minHeap.Add(GnomadUtilities.GetNormalizedItem(genomeItem, _sequenceProvider));
                        hasGenomicItem = genomeEnumerator.MoveNext();
                        genomeItem = genomeEnumerator.Current;
                    }

                    while (hasExomeItem && exomeItem.Position == position)
                    {
                        minHeap.Add(GnomadUtilities.GetNormalizedItem(exomeItem, _sequenceProvider));
                        hasExomeItem = exomeEnumerator.MoveNext();
                        exomeItem = exomeEnumerator.Current;
                    }
                    // at this point, the min heap should not be empty
                    int heapPosition = minHeap.GetMin().Position;

                    while (minHeap.Count() > 0 && heapPosition < position - VariantUtils.MaxUpstreamLength)
                    {
                        var (genomeItems, exomeItems) = GetMinItems(minHeap);
                        foreach (var item in GnomadUtilities.GetMergedItems(genomeItems, exomeItems).Values)
                        {
                            if (item.AllAlleleNumber == null || item.AllAlleleNumber.Value == 0) continue;
                            yield return item;
                        }
                        
                    }

                }
                //flush out the last positions in heap
                while (minHeap.Count() > 0)
                {
                    var (genomeItems, exomeItems) = GetMinItems(minHeap);
                    foreach (var item in GnomadUtilities.GetMergedItems(genomeItems, exomeItems).Values)
                        yield return item;
                }
                //now, only one of the iterator is left
                if (hasGenomicItem)
                    foreach (var item in GetRemainingItems(genomeEnumerator)) yield return item;

                if (hasExomeItem)
                    foreach (var item in GetRemainingItems(exomeEnumerator)) yield return item;
            }
        }

        private (Dictionary<(string refAllele, string altAllele), GnomadItem> genomeItems, Dictionary<(string refAllele, string altAllele), GnomadItem> exomeItems) GetMinItems(MinHeap<GnomadItem> minHeap)
        {
            var genomeItems = new List<ISupplementaryDataItem>();
            var exomeItems = new List<ISupplementaryDataItem>();

            if (minHeap.Count() == 0) return (null, null);
            var position = minHeap.GetMin().Position;
            
            while (minHeap.Count() > 0 && minHeap.GetMin().Position == position)
            {
                var item = minHeap.ExtractMin();
                if(item.DataType == GnomadDataType.Genome) genomeItems.Add(item);
                else exomeItems.Add(item);
            }

            genomeItems = SuppDataUtilities.RemoveConflictingAlleles(genomeItems, false);
            exomeItems  = SuppDataUtilities.RemoveConflictingAlleles(exomeItems, false);

            var genomeItemsByAllele = new Dictionary<(string refAllele, string altAllele), GnomadItem>();
            foreach (var item in genomeItems)
            {
                genomeItemsByAllele.Add((item.RefAllele, item.AltAllele) , (GnomadItem)item);
            }

            var exomeItemsByAllele = new Dictionary<(string refAllele, string altAllele), GnomadItem>();
            foreach (var item in exomeItems)
            {
                exomeItemsByAllele.Add((item.RefAllele, item.AltAllele), (GnomadItem)item);
            }
            return (genomeItemsByAllele, exomeItemsByAllele);
        }

        private IEnumerable<GnomadItem> GetRemainingItems(IEnumerator<GnomadItem> enumerator)
        {
            do
            {
                var item = enumerator.Current;
                if(item == null) yield break;
                if (item.AllAlleleNumber == null || item.AllAlleleNumber.Value == 0) continue;
                yield return GnomadUtilities.GetNormalizedItem(item, _sequenceProvider);
            } while (enumerator.MoveNext());
        }

        /// <summary>
		/// Parses a source file and return an enumeration object containing 
		/// all the data objects that have been extracted.
		/// </summary>
		/// <returns></returns>
        private IEnumerable<GnomadItem> GetItems(StreamReader reader, GnomadDataType type)
		{
            if(reader == null) yield break;
			using (reader)
			{
				string line;
				while ((line = reader.ReadLine()) != null)
				{
					// Skip empty lines.
					if (string.IsNullOrWhiteSpace(line)) continue;

					// Skip comments.
					if (line.OptimizedStartsWith('#')) continue;
					var items = ExtractItems(line, type);
					if (items == null) continue;
					foreach (var item in items)
					{
						yield return item;
					}
				}
			}
		}

       
        /// <summary>
        /// Extracts a gnomad item(s) from the specified VCF line.
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        private List<GnomadItem> ExtractItems(string line, GnomadDataType type)
		{
			if (line == null) return null;
            var splitLine = line.OptimizedSplit('\t');

            if (splitLine.Length < 8) return null;

			Clear();

			var chromosome = splitLine[VcfCommon.ChromIndex];
			if (!_sequenceProvider.RefNameToChromosome.ContainsKey(chromosome)) return null;

		    var chrom      = _sequenceProvider.RefNameToChromosome[chromosome];
			var position   = int.Parse(splitLine[VcfCommon.PosIndex]);
			var refAllele  = splitLine[VcfCommon.RefIndex];
			var altAlleles = splitLine[VcfCommon.AltIndex].OptimizedSplit(',');
		    var filters    = splitLine[VcfCommon.FilterIndex];
			var infoFields = splitLine[VcfCommon.InfoIndex];

		    var hasFailedFilters = !(filters.Equals("PASS") || filters.Equals("."));

            // parses the info fields and extract frequencies, coverage, num samples.
            ParseInfoField(infoFields);
            
		    var gnomadItemsList = new List<GnomadItem>();
            
			for (int i = 0; i < altAlleles.Length; i++)
            {
                gnomadItemsList.Add(new GnomadItem(
					chrom,
					position,
					refAllele,
					altAlleles[i],
                    _totalDepth,
					_anAll, _anAfr,_anAmr,_anEas,_anFin,_anNfe,_anOth, _anAsj, _anSas, _anMale, _anFemale,
					GetCount(_acAll, i), GetCount(_acAfr, i), GetCount(_acAmr, i), GetCount(_acEas, i), 
					GetCount(_acFin, i), GetCount(_acNfe, i), GetCount(_acOth, i), GetCount(_acAsj, i),
			        GetCount(_acSas, i), GetCount(_acMale, i), GetCount(_acFemale, i),
					GetCount(_hcAll, i), GetCount(_hcAfr, i), GetCount(_hcAmr, i), GetCount(_hcEas, i), GetCount(_hcFin, i),
					GetCount(_hcNfe, i), GetCount(_hcOth, i), GetCount(_hcAsj, i), GetCount(_hcSas, i),
                    GetCount(_hcMale, i), GetCount(_hcFemale, i),
                    //controls
                    _control_anAll,
                    GetCount(_control_acAll, i),
                    hasFailedFilters,
                    _isLowComplexityRegion,
                    type)
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
                if (key == "lcr") _isLowComplexityRegion = true;
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

                case "AC_male":
                    _acMale = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

                case "AC_female":
                    _acFemale = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
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

                case "AN_male":
                    _anMale = Convert.ToInt32(value);
                    break;

                case "AN_female":
                    _anFemale = Convert.ToInt32(value);
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

                case "nhomalt_male":
                    _hcMale = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;
                case "nhomalt_female":
                    _hcFemale = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
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

                // controls
                case "controls_AN":
                    _control_anAll = Convert.ToInt32(value);
                    break;

                case "controls_AC":
                    _control_acAll = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
                    break;

                case "DP":
					_totalDepth = Convert.ToInt32(value);
					break;

			}

		}

	}
}
