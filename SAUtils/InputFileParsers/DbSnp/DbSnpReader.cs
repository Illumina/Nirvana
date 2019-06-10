using System;
using System.Collections.Generic;
using System.IO;
using IO;
using OptimizedCore;
using SAUtils.DataStructures;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Providers;
using Variants;

namespace SAUtils.InputFileParsers.DbSnp
{
    public sealed class DbSnpReader : IDisposable
    {
        // Key in VCF info field of the allele frequencies subfield.
	    private readonly Stream _stream;
        private readonly ISequenceProvider _sequenceProvider;

        public DbSnpReader(Stream stream, ISequenceProvider sequenceProvider)
        {
            _stream           = stream;
            _sequenceProvider = sequenceProvider;
        }
	    
	    public IEnumerable<DbSnpItem> GetItems()
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
                    
	                foreach (var dbSnpItem in ExtractItem(line))
	                {
						yield return dbSnpItem;
	                }
					
                }
            }
        }

        /// <summary>
        /// Extracts a dbSNP item from the specified VCF line.
        /// </summary>
        /// <param name="vcfLine"></param>
        /// <returns></returns>
        public IEnumerable<DbSnpItem> ExtractItem(string vcfLine)
        {
            var splitLine = vcfLine.Split('\t',6);
            if (splitLine.Length < 5) yield break;

            var chromosomeName = splitLine[VcfCommon.ChromIndex];
            if (!_sequenceProvider.RefNameToChromosome.ContainsKey(chromosomeName)) yield break;

            var chromosome = _sequenceProvider.RefNameToChromosome[chromosomeName];
            var position   = int.Parse(splitLine[VcfCommon.PosIndex]);
			var dbSnpId    = Convert.ToInt64(splitLine[VcfCommon.IdIndex].Substring(2));
			var refAllele  = splitLine[VcfCommon.RefIndex];
			var altAlleles = splitLine[VcfCommon.AltIndex].OptimizedSplit(',');

            foreach (var altAllele in altAlleles)
            {
                var (shiftedPos, shiftedRef, shiftedAlt) =
                    VariantUtils.TrimAndLeftAlign(position, refAllele, altAllele, _sequenceProvider.Sequence);

                yield return new DbSnpItem(chromosome, shiftedPos, dbSnpId, shiftedRef, shiftedAlt);
            }
	        
        }

        public void Dispose()
        {
            _stream?.Dispose();
        }
    }
}
