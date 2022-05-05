using System.Collections.Generic;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Variants;
using Vcf.Info;
using Vcf.Sample;
using Vcf.VariantCreator;

namespace Vcf
{
    public sealed class Position : IPosition
    {
        public Chromosome    Chromosome           { get; private set;}
        public int            Start                { get; private set;}
        public int            End                  { get; private set;}        
        public string         RefAllele            { get; private set;}
        public string[]       AltAlleles           { get; private set;}
        public double?        Quality              { get; private set;}
        public string[]       Filters              { get; private set;}
        public IVariant[]     Variants             { get; private set;}
        public ISample[]      Samples              { get; private set;}
        public IInfoData      InfoData             { get; private set;}
        public bool           HasStructuralVariant { get; private set;}
        public bool           HasShortTandemRepeat { get; private set;}
        public string[]       VcfFields            { get; private set;}
        public bool[]         IsDecomposed         { get; private set;}
        public bool           IsRecomposed         { get; private set;}
        public string[]       Vids                 { get; private set;}
        public List<string>[] LinkedVids           { get; private set;}
        
        public void Initialize(Chromosome chromosome, int start, int end, string refAllele, string[] altAlleles,
            double? quality, string[] filters, IVariant[] variants, ISample[] samples, IInfoData infoData,
            string[] vcfFields, bool[] isDecomposed, bool isRecomposed)
        {
            Chromosome   = chromosome;
            Start        = start;
            End          = end;
            RefAllele    = refAllele;
            AltAlleles   = altAlleles;
            Quality      = quality;
            Filters      = filters;
            Variants     = variants;
            Samples      = samples;
            InfoData     = infoData;
            VcfFields    = vcfFields;
            IsDecomposed = isDecomposed;
            IsRecomposed = isRecomposed;

            (HasStructuralVariant, HasShortTandemRepeat) = CheckVariants(variants);
            Vids                                         = null;
            LinkedVids                                   = null;
        }

        private static (bool HasStructuralVariant, bool HasShortTandemRepeat) CheckVariants(IVariant[] variants)
        {
            if (variants == null) return (false, false);

            var hasStructuralVariant = false;
            var hasShortTandemRepeat = false;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var variant in variants)
            {
                if (variant.IsStructuralVariant) hasStructuralVariant = true;
                if (variant.Type == VariantType.short_tandem_repeat_variation) hasShortTandemRepeat = true;
            }

            return (hasStructuralVariant, hasShortTandemRepeat);
        }

        public static IPosition ToPosition(ISimplePosition simplePosition, IRefMinorProvider refMinorProvider, ISequenceProvider sequenceProvider, 
            IMitoHeteroplasmyProvider mitoHeteroplasmyProvider, VariantFactory variantFactory, bool enableDq = false, 
            HashSet<string> customInfoKeys=null)
        {
            if (simplePosition == null) return null;

            sequenceProvider.LoadChromosome(simplePosition.Chromosome);

            string[] vcfFields  = simplePosition.VcfFields;
            string[] altAlleles = vcfFields[VcfCommon.AltIndex].OptimizedSplit(',');
            bool isReference    = altAlleles.Length == 1 && VcfCommon.ReferenceAltAllele.Contains(altAlleles[0]);

            string globalMajorAllele = isReference
                ? refMinorProvider?.GetGlobalMajorAllele(simplePosition.Chromosome, simplePosition.Start)
                : null;

            bool isRefMinor = isReference && globalMajorAllele != null;
            
            if (isReference && !isRefMinor) return GetReferencePosition(simplePosition);

            var       infoData              = VcfInfoParser.Parse(vcfFields[VcfCommon.InfoIndex],customInfoKeys);
            int       end                   = ExtractEnd(infoData, simplePosition.Start, simplePosition.RefAllele.Length);
            double?   quality               = vcfFields[VcfCommon.QualIndex].GetNullableValue<double>(double.TryParse);
            string[]  filters               = vcfFields[VcfCommon.FilterIndex].OptimizedSplit(';');
            
            IVariant[] variants = variantFactory.CreateVariants(simplePosition.Chromosome, simplePosition.Start, end,
                simplePosition.RefAllele, altAlleles, infoData, simplePosition.IsDecomposed,
                simplePosition.IsRecomposed, simplePosition.LinkedVids, globalMajorAllele);

            ISample[] samples = vcfFields.ToSamples(variantFactory.FormatIndices, simplePosition, variants, mitoHeteroplasmyProvider, 
                enableDq);

            return PositionPool.Get(simplePosition.Chromosome, simplePosition.Start, end, simplePosition.RefAllele,
                altAlleles, quality, filters, variants, samples, infoData, vcfFields, simplePosition.IsDecomposed,
                simplePosition.IsRecomposed);
        }
        
        private static IPosition GetReferencePosition(ISimplePosition simplePosition) =>
            PositionPool.Get(simplePosition.Chromosome, simplePosition.Start, simplePosition.Start,
                simplePosition.RefAllele, simplePosition.AltAlleles, null, null, null, null, null,
                simplePosition.VcfFields, simplePosition.IsDecomposed, simplePosition.IsRecomposed);

        private static int ExtractEnd(IInfoData infoData, int start, int refAlleleLength)
        {
            if (infoData.End != null) return infoData.End.Value;
            return start + refAlleleLength - 1;
        }
    }
}
