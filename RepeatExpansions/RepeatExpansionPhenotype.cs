using System.Collections.Generic;
using System.Linq;
using Genome;
using Intervals;

namespace RepeatExpansions
{
    public sealed class RepeatExpansionPhenotype
    {
        public readonly IChromosomeInterval ChromosomeInterval;

        // used directly in JSON output
        private readonly string _phenotype;
        private readonly string _omimId;

        // used during annotation
        private readonly int[] _repeatNumbers;
        private readonly double[] _percentiles;
        private readonly string[] _classifications;
        private readonly Interval[] _classificationRanges;

        public RepeatExpansionPhenotype(IChromosomeInterval chromosomeInterval, string phenotype, string omimId,
            int[] repeatNumbers, double[] percentiles, string[] classifications, Interval[] classificationRanges)
        {
            ChromosomeInterval    = chromosomeInterval;
            _phenotype            = phenotype;
            _omimId               = omimId;
            _repeatNumbers        = repeatNumbers;
            _percentiles          = percentiles;
            _classifications      = classifications;
            _classificationRanges = classificationRanges;
        }

        public string GetAnnotation(int repeatNumber)
        {
            double percentile                   = PercentileUtilities.GetPercentile(repeatNumber, _repeatNumbers, _percentiles);
            IEnumerable<string> classifications = GetClassifications(repeatNumber);

            return GetJson(percentile, classifications);
        }

        private string GetJson(double percentile, IEnumerable<string> classifications)
        {
            string joined = string.Join(",", classifications.Select(classification => "\"" + classification + "\""));
            return $"{{\"phenotype\":\"{_phenotype}\",\"omimId\":{_omimId},\"classifications\":[{joined}],\"percentile\":{percentile:0.00}}}";
        }

        private IEnumerable<string> GetClassifications(int repeatNumber)
        {
            var classifications = new List<string>();

            for (var i = 0; i < _classificationRanges.Length; i++)
            {
                var range = _classificationRanges[i];
                if (range.Start <= repeatNumber && repeatNumber <= range.End) classifications.Add(_classifications[i]);
            }

            return classifications;
        }
    }
}