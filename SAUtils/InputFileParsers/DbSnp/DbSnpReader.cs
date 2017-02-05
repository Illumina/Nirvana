using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Utilities;

namespace SAUtils.InputFileParsers.DbSnp
{
    /// <summary>
    /// Parser for dbSNP Wiggle file
    /// </summary>
    public sealed class DbSnpReader : IEnumerable<DbSnpItem>
    {
        // Key in VCF info field of the allele frequencies subfield.
	    private readonly FileInfo _dbSnpFile;
	    private readonly Stream _stream;
        private readonly ChromosomeRenamer _renamer;

        public DbSnpReader(ChromosomeRenamer renamer)
        {
            _renamer = renamer;
        }

	    public DbSnpReader(FileInfo dbSnpFile, ChromosomeRenamer renamer) : this(renamer)
        {
            _dbSnpFile = dbSnpFile;
        }

	    public DbSnpReader(Stream stream, ChromosomeRenamer renamer) : this(renamer)
        {
		    _stream = stream;
	    }

	    /// <summary>
        /// Parses a dbSNP file and return an enumeration object containing 
        /// all the dbSNP objects that have been extracted.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<DbSnpItem> GetDbSnpItems()
        {
            using (var reader = _stream == null? GZipUtilities.GetAppropriateStreamReader(_dbSnpFile.FullName): new StreamReader(_stream))
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

			var chromosome = splitLine[VcfCommon.ChromIndex];
			if (!InputFileParserUtilities.IsDesiredChromosome(chromosome, _renamer)) return null;

			var position   = int.Parse(splitLine[VcfCommon.PosIndex]);
			var dbSnpId    = Convert.ToInt64(splitLine[VcfCommon.IdIndex].Substring(2));
			var refAllele  = splitLine[VcfCommon.RefIndex];
			var altAlleles = splitLine[VcfCommon.AltIndex].Split(',');
	        var infoField  = splitLine[VcfCommon.InfoIndex];
			
			var alleleFrequencies = GetAlleleFrequencies(infoField, refAllele, altAlleles);
	        
	        return altAlleles.Select(altAllele => new DbSnpItem(chromosome, position, dbSnpId, refAllele, alleleFrequencies[refAllele], altAllele, alleleFrequencies[altAllele], infoField)).ToList();
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

	    public IEnumerator<DbSnpItem> GetEnumerator()
        {
            return GetDbSnpItems().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
