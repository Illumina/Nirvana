namespace SAUtils.DbSnpRemapper
{
    public struct RefAltPair
    {
        public string RefAllele;
        public string AltAllele;

        public RefAltPair(string refAllele, string altAllele)
        {
            RefAllele = refAllele;
            AltAllele = altAllele;
        }
    }
}