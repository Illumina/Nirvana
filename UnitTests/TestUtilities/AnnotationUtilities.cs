using System.Collections.Generic;
using Genome;
using MitoHeteroplasmy;
using Nirvana;
using OptimizedCore;
using VariantAnnotation;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.IO;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Vcf;
using Vcf.VariantCreator;

namespace UnitTests.TestUtilities
{
    public static class AnnotationUtilities
	{
        internal static IAnnotatedPosition GetAnnotatedPosition(string cacheFilePrefix, List<string> saPaths, IMitoHeteroplasmyProvider mitoHeteroplasmyProvider,
            string vcfLine)
        {
            var annotationFiles = new AnnotationFiles();
            saPaths?.ForEach(x => annotationFiles.AddFiles(x));

            var refMinorProvider  = ProviderUtilities.GetRefMinorProvider(annotationFiles);
            var (annotator, sequenceProvider)   = GetAnnotatorAndSequenceProvider(cacheFilePrefix, saPaths);

            var variantFactory    = new VariantFactory(sequenceProvider.Sequence, new VariantId());
            var position          = ParseVcfLine(vcfLine, refMinorProvider, sequenceProvider, mitoHeteroplasmyProvider, variantFactory);
            var annotatedPosition = annotator.Annotate(position);

            return annotatedPosition;
        }

	    internal static IPosition ParseVcfLine(string vcfLine, IRefMinorProvider refMinorProvider, ISequenceProvider sequenceProvider, IMitoHeteroplasmyProvider mitoHeteroplasmyProvider, VariantFactory variantFactory)
	    {
	        var simplePosition = GetSimplePosition(vcfLine, sequenceProvider.RefNameToChromosome);
	        return Position.ToPosition(simplePosition, refMinorProvider, sequenceProvider, mitoHeteroplasmyProvider, variantFactory);
	    }

        internal static SimplePosition GetSimplePosition(string vcfLine,
            IDictionary<string, IChromosome> refNameToChromosome)
        {
            string[] vcfFields = vcfLine.OptimizedSplit('\t');
            var chromosome     = ReferenceNameUtilities.GetChromosome(refNameToChromosome, vcfFields[VcfCommon.ChromIndex]);
            int position       = int.Parse(vcfFields[VcfCommon.PosIndex]);

            return SimplePosition.GetSimplePosition(chromosome, position, vcfFields, new NullVcfFilter());
        }

        private static (Annotator Annotator, ISequenceProvider SequenceProvider) GetAnnotatorAndSequenceProvider(string cacheFilePrefix, List<string> saPaths)
        {
            var annotationFiles = new AnnotationFiles();
            saPaths?.ForEach(x => annotationFiles.AddFiles(x));

            string sequenceFilePath          = cacheFilePrefix + ".bases";
            var sequenceProvider             = ProviderUtilities.GetSequenceProvider(sequenceFilePath);
            var transcriptAnnotationProvider = ProviderUtilities.GetTranscriptAnnotationProvider(cacheFilePrefix, sequenceProvider, null);
            var saProvider                   = ProviderUtilities.GetNsaProvider(annotationFiles);
            var conservationProvider         = ProviderUtilities.GetConservationProvider(annotationFiles);

            var annotator = new Annotator(transcriptAnnotationProvider, sequenceProvider, saProvider,
                conservationProvider, null, null);
            return (annotator,sequenceProvider);
        }
    }
}