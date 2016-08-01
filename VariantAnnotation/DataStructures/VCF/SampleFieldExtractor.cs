using System.Collections.Generic;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.FileHandling;

namespace VariantAnnotation.DataStructures.VCF
{
    internal class SampleFieldExtractor
    {
        #region members

        private readonly string[] _vcfColumns;
        private FormatIndices _formatIndices;
        private readonly int? _infoDepth;

        #endregion

        // constructor
        internal SampleFieldExtractor(string[] vcfColumns, int? depth = null)
        {
            _vcfColumns = vcfColumns;
            _infoDepth  = depth;
        }

        /// <summary>
        /// extracts the genotype fields from the VCF file and returns a list of JSON samples
        /// </summary>
        internal List<JsonSample> ExtractSamples(bool fixGatkGenomeVcf = false)
        {
            // sanity check: make sure we have enough columns
            if (_vcfColumns.Length < VcfCommon.MinNumColumnsSampleGenotypes) return null;

            var samples = new List<JsonSample>();

            // extract the indices for each genotype field
            _formatIndices = FormatIndices.Extract(_vcfColumns[VcfCommon.FormatIndex]);

            // add each sample
            for (int index = VcfCommon.GenotypeIndex; index < _vcfColumns.Length; index++)
            {
                samples.Add(ExtractSample(_vcfColumns[index], fixGatkGenomeVcf));
            }

            return samples;
        }

        /// <summary>
        /// returns a JsonSample object given the data contained within the sample genotype
        /// field.
        /// </summary>
        private JsonSample ExtractSample(string sampleColumn, bool fixGatkGenomeVcf = false)
        {
            // sanity check: make sure we have a format column
            if (_formatIndices == null || string.IsNullOrEmpty(sampleColumn)) return EmptySample();

            var sampleColumns = sampleColumn.Split(':');

			// handle missing sample columns
			if ((sampleColumns.Length == 1) && sampleColumns[0] == ".") return EmptySample();
			
			var tmp = new IntermediateSampleFields(_vcfColumns, _formatIndices, sampleColumns, fixGatkGenomeVcf);

            var sample = new JsonSample
            {
	            AlleleDepths                                            = new AlleleDepths(tmp).GetAlleleDepths(),
	            FailedFilter                                            = new FailedFilter(tmp).GetFailedFilter(),
	            Genotype                                                = new Genotype(tmp).GetGenotype(),
	            GenotypeQuality                                         = new GenotypeQuality(tmp).GetGenotypeQuality(),
	            TotalDepth                                              = new TotalDepth(tmp).GetTotalDepth(_infoDepth),
	            VariantFrequency                                        = new VariantFrequency(tmp).GetVariantFrequency(),
	            CopyNumber                                              = tmp.CopyNumber?.ToString(),
	            IsLossOfHeterozygosity                                  = tmp.MajorChromosomeCount != null && tmp.CopyNumber != null &&
	                                     tmp.MajorChromosomeCount.Value == tmp.CopyNumber.Value
            };

            // sanity check: handle empty samples
            if (sample.IsNull()) sample.IsEmpty = true;
	        
            return sample;
        }

        /// <summary>
        /// returns a sample where the empty flag is enabled
        /// </summary>
        private static JsonSample EmptySample()
        {
            return new JsonSample { IsEmpty = true };
        }
    }
}
