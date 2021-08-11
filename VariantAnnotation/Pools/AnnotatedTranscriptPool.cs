using System.Collections.Generic;
using Microsoft.Extensions.ObjectPool;
using VariantAnnotation.AnnotatedPositions.Transcript;
using VariantAnnotation.Interface.AnnotatedPositions;

namespace VariantAnnotation.Pools
{
    public static class AnnotatedTranscriptPool
    {
        private static readonly ObjectPool<AnnotatedTranscript> Pool = new DefaultObjectPool<AnnotatedTranscript>(new DefaultPooledObjectPolicy<AnnotatedTranscript>(), 16);
        
        public static AnnotatedTranscript Get(ITranscript transcript, string referenceAminoAcids, string alternateAminoAcids,
            string referenceCodons, string alternateCodons, IMappedPosition mappedPosition, string hgvsCoding,
            string hgvsProtein, PredictionScore sift, PredictionScore polyphen,
            List<ConsequenceTag> consequences, bool? completeOverlap)
        {
            var annotatedTranscript =  Pool.Get();
            annotatedTranscript.Initialize(transcript, referenceAminoAcids, alternateAminoAcids, referenceCodons, alternateCodons, mappedPosition, 
                hgvsCoding, hgvsProtein, sift, polyphen, consequences, completeOverlap);
            
            return annotatedTranscript;
        }
        
        public static void Return(AnnotatedTranscript annotatedTranscript) => Pool.Return(annotatedTranscript);
        
    }
}