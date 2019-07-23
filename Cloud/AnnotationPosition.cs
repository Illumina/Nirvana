namespace Cloud
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

        public override string ToString() => $"{Chromosome}:{Position}";
    }
}