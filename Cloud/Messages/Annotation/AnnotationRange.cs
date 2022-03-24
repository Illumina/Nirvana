using System.Collections.Generic;
using Genome;

namespace Cloud.Messages.Annotation

{
    public sealed class AnnotationRange
    {
        public readonly AnnotationPosition Start;
        public readonly AnnotationPosition? End;

        public AnnotationRange(AnnotationPosition start, AnnotationPosition? end)
        {
            Start = start;
            End   = end;
        }

        public GenomicRange ToGenomicRange(IDictionary<string, Chromosome> refNameToChromosome)
        {
            var startGenomicPosition = new GenomicPosition(ReferenceNameUtilities.GetChromosome(refNameToChromosome, Start.Chromosome), Start.Position);

            GenomicPosition? endGenomicPosition = null;
            if (End != null) endGenomicPosition = new GenomicPosition(ReferenceNameUtilities.GetChromosome(refNameToChromosome, End.Value.Chromosome), End.Value.Position);

            return new GenomicRange(startGenomicPosition, endGenomicPosition);
        }
    }
}