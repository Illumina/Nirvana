using System;
using System.Collections.Generic;
using VariantAnnotation.Interface.GeneAnnotation;
using VariantAnnotation.Interface.Phantom;
using VariantAnnotation.Interface.Plugins;
using VariantAnnotation.Interface.Positions;
using VariantAnnotation.Interface.Providers;

namespace VariantAnnotation.Interface
{
    public interface IAnnotationResources:IDisposable
    {
        ISequenceProvider SequenceProvider { get; }
        ITranscriptAnnotationProvider TranscriptAnnotationProvider { get; }
        IAnnotationProvider SaProvider { get; }
        IAnnotationProvider ConservationProvider { get; }
        IRefMinorProvider RefMinorProvider { get; }
        IGeneAnnotationProvider GeneAnnotationProvider { get; }
        IPlugin[] Plugins { get; }
        IAnnotator Annotator { get; }
        IRecomposer Recomposer { get; }
        List<IDataSourceVersion> DataSourceVersions { get; }
        string VepDataVersion { get; }
        string AnnotatorVersionTag { get; set; }
        bool ForceMitochondrialAnnotation { get; }
        long InputStartVirtualPosition { get; }
        void SingleVariantPreLoad(IPosition position);
    }
}