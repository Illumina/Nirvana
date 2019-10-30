using OptimizedCore;
using VariantAnnotation.Interface.Positions;

namespace Vcf.Sample.Legacy
{
    public sealed class LegacySampleFieldExtractor
    {
        private readonly string[] _vcfColumns;
        private readonly FormatIndices _formatIndices;
        private readonly int? _infoDepth;

        internal LegacySampleFieldExtractor(string[] vcfColumns, FormatIndices formatIndices, int? depth = null)
        {
            _vcfColumns = vcfColumns;
            _infoDepth  = depth;
            _formatIndices = formatIndices;
        }

        internal ISample ExtractSample(string sampleColumn)
        {
            // sanity check: make sure we have a format column
            if (_formatIndices == null || string.IsNullOrEmpty(sampleColumn)) return Sample.EmptySample;

            var sampleColumns = sampleColumn.OptimizedSplit(':');

            // handle missing sample columns
            if (sampleColumns.Length == 1 && sampleColumns[0] == ".") return Sample.EmptySample;

            var sampleFields = new IntermediateSampleFields(_vcfColumns, _formatIndices, sampleColumns);

            var alleleDepths  = AlleleDepths.GetAlleleDepths(sampleFields);
            bool failedFilter = FailedFilter.GetFailedFilter(sampleFields);
            string genotype   = Genotype.GetGenotype(sampleFields);

            var genotypeQuality    = GenotypeQuality.GetGenotypeQuality(sampleFields);
            var totalDepth         = TotalDepth.GetTotalDepth(_infoDepth, sampleFields);
            var variantFrequencies = LegacyVariantFrequency.GetVariantFrequencies(sampleFields);
            var splitReadCounts    = ReadCounts.GetSplitReadCounts(sampleFields);
            var pairEndReadCounts  = ReadCounts.GetPairEndReadCounts(sampleFields);

            bool isLossOfHeterozygosity = sampleFields.MajorChromosomeCount != null &&
                                          sampleFields.CopyNumber != null &&
                                          sampleFields.MajorChromosomeCount.Value == sampleFields.CopyNumber.Value &&
                                          sampleFields.CopyNumber.Value > 1;

            var sample = new Sample(alleleDepths, null, sampleFields.CopyNumber,null, failedFilter, genotype,genotypeQuality, false, null, pairEndReadCounts, null, splitReadCounts, totalDepth,variantFrequencies,null,null,isLossOfHeterozygosity);

            return sample;
        }
    }
}