using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.Interface;
using Xunit;

namespace UnitTests.Utilities
{
    internal static class JsonUtilities
    {
        /// <summary>
        /// returns a JSON object given a vcf line (no annotation is performed)
        /// </summary>
        internal static UnifiedJson GetJson(string vcfLine, IChromosomeRenamer renamer)
        {
            var variantFeature = VcfUtilities.GetVariant(vcfLine, renamer);
            var json = new UnifiedJson(variantFeature);
            json.AddVariantData(variantFeature);
            return json;
        }

        // ReSharper disable once UnusedParameter.Global
        internal static void AlleleEquals(string vcfLine, string saPath, string expected, int alleleIndex = 0)
        {
            var observed = GetAlleleJson(vcfLine, new List<string> { saPath }, alleleIndex);
            Assert.Equal(expected, observed);
        }

        // ReSharper disable once UnusedParameter.Global
        internal static void AlleleEquals(IAnnotatedVariant annotatedVariant, string expected, int alleleIndex = 0)
        {
            var observed = GetAllele(annotatedVariant, alleleIndex);
            Assert.Equal(expected, observed);
        }

        // ReSharper disable once UnusedParameter.Global
        internal static void AlleleContains(string vcfLine, string saPath, string expected, int alleleIndex = 0)
        {
            var observed = GetAlleleJson(vcfLine, new List<string> { saPath }, alleleIndex);
            Assert.Contains(expected, observed);
        }

        // ReSharper disable once UnusedParameter.Global
        internal static void AlleleContains(string vcfLine, List<string> supplementaryFiles, string expected, int alleleIndex = 0)
        {
            var observed = GetAlleleJson(vcfLine, supplementaryFiles, alleleIndex);
            Assert.Contains(expected, observed);
        }

        // ReSharper disable once UnusedParameter.Global
        internal static void AlleleDoesNotContain(string vcfLine, List<string> supplementaryFiles, string expected, int alleleIndex = 0)
        {
            var observed = GetAlleleJson(vcfLine, supplementaryFiles, alleleIndex);
            Assert.DoesNotContain(expected, observed);
        }

        // ReSharper disable once UnusedParameter.Global
        internal static void AlleleContains(IAnnotatedVariant annotatedVariant, string expected, int alleleIndex = 0)
        {
            var observed = GetAllele(annotatedVariant, alleleIndex);
            Assert.Contains(expected, observed);
        }

		internal static string GetAllele(IAnnotatedVariant annotatedVariant, int alleleIndex = 0)
        {
            var altAllele = annotatedVariant.AnnotatedAlternateAlleles.ElementAt(alleleIndex);
            Assert.NotNull(altAllele);

            return altAllele.ToString();
        }

        internal static string GetSampleJson(IAnnotatedVariant annotatedVariant, int sampleIndex)
        {
            var sample = annotatedVariant.AnnotatedSamples.ElementAt(sampleIndex);
            Assert.NotNull(sample);

            return sample.ToString();
        }

        private static string GetAlleleJson(string vcfLine, List<string> supplementaryFiles, int alleleIndex)
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, supplementaryFiles, vcfLine);
            Assert.NotNull(annotatedVariant);

            return GetAllele(annotatedVariant, alleleIndex);
        }

		internal static string GetFirstAlleleJson(IAnnotatedVariant annotatedVariant)
            => annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault()?.ToString();


		internal static List<string> GetOverlappingTranscriptIds(IAnnotatedAlternateAllele annotatedAllele)
		{
			return annotatedAllele.SvOverlappingTranscripts.Select(transcript => transcript.TranscriptID).ToList();
		}
    }
}
