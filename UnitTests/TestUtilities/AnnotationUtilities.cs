using System.Collections.Generic;
using Genome;
using Nirvana;
using VariantAnnotation;
using VariantAnnotation.Interface.AnnotatedPositions;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;
using Vcf;
using Vcf.VariantCreator;

namespace UnitTests.TestUtilities
{
    public static class AnnotationUtilities
	{
        internal static IAnnotatedPosition GetAnnotatedPosition(string cacheFilePrefix, List<string> saPaths,
            string vcfLine)
        {
            List<(string dataFile, string indexFile)> dataAndIndexPaths = null;
            if(saPaths!=null)
            {
                dataAndIndexPaths = new List<(string dataFile, string indexFile)>();
                foreach (var saPath in saPaths)
                {
                    dataAndIndexPaths.AddRange(ProviderUtilities.GetSaDataAndIndexPaths(saPath));
                }
            }

            var refMinorProvider  = ProviderUtilities.GetRefMinorProvider(dataAndIndexPaths);
            var (annotator, sequenceProvider)   = GetAnnotatorAndSequenceProvider(cacheFilePrefix, saPaths);

            var variantFactory    = new VariantFactory(sequenceProvider);
            var position          = ParseVcfLine(vcfLine, refMinorProvider, variantFactory, sequenceProvider.RefNameToChromosome);
            var annotatedPosition = annotator.Annotate(position);

            return annotatedPosition;
        }

	    internal static IPosition ParseVcfLine(string vcfLine, IRefMinorProvider refMinorProvider, VariantFactory variantFactory, IDictionary<string, IChromosome> refNameToChromosome)
	    {
	        var simplePosition = GetSimplePosition(vcfLine, refNameToChromosome);
	        return Position.ToPosition(simplePosition, refMinorProvider, variantFactory);
	    }

        internal static SimplePosition GetSimplePosition(string vcfLine,
            IDictionary<string, IChromosome> refNameToChromosome) =>
            SimplePosition.GetSimplePosition(vcfLine, new NullVcfFilter(), refNameToChromosome);

        private static (Annotator Annotator, ISequenceProvider SequenceProvider) GetAnnotatorAndSequenceProvider(string cacheFilePrefix, List<string> saPaths)
        {
            List<(string dataFile, string indexFile)> dataAndIndexPaths = null;
            if (saPaths != null)
            {
                dataAndIndexPaths = new List<(string dataFile, string indexFile)>();
                foreach (var saPath in saPaths)
                {
                    dataAndIndexPaths.AddRange(ProviderUtilities.GetSaDataAndIndexPaths(saPath));
                }

            }

            var sequenceFilePath                 = cacheFilePrefix + ".bases";
            var sequenceProvider                 = ProviderUtilities.GetSequenceProvider(sequenceFilePath);
            var transcriptAnnotationProvider     = ProviderUtilities.GetTranscriptAnnotationProvider(cacheFilePrefix, sequenceProvider);
            var saProvider                       = ProviderUtilities.GetNsaProvider(dataAndIndexPaths);
            var conservationProvider             = ProviderUtilities.GetConservationProvider(dataAndIndexPaths);

            var annotator = new Annotator(transcriptAnnotationProvider, sequenceProvider, saProvider, conservationProvider, null);
            return (annotator,sequenceProvider);
        }
    }
}