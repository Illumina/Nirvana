using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Genome;
using IO;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;

namespace SAUtils.InputFileParsers.DbSnp
{
    public sealed class GlobalMinorReader 
    {
        // Key in VCF info field of the allele frequencies subfield.
	    private readonly Stream _stream;
        private readonly IDictionary<string, IChromosome> _refChromDict;

        public GlobalMinorReader(Stream stream, IDictionary<string, IChromosome> refChromDict)
        {
            _stream = stream;
            _refChromDict = refChromDict;
        }
	    
	    public IEnumerable<AlleleFrequencyItem> GetItems()
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
                    var items = ExtractItem(line);
	                if (items == null || items.Count == 0) continue;
	                foreach (var item in items)
	                {
						yield return item;
	                }
					
                }
            }
        }

        /// <summary>
        /// Extracts a dbSNP item from the specified VCF line.
        /// </summary>
        /// <param name="vcfline"></param>
        /// <returns></returns>
        private List<AlleleFrequencyItem> ExtractItem(string vcfline)
        {
            var splitLine = vcfline.OptimizedSplit('\t');
            if (splitLine.Length < 8) return null;

            var chromosomeName = splitLine[VcfCommon.ChromIndex];
            if (!_refChromDict.ContainsKey(chromosomeName)) return null;

            var chromosome = _refChromDict[chromosomeName];

            var position   = int.Parse(splitLine[VcfCommon.PosIndex]);
			var refAllele  = splitLine[VcfCommon.RefIndex];
			var altAlleles = splitLine[VcfCommon.AltIndex].OptimizedSplit(',');
	        var infoField  = splitLine[VcfCommon.InfoIndex];
			
			var alleleFrequencies = GetAlleleFrequencies(infoField, refAllele, altAlleles);

            var frequencyItems = new List<AlleleFrequencyItem>();
            foreach ((string allele, double frequency) in alleleFrequencies)
            {
                frequencyItems.Add(new AlleleFrequencyItem(chromosome, position, refAllele, allele, frequency));
            }

            return frequencyItems;
        }


        private static Dictionary<string, double> GetAlleleFrequencies(string infoField, string refAllele, string[] altAlleles)
        {
            var freqDict = new Dictionary<string, double> { [refAllele] = double.MinValue };

            foreach (var altAllele in altAlleles)
            {
                freqDict[altAllele] = double.MinValue;
            }

            if (infoField.Trim() == ".") return freqDict;

            // for now we also want to disregard anything other than SNVs
            var allSnv = refAllele.Length == 1 && altAlleles.All(altAllele => altAllele.Length == 1);
            if (!allSnv) return freqDict;

            // return if there are no freq information
            if (!infoField.Contains("CAF="))
                return freqDict;

            foreach (var info in infoField.OptimizedSplit(';'))
            {
                if (!info.StartsWith("CAF=")) continue;
                var alleleFrequencies = info.OptimizedKeyValue().Value.OptimizedSplit(',');

                freqDict[refAllele] = GetFrequency(alleleFrequencies[0]);

                for (int i = 1; i < alleleFrequencies.Length; i++)
                    freqDict[altAlleles[i - 1]] = GetFrequency(alleleFrequencies[i]);
                break;
            }

            return freqDict;
        }

        private static double GetFrequency(string alleleFrequency)
	    {
		    return alleleFrequency == "." || alleleFrequency == "0" ? double.MinValue : Convert.ToDouble(alleleFrequency);
	    }
        
    }
}
