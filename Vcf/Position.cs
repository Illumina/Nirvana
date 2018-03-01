using System.Linq;
using System.Reflection.Metadata.Ecma335;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Sequence;
using Vcf.Info;
using Vcf.Sample;
using Vcf.VariantCreator;

namespace Vcf
{
    public sealed class Position : IPosition
    {
        public IChromosome Chromosome { get; }
        public int Start { get; }
        public int End { get; }        
        public string RefAllele { get; }
        public string[] AltAlleles { get; }
        public double? Quality { get; }
        public string[] Filters { get; }
        public IVariant[] Variants { get; }
        public ISample[] Samples { get; }
        public IInfoData InfoData { get; }
        public string[] VcfFields { get; }
        public bool[] IsDecomposed { get; }
        public bool IsRecomposed { get; }

        public Position(IChromosome chromosome, int start, int end, string refAllele, string[] altAlleles,
            double? quality, string[] filters, IVariant[] variants, ISample[] samples, IInfoData infoData,
            string[] vcfFields, bool[] isDecomposed, bool isRecomposed)
        {
            Chromosome = chromosome;
            Start      = start;
            End        = end;
            RefAllele  = refAllele;
            AltAlleles = altAlleles;
            Quality    = quality;
            Filters    = filters;
            Variants   = variants;
            Samples    = samples;
            InfoData   = infoData;
            VcfFields  = vcfFields;
            IsDecomposed = isDecomposed;
            IsRecomposed = isRecomposed;
        }

        public static Position CreatFromSimplePosition(ISimplePosition simplePosition, VariantFactory variantFactory)
        {
            if (simplePosition == null) return null;
            var vcfFields = simplePosition.VcfFields;
            var infoData = VcfInfoParser.Parse(vcfFields[VcfCommon.InfoIndex]);
            var id = vcfFields[VcfCommon.IdIndex];
            int end = ExtractEnd(infoData, simplePosition.Start, simplePosition.RefAllele.Length); // re-calculate the end by checking INFO field
            string[] altAlleles = vcfFields[VcfCommon.AltIndex].Split(',').ToArray();
            double? quality = vcfFields[VcfCommon.QualIndex].GetNullableValue<double>(double.TryParse);
            string[] filters = vcfFields[VcfCommon.FilterIndex].Split(';');
            var samples = new SampleFieldExtractor(vcfFields, infoData.Depth).ExtractSamples();

            var variants = variantFactory.CreateVariants(simplePosition.Chromosome, id, simplePosition.Start, end, simplePosition.RefAllele, altAlleles, infoData, simplePosition.IsDecomposed, simplePosition.IsRecomposed);

            return new Position(simplePosition.Chromosome, simplePosition.Start, end, simplePosition.RefAllele, altAlleles, quality, filters, variants, samples,
                infoData, vcfFields, simplePosition.IsDecomposed, simplePosition.IsRecomposed);
        }

        private static int ExtractEnd(IInfoData infoData, int start, int refAlleleLength)
        {
            if (infoData.End != null) return infoData.End.Value;
            return start + refAlleleLength - 1;
        }

        
    }
}
