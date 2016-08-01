using System.Linq;
using VariantAnnotation;
using VariantAnnotation.AnnotationSources;
using VariantAnnotation.Interface;

namespace Nirvana
{
    public class AnnotationSourceFactory : IAnnotationSourceFactory
    {
        public IAnnotationSource CreateAnnotationSource(IAnnotatorInfo annotatorInfo, IAnnotatorPaths annotatorPaths)
        {
            var annotationSource = new NirvanaAnnotationSource(annotatorPaths);

            if (annotatorInfo.BooleanArguments.Contains(AnnotatorInfoCommon.ReferenceNoCall))
                annotationSource.EnableReferenceNoCalls(annotatorInfo.BooleanArguments.Contains(AnnotatorInfoCommon.TranscriptOnlyRefNoCall));

            if (annotatorInfo.BooleanArguments.Contains(AnnotatorInfoCommon.EnableMitochondrialAnnotation))
                annotationSource.EnableMitochondrialAnnotation();

            return annotationSource;
        }
    }
}