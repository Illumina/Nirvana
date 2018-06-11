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

        public int CompareTo(IInterimSaItem other)
        {
            if (other == null) return -1;
            return Chromosome.Equals(other.Chromosome) ? Position.CompareTo(other.Position) : string.CompareOrdinal(Chromosome, other.Chromosome);
        }
    }
}