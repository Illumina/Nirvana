using System.Collections.Generic;
using Nirvana;
using VariantAnnotation;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Sequence;
using Vcf;
using Vcf.VariantCreator;

namespace UnitTests.TestUtilities
{
    public static class AnnotationUtilities
	{
	    internal static IAnnotatedPosition GetAnnotatedPosition(string cacheFilePrefix, List<string> saPaths,
            string vcfLine, bool enableVerboseTranscripts)
	    {
	        var refMinorProvider = ProviderUtilities.GetRefMinorProvider(saPaths);
	        var annotatorAndRef = GetAnnotatorAndReferenceDict(cacheFilePrefix, saPaths);

	        var annotator = annotatorAndRef.Annotator;
            var refNames = annotatorAndRef.RefNames;
            var variantFactory = new VariantFactory(refNames,refMinorProvider,enableVerboseTranscripts);

	        var position = VcfReaderUtils.ParseVcfLine(vcfLine,variantFactory,refNames);

	        var annotatedPosition = annotator.Annotate(position);

	        return annotatedPosition;
	    }

        private static (Annotator Annotator, IDictionary<string,IChromosome> RefNames) GetAnnotatorAndReferenceDict(string cacheFilePrefix, List<string> saPaths)
        {
            var sequenceFilePath             = cacheFilePrefix + ".bases";
            var sequenceProvider             = ProviderUtilities.GetSequenceProvider(sequenceFilePath);
            var refNames                     = sequenceProvider.RefNameToChromosome;
            var transcriptAnnotationProvider = ProviderUtilities.GetTranscriptAnnotationProvider(cacheFilePrefix, sequenceProvider);
            var saProvider                   = ProviderUtilities.GetSaProvider(saPaths);
            var conservationProvider         = ProviderUtilities.GetConservationProvider(saPaths);

            var annotator = new Annotator(transcriptAnnotationProvider, sequenceProvider, saProvider, conservationProvider, null);
            return (annotator,refNames);
        }
    }
}