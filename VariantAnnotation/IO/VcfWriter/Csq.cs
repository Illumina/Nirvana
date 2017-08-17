namespace VariantAnnotation.IO.VcfWriter
{
    public static class CsqCommon
    {
        public const string TranscriptFeatureType = "Transcript";
        public const string RegulatoryFeatureType = "RegulatoryFeature";
    }

    /// <summary>
    /// The annoying bit about CSQ fields is that the order changes depending on which
    /// parameters have been passed to VEP. As a result, we need to keep all of the key
    /// value pairs in a dictionary.
    /// </summary>
    public sealed class CsqEntry
    {
        public string Allele;
        public string Consequence;
        public string Feature;
        public string FeatureType;
        public string Symbol;
    }
}