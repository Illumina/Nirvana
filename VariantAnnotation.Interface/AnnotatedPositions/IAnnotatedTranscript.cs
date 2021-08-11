using System.Collections.Generic;
using VariantAnnotation.Interface.IO;

namespace VariantAnnotation.Interface.AnnotatedPositions
{
    public interface IAnnotatedTranscript : IJsonSerializer
    {
        ITranscript          Transcript          { get; }
        string               ReferenceAminoAcids { get; }
        string               AlternateAminoAcids { get; }
        string               ReferenceCodons     { get; }
        string               AlternateCodons     { get; }
        IMappedPosition      MappedPosition      { get; }
        string               HgvsCoding          { get; }
        string               HgvsProtein         { get; }
        PredictionScore      Sift                { get; }
        PredictionScore      PolyPhen            { get; }
        List<ConsequenceTag> Consequences        { get; }
        bool?                CompleteOverlap     { get; }
        List<double>         ConservationScores  { get; set; }

        void AddGeneFusions(IAnnotatedGeneFusion[] geneFusions);
        void AddGeneFusionPairs(HashSet<IGeneFusionPair> geneKeys);

        public void Initialize(ITranscript transcript, string referenceAminoAcids, string alternateAminoAcids,
            string referenceCodons, string alternateCodons, IMappedPosition mappedPosition, string hgvsCoding,
            string hgvsProtein, PredictionScore sift, PredictionScore polyphen,
            List<ConsequenceTag> consequences, bool? completeOverlap);
    }
}