using System.Collections.Generic;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IAnnotatedTranscript : IJsonSerializer
    {
        ITranscript Transcript { get; }
        string ReferenceAminoAcids { get; }
        string AlternateAminoAcids { get; }
        string ReferenceCodons { get; }
        string AlternateCodons { get; }
        IMappedPosition MappedPosition { get; }
        string HgvsCoding { get; }
        string HgvsProtein { get; }
        PredictionScore Sift { get; }
        PredictionScore PolyPhen { get; }
        IEnumerable<ConsequenceTag> Consequences { get; }
        IGeneFusionAnnotation GeneFusionAnnotation { get; }
        IList<IPluginData> PluginData { get; }
    }
}