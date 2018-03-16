using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;

namespace Vcf.Sample
{
    public sealed class SampleFieldExtractor
    {
        private readonly string[] _vcfColumns;
        private FormatIndices _formatIndices;
        private readonly int? _infoDepth;

        internal SampleFieldExtractor(string[] vcfColumns, int? depth = null)
        {
            _vcfColumns = vcfColumns;
            _infoDepth  = depth;
        }

        /// <summary>
        /// extracts the genotype fields from the VCF file and returns a list of JSON samples
        /// </summary>
        internal ISample[] ExtractSamples()
        {
            // sanity check: make sure we have enough columns
            if (_vcfColumns.Length < VcfCommon.MinNumColumnsSampleGenotypes) return null;

            var nSamples = _vcfColumns.Length - VcfCommon.MinNumColumnsSampleGenotypes + 1;
            var samples = new ISample[nSamples];

            // extract the indices for each genotype field
            _formatIndices = FormatIndices.Extract(_vcfColumns[VcfCommon.FormatIndex]);

            // add each sample
            for (var index = VcfCommon.GenotypeIndex; index < _vcfColumns.Length; index++)
            {
                samples[index - VcfCommon.GenotypeIndex] = ExtractSample(_vcfColumns[index]);
            }

            return samples;
        }

        /// <summary>
        /// returns a JsonSample object given the data contained within the sample genotype
        /// field.
        /// </summary>
        private ISample ExtractSample(string sampleColumn)
        {
            // sanity check: make sure we have a format column
            if (_formatIndices == null || string.IsNullOrEmpty(sampleColumn)) return Sample.EmptySample;

            var sampleColumns = sampleColumn.Split(':');

            // handle missing sample columns
            if (sampleColumns.Length == 1 && sampleColumns[0] == ".") return Sample.EmptySample;

            var sampleFields = new IntermediateSampleFields(_vcfColumns, _formatIndices, sampleColumns);

            var alleleDepths      = AlleleDepths.GetAlleleDepths(sampleFields);
            var failedFilter      = FailedFilter.GetFailedFilter(sampleFields);
            var genotype          = Genotype.GetGenotype(sampleFields);

            var genotypeQuality    = GenotypeQuality.GetGenotypeQuality(sampleFields);
            var totalDepth         = TotalDepth.GetTotalDepth(_infoDepth, sampleFields);
            var variantFrequencies = VariantFrequency.GetVariantFrequencies(sampleFields);
            var splitReadCounts    = ReadCounts.GetSplitReadCounts(sampleFields);
            var pairEndReadCounts  = ReadCounts.GetPairEndReadCounts(sampleFields);

            var isLossOfHeterozygosity = sampleFields.MajorChromosomeCount != null && sampleFields.CopyNumber != null &&
                                         sampleFields.MajorChromosomeCount.Value == sampleFields.CopyNumber.Value &&
                                         sampleFields.CopyNumber.Value > 1;

            var sample = new Sample(genotype, genotypeQuality, variantFrequencies, totalDepth, alleleDepths, failedFilter,
                sampleFields.CopyNumber, isLossOfHeterozygosity, sampleFields.DenovoQuality, splitReadCounts,
                pairEndReadCounts, sampleFields.RepeatNumber, sampleFields.RepeatNumberSpan, sampleFields.MAD,
                sampleFields.SCH, sampleFields.PLG, sampleFields.PCN, sampleFields.DCS, sampleFields.DID,
                sampleFields.DST, sampleFields.PCH, sampleFields.CHC, sampleFields.AQ, sampleFields.LQ);

            return sample;
        }
    }
}