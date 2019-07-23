using System.Collections.Generic;
using Genome;

namespace Cloud

{
    public sealed class AnnotationRange
    {
        public AnnotationPosition Start { get; }
        public AnnotationPosition? End { get; }

        public AnnotationRange(AnnotationPosition start, AnnotationPosition? end)
        {
            Start = start;
            End = end;
        }

        public GenomicRange ToGenomicRange(IDictionary<string, IChromosome> refNameToChromosome)
        {
            var startGenomicPosition = new GenomicPosition(ReferenceNameUtilities.GetChromosome(refNameToChromosome, Start.Chromosome), Start.Position);

            GenomicPosition? endGenomicPosition = null;
            if (End != null)
                endGenomicPosition = new GenomicPosition(ReferenceNameUtilities.GetChromosome(refNameToChromosome, End.Value.Chromosome), End.Value.Position);

            return new GenomicRange(startGenomicPosition, endGenomicPosition);
        }

        public override string ToString() => $"{Start.ToString()}-{End?.ToString()}";
    }
}