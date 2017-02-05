using System.Linq;
using VariantAnnotation;
using VariantAnnotation.AnnotationSources;
using VariantAnnotation.DataStructures.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.CustomInterval;
using VariantAnnotation.FileHandling.Phylop;
using VariantAnnotation.FileHandling.SupplementaryAnnotations;
using VariantAnnotation.FileHandling.TranscriptCache;
using VariantAnnotation.Interface;
using VariantAnnotation.Loftee;
using VariantAnnotation.Utilities;

namespace Nirvana
{
    public class AnnotationSourceFactory : IAnnotationSourceFactory
    {
        public IAnnotationSource CreateAnnotationSource(IAnnotatorInfo annotatorInfo, IAnnotatorPaths annotatorPaths)
        {
            var conservationScoreReader = new PhylopReader(annotatorPaths.SupplementaryAnnotation);

            var transcriptStream = FileUtilities.GetReadStream(CacheConstants.TranscriptPath(annotatorPaths.CachePrefix));
            var siftStream       = FileUtilities.GetReadStream(CacheConstants.SiftPath(annotatorPaths.CachePrefix));
            var polyPhenStream   = FileUtilities.GetReadStream(CacheConstants.PolyPhenPath(annotatorPaths.CachePrefix));
            var referenceStream  = FileUtilities.GetReadStream(annotatorPaths.CompressedReference);

            var streams = new AnnotationSourceStreams(transcriptStream, siftStream, polyPhenStream, referenceStream);

            var caProvider = annotatorPaths.CustomAnnotation.Any()          ? new CustomAnnotationProvider(annotatorPaths.CustomAnnotation)               : null;
            var ciProvider = annotatorPaths.CustomIntervals.Any()           ? new CustomIntervalProvider(annotatorPaths.CustomIntervals)                  : null;
            var saProvider = annotatorPaths.SupplementaryAnnotation != null ? new SupplementaryAnnotationProvider(annotatorPaths.SupplementaryAnnotation) : null;

			//adding the saPath because OMIM needs it
            var annotationSource = new NirvanaAnnotationSource(streams, saProvider, conservationScoreReader, caProvider, ciProvider, annotatorPaths.SupplementaryAnnotation);

            if (annotatorInfo.BooleanArguments.Contains(AnnotatorInfoCommon.ReferenceNoCall))
                annotationSource.EnableReferenceNoCalls(annotatorInfo.BooleanArguments.Contains(AnnotatorInfoCommon.TranscriptOnlyRefNoCall));

            if (annotatorInfo.BooleanArguments.Contains(AnnotatorInfoCommon.EnableMitochondrialAnnotation))
                annotationSource.EnableMitochondrialAnnotation();

            if (annotatorInfo.BooleanArguments.Contains(AnnotatorInfoCommon.ReportAllSvOverlappingTranscripts))
                annotationSource.EnableReportAllSvOverlappingTranscripts();

            if (annotatorInfo.BooleanArguments.Contains(AnnotatorInfoCommon.EnableLoftee))
                annotationSource.AddPlugin(new Loftee());

            return annotationSource;
        }
    }
}