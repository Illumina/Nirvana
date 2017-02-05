

using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
    public sealed class ExacItem : SupplementaryDataItem
    {
        #region members

        private string ReferenceAllele { get; }
        private string AlternateAllele { get; }

        public int? AllAlleleCount { get; private set; }
        private int? AfrAlleleCount { get; set; }
        private int? AmrAlleleCount { get; set; }
        private int? EasAlleleCount { get; set; }
        private int? FinAlleleCount { get; set; }
        private int? NfeAlleleCount { get; set; }
        private int? OthAlleleCount { get; set; }
        private int? SasAlleleCount { get; set; }
        public int? AllAlleleNumber { get; private set; }
        private int? AfrAlleleNumber { get; set; }
        private int? AmrAlleleNumber { get; set; }
        private int? EasAlleleNumber { get; set; }
        private int? FinAlleleNumber { get; set; }
        private int? NfeAlleleNumber { get; set; }
        private int? OthAlleleNumber { get; set; }
        private int? SasAlleleNumber { get; set; }

        public int Coverage { get; }

        #endregion

        public ExacItem(string chromosome,
            int position,
            string refAllele,
            string alternateAllele,
            int coverage,
            int? allAlleleNumber, int? afrAlleleNumber, int? amrAlleleNumber, int? easAlleleNumber,
            int? finAlleleNumber, int? nfeAlleleNumber, int? othAlleleNumber, int? sasAlleleNumber, int? allAlleleCount,
            int? afrAlleleCount, int? amrAlleleCount, int? easAlleleCount, int? finAlleleCount, int? nfeAlleleCount,
            int? othAlleleCount, int? sasAlleleCount)
        {
            Chromosome = chromosome;
            Start = position;
            ReferenceAllele = refAllele;
            AlternateAllele = alternateAllele;

            Coverage = coverage;


            AllAlleleNumber = allAlleleNumber;
            AfrAlleleNumber = afrAlleleNumber;
            AmrAlleleNumber = amrAlleleNumber;
            EasAlleleNumber = easAlleleNumber;
            FinAlleleNumber = finAlleleNumber;
            NfeAlleleNumber = nfeAlleleNumber;
            OthAlleleNumber = othAlleleNumber;
            SasAlleleNumber = sasAlleleNumber;

            AllAlleleCount = allAlleleCount;
            AfrAlleleCount = afrAlleleCount;
            AmrAlleleCount = amrAlleleCount;
            EasAlleleCount = easAlleleCount;
            FinAlleleCount = finAlleleCount;
            NfeAlleleCount = nfeAlleleCount;
            OthAlleleCount = othAlleleCount;
            SasAlleleCount = sasAlleleCount;

            RemoveAlleleNumberZero();
        }

        private void RemoveAlleleNumberZero()
        {
            if (AllAlleleNumber == null || AllAlleleNumber.Value == 0)
            {
                AllAlleleNumber = null;
                AllAlleleCount = null;
            }

            if (AfrAlleleNumber == null || AfrAlleleNumber.Value == 0)
            {
                AfrAlleleNumber = null;
                AfrAlleleCount = null;
            }

            if (AmrAlleleNumber == null || AmrAlleleNumber.Value == 0)
            {
                AmrAlleleNumber = null;
                AmrAlleleCount = null;
            }

            if (EasAlleleNumber == null || EasAlleleNumber.Value == 0)
            {
                EasAlleleNumber = null;
                EasAlleleCount = null;
            }

            if (FinAlleleNumber == null || FinAlleleNumber.Value == 0)
            {
                FinAlleleNumber = null;
                FinAlleleCount = null;
            }

            if (NfeAlleleNumber == null || NfeAlleleNumber.Value == 0)
            {
                NfeAlleleNumber = null;
                NfeAlleleCount = null;
            }

            if (OthAlleleNumber == null || OthAlleleNumber.Value == 0)
            {
                OthAlleleNumber = null;
                OthAlleleCount = null;
            }

            if (SasAlleleNumber == null || SasAlleleNumber.Value == 0)
            {
                SasAlleleNumber = null;
                SasAlleleCount = null;
            }
        }

        public override SupplementaryDataItem SetSupplementaryAnnotations(SupplementaryPositionCreator sa, string refBases = null)
        {
            // check if the ref allele matches the refBases as a prefix
            if (!SupplementaryAnnotationUtilities.ValidateRefAllele(ReferenceAllele, refBases))
            {
                return null; //the ref allele for this entry did not match the reference bases.
            }

			var newAlleles = SupplementaryAnnotationUtilities.GetReducedAlleles(Start, ReferenceAllele, AlternateAllele);

			var newStart = newAlleles.Item1;
			var newRefAllele = newAlleles.Item2;
			var newAltAllele = newAlleles.Item3;

			if (newRefAllele != ReferenceAllele)
            {
                return new ExacItem(Chromosome, newStart, newRefAllele, newAltAllele, Coverage,
                    AllAlleleNumber, AfrAlleleNumber, AmrAlleleNumber, EasAlleleNumber, FinAlleleNumber, NfeAlleleNumber, OthAlleleNumber, SasAlleleNumber,
                    AllAlleleCount, AfrAlleleCount, AmrAlleleCount, EasAlleleCount, FinAlleleCount, NfeAlleleCount, OthAlleleCount, SasAlleleCount);
            }

            SetSaFields(sa, newAltAllele);

            return null;
        }

        public override SupplementaryInterval GetSupplementaryInterval(ChromosomeRenamer renamer)
        {
            throw new System.NotImplementedException();
        }

        private void SetSaFields(SupplementaryPositionCreator saCreator, string newAltAllele)
        {
			var annotation = new ExacAnnotation
			{
				ExacCoverage = Coverage,

				ExacAllAn = AllAlleleNumber,
				ExacAfrAn = AfrAlleleNumber,
				ExacAmrAn = AmrAlleleNumber,
				ExacEasAn = EasAlleleNumber,
				ExacFinAn = FinAlleleNumber,
				ExacNfeAn = NfeAlleleNumber,
				ExacOthAn = OthAlleleNumber,
				ExacSasAn = SasAlleleNumber,

				ExacAllAc = AllAlleleCount,
				ExacAfrAc = AfrAlleleCount,
				ExacAmrAc = AmrAlleleCount,
				ExacEasAc = EasAlleleCount,
				ExacFinAc = FinAlleleCount,
				ExacNfeAc = NfeAlleleCount,
				ExacOthAc = OthAlleleCount,
				ExacSasAc = SasAlleleCount
			};

			saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.Exac, newAltAllele, annotation);
		}

        // note that for an ExacItem, the chromosome, position and alt allele should uniquely identify it. If not, there is an error in the data source.
        public override bool Equals(object other)
        {
            // If parameter is null return false.

            // if other cannot be cast into OneKGenItem, return false
            var otherItem = other as ExacItem;
            if (otherItem == null) return false;

            // Return true if the fields match:
            return string.Equals(Chromosome, otherItem.Chromosome)
                && Start == otherItem.Start
                && AlternateAllele.Equals(otherItem.AlternateAllele)
                ;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Start.GetHashCode() ^ Chromosome.GetHashCode();
                hashCode = (hashCode * 397) ^ (AlternateAllele?.GetHashCode() ?? 0);

                return hashCode;
            }
        }
    }
}
