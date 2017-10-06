using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Sequence;

namespace SAUtils.InputFileParsers.DbSnp
{
    public sealed class DbSnpReader 
    {
        // Key in VCF info field of the allele frequencies subfield.
	    private readonly Stream _stream;
        private readonly IDictionary<string, IChromosome> _refChromDict;



        public DbSnpReader(Stream stream, IDictionary<string, IChromosome> refChromDict)
        {
            _stream = stream;
            _refChromDict = refChromDict;
        }

	    
	    public IEnumerable<DbSnpItem> GetDbSnpItems()
        {
            using (var reader = new StreamReader(_stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    // Skip comments.
                    if (line.StartsWith("#")) continue;
                    var dbSnpItems = ExtractItem(line);
	                if (dbSnpItems == null || dbSnpItems.Count == 0) continue;
	                foreach (var dbSnpItem in dbSnpItems)
	                {
						yield return dbSnpItem;
	                }
					
                }
            }
        }

        /// <summary>
        /// Extracts a dbSNP item from the specified VCF line.
        /// </summary>
        /// <param name="vcfline"></param>
        /// <returns></returns>
        public List<DbSnpItem> ExtractItem(string vcfline)
        {
            var splitLine = vcfline.Split('\t');
            if (splitLine.Length < 8) return null;

            var chromosomeName = splitLine[VcfCommon.ChromIndex];
            if (!_refChromDict.ContainsKey(chromosomeName)) return null;

            var chromosome = _refChromDict[chromosomeName];

            var position   = int.Parse(splitLine[VcfCommon.PosIndex]);
			var dbSnpId    = Convert.ToInt64(splitLine[VcfCommon.IdIndex].Substring(2));
			var refAllele  = splitLine[VcfCommon.RefIndex];
			var altAlleles = splitLine[VcfCommon.AltIndex].Split(',');
	        var infoField  = splitLine[VcfCommon.InfoIndex];
			
			var alleleFrequencies = GetAlleleFrequencies(infoField, refAllele, altAlleles);
	        
	        return altAlleles.Select(altAllele => new DbSnpItem(chromosome, position, dbSnpId, refAllele, alleleFrequencies[refAllele], altAllele, alleleFrequencies[altAllele])).ToList();
        }


	    private static Dictionary<string, double> GetAlleleFrequencies(string infoField, string refAllele, string[] altAlleles)
	    {
		    var freqDict = new Dictionary<string, double> {[refAllele] = double.MinValue};

		    foreach (var altAllele in altAlleles)
		    {
			    freqDict[altAllele] = double.MinValue;
		    }

			if (infoField.Trim() == ".") return freqDict;

			// for now we also want to disregard anything other than SNVs
		    var allSnv = refAllele.Length == 1 && altAlleles.All(altAllele => altAllele.Length == 1);
		    if (! allSnv) return freqDict;
		    
			// return if there are no freq information
		    if (!infoField.Contains("CAF="))
			    return freqDict;

			foreach (var info in infoField.Split(';'))
		    {
			    if (!info.StartsWith("CAF=")) continue;
			    var alleleFrequencies = info.Split('=')[1].Split(',');

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
