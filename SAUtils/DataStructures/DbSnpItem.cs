using VariantAnnotation.Interface.Sequence;

namespace SAUtils.DataStructures
{
	public sealed class DbSnpItem: SupplementaryDataItem
	{
		public long RsId { get; }
	    public double RefAlleleFreq { get; }
		public double AltAlleleFreq { get; }

	    public DbSnpItem(IChromosome chromosome,
			int position,
			long rsId,
			string refAllele,
			double refAlleleFreq,
			string alternateAllele,
			double altAlleleFreq)
		{
			Chromosome      = chromosome;
			Start           = position;
			RsId            = rsId;
			ReferenceAllele = refAllele;
			AlternateAllele = alternateAllele;
			RefAlleleFreq   = refAlleleFreq;
			AltAlleleFreq   = altAlleleFreq;
		}


		public override SupplementaryIntervalItem GetSupplementaryInterval()
		{
			throw new System.NotImplementedException();
		}



		public override bool Equals(object other)
		{
			// If parameter is null return false.

		    if (!(other is DbSnpItem otherItem)) return false;

			// Return true if the fields match:
			return Equals(Chromosome, otherItem.Chromosome)
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
