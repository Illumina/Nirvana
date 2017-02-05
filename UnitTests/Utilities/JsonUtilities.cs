using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    internal static class JsonUtilities
    {
        /// <summary>
        /// returns a JSON object given a vcf line (no annotation is performed)
        /// </summary>
        internal static UnifiedJson GetJson(string vcfLine, ChromosomeRenamer renamer)
        {
            var variantFeature = VcfUtilities.GetVariant(vcfLine, renamer);
            var json = new UnifiedJson(variantFeature);
            json.AddVariantData(variantFeature);
            return json;
        }

        // ReSharper disable once UnusedParameter.Global
        internal static void AlleleEquals(string vcfLine, string supplementaryAnnotationPath, string expected, int alleleIndex = 0)
        {
            var observed = GetAlleleJson(vcfLine, supplementaryAnnotationPath, alleleIndex);
            Assert.Equal(expected, observed);
        }

        // ReSharper disable once UnusedParameter.Global
        internal static void AlleleEquals(IAnnotatedVariant annotatedVariant, string expected, int alleleIndex = 0)
        {
            var observed = GetAllele(annotatedVariant, alleleIndex);
            Assert.Equal(expected, observed);
        }

        // ReSharper disable once UnusedParameter.Global
        internal static void AlleleContains(string vcfLine, string supplementaryFile, string expected, int alleleIndex = 0)
        {
            var observed = GetAlleleJson(vcfLine, supplementaryFile, alleleIndex);
            Assert.Contains(expected, observed);
        }

        // ReSharper disable once UnusedParameter.Global
        internal static void AlleleContains(IAnnotatedVariant annotatedVariant, string expected, int alleleIndex = 0)
        {
            var observed = GetAllele(annotatedVariant, alleleIndex);
            Assert.Contains(expected, observed);
        }

		internal static void CustomAlleleContains(string vcfLine, string saFile, List<string> caFiles, string expected, int alleleIndex = 0)
		{
			var observed = GetCustomAlleleJson(vcfLine, saFile, caFiles, alleleIndex);
			Assert.Contains(expected, observed);
		}

		internal static void CustomAlleleDoesNotContains(string vcfLine, string saFile, List<string> caFiles, string expected, int alleleIndex = 0)
		{
			var observed = GetCustomAlleleJson(vcfLine, saFile, caFiles, alleleIndex);
			Assert.DoesNotContain(expected, observed);
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

        private static string GetAlleleJson(string vcfLine, string supplementaryFile, int alleleIndex)
        {
            var annotatedVariant = DataUtilities.GetVariant(DataUtilities.EmptyCachePrefix, supplementaryFile, vcfLine);
            Assert.NotNull(annotatedVariant);

            return GetAllele(annotatedVariant, alleleIndex);
        }

		private static string GetCustomAlleleJson(string vcfLine, string saPath, List<string> caFiles, int alleleIndex)
		{
			var annotatedVariant = DataUtilities.GetCustomVariant(DataUtilities.EmptyCachePrefix, saPath, caFiles, vcfLine);
			Assert.NotNull(annotatedVariant);

			return GetAllele(annotatedVariant, alleleIndex);
		}

		/// <summary>
		/// returns the first alternate allele in JSON representation
		/// </summary>
		internal static string GetFirstAlleleJson(IAnnotatedVariant annotatedVariant)
            => annotatedVariant.AnnotatedAlternateAlleles.FirstOrDefault()?.ToString();


		internal static List<string> GetOverlappingTranscriptIds(IAnnotatedAlternateAllele annotatedAllele)
		{
			return annotatedAllele.SvOverlappingTranscripts.Select(transcript => transcript.TranscriptID).ToList();
		}
    }


}
