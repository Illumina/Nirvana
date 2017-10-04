using System;
using System.Collections.Generic;
using System.Linq;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using Vcf.Info;
using Vcf.Sample;
using Vcf.VariantCreator;

namespace Vcf
{
    public static class VcfReaderUtils
    {
        private static readonly HashSet<string> NonInformativeAltAllele = new HashSet<string> { "<*>", "*", "<M>" };
        internal const string NonRefAltAllele = "<NON_REF>";

        internal static IPosition ParseVcfLine(string vcfLine, VariantFactory variantFactory, IDictionary<string, IChromosome> refNameToChromosome)
        {
            var vcfFields = vcfLine.Split('\t');
            var infoData = VcfInfoParser.Parse(vcfFields[VcfCommon.InfoIndex]);

            var id = vcfFields[VcfCommon.IdIndex];
            var chromosome = GetChromosome(refNameToChromosome, vcfFields[VcfCommon.ChromIndex]);
            int start = Convert.ToInt32(vcfFields[VcfCommon.PosIndex]);
            string refAllele = vcfFields[VcfCommon.RefIndex];
            int end = ExtractEnd(infoData, start, refAllele.Length);
            string[] altAlleles = vcfFields[VcfCommon.AltIndex].Split(',').Where(x => !NonInformativeAltAllele.Contains(x)).ToArray();
            if (altAlleles.Length == 0) return null;
            // only filter out "<NON_REF>" allele when there are more than one alternative alleles
            if (altAlleles.Length > 1) altAlleles = altAlleles.Where(x => x != NonRefAltAllele).ToArray();
            double? quality = vcfFields[VcfCommon.QualIndex].GetNullableValue<double>(Double.TryParse);
            string[] filters = vcfFields[VcfCommon.FilterIndex].Split(';');
            var samples = new SampleFieldExtractor(vcfFields, infoData.Depth).ExtractSamples();

            var sampleCopyNumber = GetSampleCopyNumbers(samples);

            var variants = variantFactory.CreateVariants(chromosome, id, start, end, refAllele, altAlleles, infoData, sampleCopyNumber);

            return new Position(chromosome, start, end, refAllele, altAlleles, quality, filters, variants, samples,
                infoData, vcfFields);
        }

        internal static int? GetSampleCopyNumbers(ISample[] samples)
        {
            if (samples == null) return null;
            int? copyNumber = null;

            foreach (var sample in samples)
            {
                if (sample.CopyNumber != null) copyNumber = sample.CopyNumber;
            }

            return copyNumber;
        }

        private static int ExtractEnd(IInfoData infoData, int start, int refAlleleLength)
        {
            if (infoData.End != null) return infoData.End.Value;
            return start + refAlleleLength - 1;
        }

        private static IChromosome GetChromosome(IDictionary<string, IChromosome> refNameToChromosome, string chrom)
        {
            return refNameToChromosome.ContainsKey(chrom) ? refNameToChromosome[chrom] : new EmptyChromosome(chrom);
        }
    }
}