namespace CacheUtils.GFF
{
    public sealed class RequiredFields : IRequiredFields
    {
        public string UcscName { get; }
        public string Source { get; }
        public bool OnReverseStrand { get; }

        public RequiredFields(string ucscName, string source, bool onReverseStrand)
        {
            UcscName        = ucscName;
            Source          = source;
            OnReverseStrand = onReverseStrand;
        }
    }
}
