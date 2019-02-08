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
    public sealed class DbSnpReader : IDisposable
    {
        // Key in VCF info field of the allele frequencies subfield.
	    private readonly Stream _stream;
        private readonly IDictionary<string, IChromosome> _refChromDict;

        public DbSnpReader(Stream stream, IDictionary<string, IChromosome> refChromDict)
        {
            _stream = stream;
            _refChromDict = refChromDict;
        }
	    
	    public IEnumerable<DbSnpItem> GetItems()
        {
            using (var reader = FileUtilities.GetStreamReader(_stream))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Skip empty lines.
                    if (line.IsWhiteSpace()) continue;
                    // Skip comments.
                    if (line.OptimizedStartsWith('#')) continue;
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
            var splitLine = vcfline.Split('\t',6);
            if (splitLine.Length < 5) return null;

            var chromosomeName = splitLine[VcfCommon.ChromIndex];
            if (!_refChromDict.ContainsKey(chromosomeName)) return null;

            var chromosome = _refChromDict[chromosomeName];

            var position   = int.Parse(splitLine[VcfCommon.PosIndex]);
			var dbSnpId    = Convert.ToInt64(splitLine[VcfCommon.IdIndex].Substring(2));
			var refAllele  = splitLine[VcfCommon.RefIndex];
			var altAlleles = splitLine[VcfCommon.AltIndex].OptimizedSplit(',');
			
	        return altAlleles.Select(altAllele => new DbSnpItem(chromosome, position, dbSnpId, refAllele, altAllele)).ToList();
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
