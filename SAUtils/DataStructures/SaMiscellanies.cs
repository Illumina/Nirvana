using SAUtils.Interface;

namespace SAUtils.DataStructures
{
    public sealed class SaMiscellanies :IInterimSaItem
    {
        public string KeyName { get; }
        public string Chromosome { get; }
        public int Position { get; }
        public string GlobalMajorAllele { get; }

        public SaMiscellanies(string keyName, string chr, int pos, string globalMajorAllele)
        {
            KeyName           = keyName;
            Chromosome        = chr;
            Position          = pos;
            GlobalMajorAllele = globalMajorAllele;
        }

        public int CompareTo(IInterimSaItem otherItem)
        {
            if (otherItem == null) return -1;
            return Chromosome.Equals(otherItem.Chromosome) ? Position.CompareTo(otherItem.Position) : string.CompareOrdinal(Chromosome, otherItem.Chromosome);
        }
    }
}