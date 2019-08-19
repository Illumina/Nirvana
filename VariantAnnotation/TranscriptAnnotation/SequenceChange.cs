namespace VariantAnnotation.TranscriptAnnotation
{
    public struct SequenceChange
    {
        public readonly string Reference;
        public readonly string Alternate;

        public SequenceChange(string reference, string alternate)
        {
            Reference = reference;
            Alternate = alternate;
        }
    }
}