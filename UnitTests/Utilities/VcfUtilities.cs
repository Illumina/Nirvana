using VariantAnnotation.DataStructures;
using VariantAnnotation.DataStructures.JsonAnnotations;
using VariantAnnotation.DataStructures.VCF;
using VariantAnnotation.FileHandling;
using VariantAnnotation.Interface;
using VariantAnnotation.Utilities;
using Xunit;

namespace UnitTests.Utilities
{
    internal static class VcfUtilities
    {
        public static string GetVcfColumn(ISupplementaryAnnotationReader saReader, string vcfLine,
            int vcfColumn)
        {
            var vcfVariant = GetVcfVariant(vcfLine);
            var annotationSource = ResourceUtilities.GetAnnotationSource(DataUtilities.EmptyCachePrefix, saReader);

            UnifiedJson.NeedsVariantComma = false;
            var annotatedVariant = annotationSource?.Annotate(vcfVariant);
            Assert.NotNull(annotatedVariant);

            var vcf = new VcfConversion();
            return vcf.Convert(vcfVariant, annotatedVariant).Split('\t')[vcfColumn];
        }

        public static void FieldEquals(ISupplementaryAnnotationReader saReader, string vcfLine,
            string expected, int vcfColumn)
        {
            var column = GetVcfColumn(saReader, vcfLine, vcfColumn);
            Assert.Equal(expected, column);
        }

        public static void FieldContains(ISupplementaryAnnotationReader saReader, string vcfLine,
            string expected, int vcfColumn)
        {
            var column = GetVcfColumn(saReader, vcfLine, vcfColumn);
            Assert.Contains(expected, column);
        }

        public static void FieldDoesNotContain(ISupplementaryAnnotationReader saReader,
            string vcfLine, string expected, int vcfColumn)
        {
            var column = GetVcfColumn(saReader, vcfLine, vcfColumn);
            Assert.DoesNotContain(expected, column);
        }

        internal static VariantFeature GetNextVariant(LiteVcfReader reader, ChromosomeRenamer renamer,
            bool isGatkGenomeVcf = false)
        {
            var vcfLine = reader.ReadLine();
            return GetVariant(vcfLine, renamer, isGatkGenomeVcf);
        }

        internal static VariantFeature GetVariant(string vcfLine, ChromosomeRenamer renamer,
            bool isGatkGenomeVcf = false)
        {
            if (string.IsNullOrEmpty(vcfLine)) return null;

            var fields = vcfLine.Split('\t');
            if (fields.Length < VcfCommon.MinNumColumns) return null;

            var variant = new VariantFeature(GetVcfVariant(vcfLine, isGatkGenomeVcf), renamer, new VID());
            variant.AssignAlternateAlleles();
            return variant;
        }

        internal static VcfVariant GetVcfVariant(string vcfLine, bool isGatkGenomeVcf = false)
        {
            var fields = vcfLine.Split('\t');
            return new VcfVariant(fields, vcfLine, isGatkGenomeVcf);
        }
    }
}
