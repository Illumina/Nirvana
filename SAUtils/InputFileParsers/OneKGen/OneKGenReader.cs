using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using IO;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace SAUtils.InputFileParsers.OneKGen
{
    public sealed class OneKGenReader :IDisposable
    {
        private readonly Stream _stream;
        private readonly IDictionary<string,IChromosome> _refNameDictionary;
        private readonly ISequenceProvider _sequenceProvider;

        private  string _ancestralAllele;

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

        // empty constructor for onekg reader for unit tests.
        public OneKGenReader(Stream stream, ISequenceProvider sequenceProvider) 
        {
            _stream = stream;
            _sequenceProvider = sequenceProvider;
            _refNameDictionary = sequenceProvider.RefNameToChromosome;
        }

        private void Clear()
	    {
		    _ancestralAllele = null;

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
	    }

	    public IEnumerable<OneKGenItem> GetItems()
        {
            using (var reader = FileUtilities.GetStreamReader(_stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    // Skip comments.
                    if (line.OptimizedStartsWith('#')) continue;
                    
	                foreach (var oneKGenItem in ExtractItems(line))
	                {
						yield return oneKGenItem;
	                }
					
                }
            }
        }

        internal IEnumerable<OneKGenItem> ExtractItems(string vcfLine)
        {
            var splitLine = vcfLine.OptimizedSplit('\t');// we don't care about the many fields after info field
            if (splitLine.Length < 8) yield break;

            Clear();
			
            var chromosomeName  = splitLine[VcfCommon.ChromIndex];
            if (!_refNameDictionary.ContainsKey(chromosomeName)) yield break;
            var chromosome = _refNameDictionary[chromosomeName];
            var position   = int.Parse(splitLine[VcfCommon.PosIndex]);//we have to get it from RSPOS in info
            var refAllele  = splitLine[VcfCommon.RefIndex];
            var altAlleles = splitLine[VcfCommon.AltIndex].OptimizedSplit(',');
            var infoFields = splitLine[VcfCommon.InfoIndex];

            // parses the info fields and extract frequencies, ancestral allele, allele counts, etc.
            var hasSymbolicAllele = altAlleles.Any(x => x.OptimizedStartsWith('<') && x.OptimizedEndsWith('>'));
	        if (hasSymbolicAllele) yield break;

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
			ParseInfoField(infoFields, hasSymbolicAllele);

	        for (var i = 0; i < altAlleles.Length; i++)
            {
                var (shiftedPos, shiftedRef, shiftedAlt) = VariantUtils.TrimAndLeftAlign(position, refAllele,
                    altAlleles[i], _sequenceProvider.Sequence);

                yield return new OneKGenItem(
					chromosome,
					shiftedPos,
					shiftedRef,
					shiftedAlt,
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
					_sasAlleleNumber
					);

                
			}
			
        }

	    private static int? GetAlleleCount(int[] alleleCounts, int i)
	    {
		    if (alleleCounts == null) return null;
		    if (i >= alleleCounts.Length) return null;
		    return alleleCounts[i];
	    }

        private void ParseInfoField(string infoFields, bool hasSymbolicAllele)
        {
            if (infoFields == "" || infoFields == ".") return;
            var infoItems = infoFields.OptimizedSplit(';');

            foreach (string infoItem in infoItems)
            {
                (string key, string value) = infoItem.OptimizedKeyValue();

                // sanity check
                if (value != null) SetInfoField(key, value, hasSymbolicAllele);
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
					{
					}

				    break;
				case "END":
					if (hasSymbolicAllele)
					{
					}

				    break;
				case "CIEND":
				case "CIPOS":
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
					_allAlleleCounts = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;
				case "AMR_AC":
					_amrAlleleCounts = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;
				case "AFR_AC":
					_afrAlleleCounts = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;
				case "EUR_AC":
					_eurAlleleCounts = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;
				case "EAS_AC":
					_easAlleleCounts = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;
				case "SAS_AC":
					_sasAlleleCounts = value.OptimizedSplit(',').Select(val => Convert.ToInt32(val)).ToArray();
					break;
			}
		}

		private static string GetAncestralAllele(string value)
		{
			if (value == "" || value == ".") return null;

			var ancestralAllele = value.OptimizedSplit('|')[0];
			if (string.IsNullOrEmpty(ancestralAllele)) return null;
			return ancestralAllele.All(IsNucleotide) ? ancestralAllele : null;
		}
		private static bool IsNucleotide(char c)
		{
			c = char.ToUpper(c);
			return c == 'A' || c == 'C' || c == 'G' || c == 'T' || c == 'N';
		}

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
