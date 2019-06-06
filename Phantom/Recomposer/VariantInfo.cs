using System.Collections.Generic;
using System.Linq;
using Phantom.PositionCollections;

namespace Phantom.Recomposer
{
    public sealed class VariantInfo
    {
        public const string FailedFilterTag = "FilteredVariantsRecomposed";
        public readonly string Qual;
        private readonly string[] _positionFilters;
        private readonly List<bool>[] _sampleFilterFailed;
        public readonly string[] SampleGqs;
        public readonly string[] SamplePhaseSets;
        public readonly int?[] HomoReferenceSamplePloidies;
        public readonly Dictionary<string, (List<SampleHaplotype> SampleHaplotypes, List<string> LinkedVids)> AltAlleleToSample = new Dictionary<string, (List<SampleHaplotype>, List<string>)>();

        public VariantInfo(string qual, string[] positionFilters, string[] sampleGqs, string[] samplePhaseSets, int?[] homoReferenceSamplePloidies, List<bool>[] sampleFilterFailed)
        {
            Qual = qual;
            _positionFilters = positionFilters;
            SampleGqs = sampleGqs;
            SamplePhaseSets = samplePhaseSets;
            HomoReferenceSamplePloidies = homoReferenceSamplePloidies;
            _sampleFilterFailed = sampleFilterFailed;
        }

        public void AddAllele(string altAllele, List<SampleHaplotype> sampleAlleles, List<string> linkedVids)
        {
            AltAlleleToSample.Add(altAllele, (sampleAlleles, linkedVids));
        }

        public void UpdateSampleFilters(IEnumerable<int> variantPosIndexes, List<SampleHaplotype> sampleHaplotypes)
        {
            bool failed = variantPosIndexes.Select(x => _positionFilters[x]).Any(x => x != "PASS" && x != ".");
            sampleHaplotypes.ForEach(x => _sampleFilterFailed[x.SampleIndex].Add(failed));
        }

        // If the filter for any of the alleles in a sample is failed, the sample is assigned a failed filter tag
        // However, if any of the samples has a passed filter tag, this MNV position will have a "PASS" tag.
        // We don't want the passed MNV to be filtered out.
        public string GetMnvFilterTag() => _sampleFilterFailed.Where(x => x.Count > 0).Select(x => x.Any(y => y)).Any(x => !x) ? "PASS" : FailedFilterTag;
    }
}