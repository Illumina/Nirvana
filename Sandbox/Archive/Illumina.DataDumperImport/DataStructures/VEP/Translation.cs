namespace Illumina.DataDumperImport.DataStructures.VEP
{
    public sealed class Translation
    {
        // public TranslationAdaptor Adaptor; // skip
        public Exon EndExon          = null;
        public Exon StartExon        = null;
        public Transcript Transcript = null;

        public int End;
        public int Start;
        public byte Version;
    }
}
