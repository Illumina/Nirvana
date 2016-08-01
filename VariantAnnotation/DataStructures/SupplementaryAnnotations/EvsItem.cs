namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public class EvsItem:SupplementaryDataItem
	{
		#region members

	    private string RsId { get; }
	    private string ReferenceAllele { get; }
	    private string AlternateAllele { get; }

	    private string AfrFreq { get; }
	    private string AllFreq { get; }
	    private string EurFreq { get; }
	    private string Coverage { get; }
	    private string NumSamples { get; }

		#endregion

		public EvsItem(string chromosome,
			int position,
			string rsId,
			string refAllele,
			string alternateAllele,
			string allFreq,
			string afrFreq,
			string eurFreq,
			string coverage,
			string numSamples
			)
		{
			Chromosome = chromosome;
			Start = position;
			RsId = rsId;
			ReferenceAllele = refAllele;
			AlternateAllele = alternateAllele;
			AfrFreq = afrFreq;
			EurFreq = eurFreq;
			AllFreq = allFreq;
			NumSamples = numSamples;
			Coverage = coverage;

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
				return new EvsItem(Chromosome, newStart, RsId, newRefAllele, newAltAllele, AllFreq, AfrFreq, EurFreq, Coverage, NumSamples);
			}

			SetSaFields(sa, newAltAllele);

			return null;
		}

		public override SupplementaryInterval GetSupplementaryInterval()
		{
			throw new System.NotImplementedException();
		}

		private void SetSaFields(SupplementaryAnnotation sa, string newAltAllele)
		{
			var asa = new SupplementaryAnnotation.AlleleSpecificAnnotation
			{
				EvsAll = AllFreq,
				EvsAfr = AfrFreq,
				EvsEur = EurFreq,
				EvsCoverage = Coverage,
				NumEvsSamples = NumSamples
			};


			if (!sa.AlleleSpecificAnnotations.ContainsKey(newAltAllele))
				sa.AlleleSpecificAnnotations[newAltAllele] = asa;
			else
				sa.AlleleSpecificAnnotations[newAltAllele].MergeEvsAnnotations(asa);

		}


		public override bool Equals(object other)
		{
			// If parameter is null return false.

			// if other cannot be cast into OneKGenItem, return false
			var otherItem = other as EvsItem;
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
				var hashCode = RsId?.GetHashCode() ?? 0;
				hashCode = (hashCode * 397) ^ (ReferenceAllele?.GetHashCode() ?? 0);
				hashCode = (hashCode * 397) ^ (AlternateAllele?.GetHashCode() ?? 0);

				return hashCode;
			}
		}
	}
}
