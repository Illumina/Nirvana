using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.DataStructures;
using VariantAnnotation.FileHandling;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.Interface;
using Xunit;

namespace UnitTests.Utilities
{
    internal class VcfUtilities : RandomFileBase
    {
        internal static VariantFeature GetVariantFeature(string vcfLine, bool isGatkGenomeVcf = false)
        {
            return new VariantFeature(GetVariant(vcfLine, isGatkGenomeVcf));
        }

        internal static VcfVariant GetVariant(string vcfLine, bool isGatkGenomeVcf = false)
        {
            var fields = vcfLine.Split('\t');
            return new VcfVariant(fields, vcfLine, isGatkGenomeVcf);
        }

        internal void FieldEquals(string vcfLine, string supplementaryFile, string expected, int vcfColumn)
        {
            var observed = GetObservedField(vcfLine, supplementaryFile, vcfColumn);
            Assert.Equal(expected, observed);
        }

        internal void FieldContains(string vcfLine, string supplementaryFile, string expected, int vcfColumn)
        {
            var observed = GetObservedField(vcfLine, supplementaryFile, vcfColumn);
            Assert.Contains(expected, observed);
        }

        internal void FieldDoesNotContain(string vcfLine, string supplementaryFile, string expected, int vcfColumn)
        {
            var observed = GetObservedField(vcfLine, supplementaryFile, vcfColumn);
            Assert.DoesNotContain(expected, observed);
        }

        /// <summary>
        /// given a key in the info field, this method will return the value if the key exists. Returns
        /// null otherwise.
        /// </summary>
        internal static string FindInfoValue(string key, string infoField)
        {
            return (from kvpString in infoField.Split(';')
                    select kvpString.Split('=')
                into keyValuePair
                    where keyValuePair[0] == key
                    select keyValuePair[1]).FirstOrDefault();
        }

        internal string GetObservedField(string vcfLine, string supplementaryFile, int vcfColumn)
        {
            var annotatedVariant = DataUtilities.GetVariant(null, supplementaryFile, vcfLine);
            Assert.NotNull(annotatedVariant);

            var observedVcfLine = WriteAndGetFirstVcfLine(vcfLine, annotatedVariant);
            return observedVcfLine.Split('\t')[vcfColumn];
        }

        /// <summary>
        /// writes an annotated variant to a VCF file and then returns the first line
        /// </summary>
        internal string WriteAndGetFirstVcfLine(string vcfLine, IAnnotatedVariant annotatedVariant)
        {
            var randomPath  = GetRandomPath();
            var headerLines = new List<string> { "#CHROM\tPOS\tID\tREF\tALT\tQUAL\tFILTER\tINFO" };

            using (var writer = new LiteVcfWriter(randomPath, headerLines, string.Empty, new List<DataSourceVersion>()))
            {
                Write(writer, vcfLine, annotatedVariant);
            }

            string observedVcfLine;
            using (var reader = new LiteVcfReader(randomPath))
            {
                observedVcfLine = reader.ReadLine();
            }

            return observedVcfLine;
        }

        private static void Write(LiteVcfWriter writer, string vcfLine, IAnnotatedVariant annotatedVariant)
        {
            var fields = vcfLine.Split('\t');
            if (fields.Length < VcfCommon.MinNumColumns) return;
            var vcfVariant = new VcfVariant(fields, vcfLine, false);
            writer.Write(vcfVariant, annotatedVariant);
        }
    }
}
