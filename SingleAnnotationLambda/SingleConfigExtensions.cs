using System.IO;
using Cloud.Messages.Single;
using ErrorHandling.Exceptions;
using Genome;
using OptimizedCore;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Vcf;
using Vcf.VariantCreator;

namespace SingleAnnotationLambda
{
    public static class SingleConfigExtensions
    {
        public static void Validate(this SingleConfig config)
        {
            if (string.IsNullOrEmpty(config.id)) throw new UserErrorException("Please specify the id.");
            if (string.IsNullOrEmpty(config.genomeAssembly)) throw new UserErrorException("Please specify the genome assembly.");
            if (config.variant == null) throw new UserErrorException("Please specify the variant (chromosome, position, reference allele, and alt alleles).");
            config.ValidateSupplementaryAnnotations();
            config.ValidateVepVersion();
            config.variant?.Validate();
        }

        private static void ValidateSupplementaryAnnotations(this SingleConfig config)
        {
            if (string.IsNullOrEmpty(config.supplementaryAnnotations)) return;
            if (SupplementaryAnnotationUtilities.IsValueSupported(config.supplementaryAnnotations)) return;
            throw new UserErrorException($"An invalid supplementary annotation value ({config.supplementaryAnnotations}) was specified. Please choose one of the following values: {SupplementaryAnnotationUtilities.GetSupportedValues()}");
        }

        private static void ValidateVepVersion(this SingleConfig config)
        {
            if (config.vepVersion == 0) config.vepVersion = CacheUtilities.DefaultVepVersion;
            if (CacheUtilities.IsVepVersionSupported(config.vepVersion)) return;
            throw new UserErrorException($"An invalid VEP version ({config.vepVersion}) was specified. Please choose one of the following versions: {CacheUtilities.GetSupportedVersions()}");
        }

        public static (IPosition, string[]) GetPositionAndSampleNames(this SingleConfig config, ISequenceProvider sequenceProvider,
            IRefMinorProvider refMinorProvider) => (ToPosition(config.variant.GetVcfFields(), sequenceProvider, refMinorProvider), config.variant.sampleNames);

        private static IPosition ToPosition(string[] vcfFields, ISequenceProvider sequenceProvider, IRefMinorProvider refMinorProvider)
        {
            IChromosome chromosome = ReferenceNameUtilities.GetChromosome(sequenceProvider.RefNameToChromosome, vcfFields[VcfCommon.ChromIndex]);

            sequenceProvider.LoadChromosome(chromosome);

            (int start, bool foundError) = vcfFields[VcfCommon.PosIndex].OptimizedParseInt32();
            if (foundError) throw new InvalidDataException($"Unable to convert the VCF position to an integer: {vcfFields[VcfCommon.PosIndex]}");

            SimplePosition simplePosition = SimplePosition.GetSimplePosition(chromosome, start, vcfFields, new NullVcfFilter());
            var variantFactory = new VariantFactory(sequenceProvider.Sequence, sequenceProvider.RefNameToChromosome, false);
            return Position.ToPosition(simplePosition, refMinorProvider, sequenceProvider, variantFactory);
        }
    }
}
