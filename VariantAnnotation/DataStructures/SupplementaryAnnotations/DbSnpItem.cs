namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public class DbSnpItem : SupplementaryDataItem
    {
        internal long RsId { get; }
        private string ReferenceAllele { get; }
        private string AlternateAllele { get; }
        private double RefAlleleFreq { get; }
        internal double AltAlleleFreq { get; }
        private readonly string _infoField;// stores the original dbSnp entry line that was parsed. will use it for ToString

        public DbSnpItem(string chromosome,
            int position,
            long rsId,
            string refAllele,
            double refAlleleFreq,
            string alternateAllele,
            double altAlleleFreq,
            string infoField = null)
        {
            Chromosome = chromosome;
            Start = position;
            RsId = rsId;
            ReferenceAllele = refAllele;
            AlternateAllele = alternateAllele;
            RefAlleleFreq = refAlleleFreq;
            AltAlleleFreq = altAlleleFreq;
            _infoField = infoField;
        }

        public override SupplementaryDataItem SetSupplementaryAnnotations(SupplementaryAnnotation sa, string refBases = null)
        {
            // check if the ref allele matches the refBases as a prefix
            if (!SupplementaryAnnotation.ValidateRefAllele(ReferenceAllele, refBases))
            {
                return null; //the ref allele for this entry did not match the reference bases.
            }

            int newStart = Start;
            var newAlleles = SupplementaryAnnotation.GetReducedAlleles(ReferenceAllele, AlternateAllele, ref newStart);

            var newRefAllele = newAlleles.Item1;
            var newAltAllele = newAlleles.Item2;

            if (newRefAllele != ReferenceAllele)
            {
                return new DbSnpItem(Chromosome,
                    newStart,
                    RsId,
                    newRefAllele,
                    RefAlleleFreq,
                    newAltAllele,
                    AltAlleleFreq,
                    _infoField); // we need to keep the vcfline for the sake of conflict resolution

            }
            // it's a SNV or MNV at this position
            SetSaFields(sa, newAltAllele);

            return null;
        }

        public override SupplementaryInterval GetSupplementaryInterval()
        {
            throw new System.NotImplementedException();
        }

        private void SetSaFields(SupplementaryAnnotation sa, string newAltAllele)
        {

            if (!sa.AlleleSpecificAnnotations.ContainsKey(newAltAllele))
                sa.AlleleSpecificAnnotations[newAltAllele] = new SupplementaryAnnotation.AlleleSpecificAnnotation();

            var asa = sa.AlleleSpecificAnnotations[newAltAllele];

            if (!asa.DbSnp.Contains(RsId)) asa.DbSnp.Add(RsId);

            if (!RefAlleleFreq.Equals(double.MinValue))
            {
                sa.RefAllele = ReferenceAllele;
                sa.RefAlleleFreq = RefAlleleFreq;
            }
            if (!AltAlleleFreq.Equals(double.MinValue)) asa.AltAlleleFreq = AltAlleleFreq;
        }

        public override bool Equals(object other)
        {
            // If parameter is null return false.

            var otherItem = other as DbSnpItem;
            if (otherItem == null) return false;

            // Return true if the fields match:
            return string.Equals(Chromosome, otherItem.Chromosome)
                   && Start == otherItem.Start
                   && RsId == otherItem.RsId
                   && string.Equals(ReferenceAllele, otherItem.ReferenceAllele)
                   && AlternateAllele.Equals(otherItem.AlternateAllele);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = RsId.GetHashCode();
                hashCode = (hashCode * 397) ^ (ReferenceAllele?.GetHashCode() ?? 0);
                hashCode = (hashCode * 397) ^ (AlternateAllele?.GetHashCode() ?? 0);
                return hashCode;
            }
        }
    }
}
