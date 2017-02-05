using System.Collections.Generic;
using VariantAnnotation.Utilities;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public sealed class DbSnpItem: SupplementaryDataItem
	{
		public long RsId { get; }
	    private string ReferenceAllele { get; }
	    private string AlternateAllele { get; }
	    private double RefAlleleFreq { get; }
		public double AltAlleleFreq { get; }
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
			Chromosome      = chromosome;
			Start           = position;
			RsId            = rsId;
			ReferenceAllele = refAllele;
			AlternateAllele = alternateAllele;
			RefAlleleFreq   = refAlleleFreq;
			AltAlleleFreq   = altAlleleFreq;
			_infoField      = infoField;
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

		public override SupplementaryInterval GetSupplementaryInterval(ChromosomeRenamer renamer)
		{
			throw new System.NotImplementedException();
		}

		private void SetSaFields(SupplementaryPositionCreator saCreator, string newAltAllele)
		{

			if (!RefAlleleFreq.Equals(double.MinValue))
			{
				saCreator.RefAllele = ReferenceAllele;
				saCreator.RefAlleleFreq = RefAlleleFreq;
			}
			//var sa = saCreator.SaPosition;

			//set asa field
			var annotation = new DbSnpAnnotation
			{
				DbSnp = new List<long> { RsId },
				AltAlleleFreq = AltAlleleFreq
			};

			saCreator.AddExternalDataToAsa(DataSourceCommon.DataSource.DbSnp,newAltAllele,annotation);

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
