namespace Cloud.Messages.Annotation
{
    public struct AnnotationPosition
    {
        public readonly string Chromosome;
        public readonly int Position;

        public AnnotationPosition(string chromosome, int position)
        {
            Chromosome = chromosome;
            Position = position;
        }
    }
}