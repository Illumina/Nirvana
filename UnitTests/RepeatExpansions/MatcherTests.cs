using System.Text;
using Genome;
using Intervals;
using RepeatExpansions;
using UnitTests.TestUtilities;
using Variants;
using Xunit;

namespace UnitTests.RepeatExpansions
{
    public sealed class MatcherTests
    {
        private readonly Matcher _matcher;

        public MatcherTests()
        {
            var repeatNumbers    = new[] { 7, 8, 9 };
            double[] percentiles = { 0, 1, 1.5 };

            var classificationRanges = new[] { new Interval(0, 27) };
            var classifications      = new[] { "Normal" };

            var aInterval = new ChromosomeInterval(ChromosomeUtilities.Chr1, 100, 200);
            var aPhenotype = new RepeatExpansionPhenotype(aInterval, "A", null, repeatNumbers, percentiles, classifications, classificationRanges);

            var chr1Phenotypes = new Interval<RepeatExpansionPhenotype>[1];
            chr1Phenotypes[0] = new Interval<RepeatExpansionPhenotype>(aInterval.Start, aInterval.End, aPhenotype);

            var intervalArrays = new IntervalArray<RepeatExpansionPhenotype>[1];
            intervalArrays[ChromosomeUtilities.Chr1.Index] = new IntervalArray<RepeatExpansionPhenotype>(chr1Phenotypes);

            var phenotypeForest = new IntervalForest<RepeatExpansionPhenotype>(intervalArrays);
            _matcher = new Matcher(phenotypeForest);
        }

        [Fact]
        public void GetMatchingAnnotations_Overlap_ReturnEntry()
        {
            var variant = new RepeatExpansion(ChromosomeUtilities.Chr1, 100, 200, null, null, null, 9, 7);
            var sa      = _matcher.GetMatchingAnnotations(variant);

            var sb = new StringBuilder();
            sa.SerializeJson(sb);
            string observedResult = sb.ToString();

            Assert.Contains("{\"phenotype\":\"A\"", observedResult);
        }

        [Fact]
        public void GetMatchingAnnotations_NoOverlap_ReturnNull()
        {
            var variant = new RepeatExpansion(ChromosomeUtilities.Chr1, 220, 230, null, null, null, 9, 7);
            var sa = _matcher.GetMatchingAnnotations(variant);
            Assert.Null(sa);
        }
    }
}
