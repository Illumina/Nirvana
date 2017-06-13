using System;
using System.Globalization;
using System.Linq;
using VariantAnnotation.Algorithms;

namespace VariantAnnotation.DataStructures.SupplementaryAnnotations
{
	public static class SupplementaryAnnotationUtilities
	{
		/// <summary>
		/// Returns a regular alternate allele when a provided with one have SA format.
		/// In case of long insertions or InsDel, where the saAltAllele contains an MD5 hash, the hash is returned.
		/// </summary>
		/// <param name="saAltAllele"> supplementary annotation alternate allele</param>
		/// <param name="emptyAllele">The way the calling function wants to represent an empty allele</param>
		/// <returns>regular alternate allele</returns>
		public static string ReverseSaReducedAllele(string saAltAllele, string emptyAllele = "-")
		{
			if (saAltAllele == null) return null;
			if (saAltAllele.All(char.IsDigit)) return emptyAllele; // this was a deletion

			int firstBaseIndex;
			for (firstBaseIndex = 0; firstBaseIndex < saAltAllele.Length; firstBaseIndex++)
			{
				if (saAltAllele[firstBaseIndex] != 'i' && saAltAllele[firstBaseIndex] != '<' &&
					!char.IsDigit(saAltAllele[firstBaseIndex]))
					break;
			}

			if (saAltAllele.Substring(firstBaseIndex) == "") return emptyAllele;

			return firstBaseIndex > 0 && firstBaseIndex < saAltAllele.Length ? saAltAllele.Substring(firstBaseIndex) : saAltAllele;
		}

		public static bool ValidateRefAllele(string refAllele, string refBases)
		{
			if (refBases == null) return true;
			if (refAllele == ".") return true;//ref base is unknown
			if (refBases.All(x => x == 'N')) return true;

			if (refAllele.Length < refBases.Length)
				return refBases.StartsWith(refAllele);

			// in rare cases the refAllele will be too large for our refBases string that is limited in length
			return refAllele.StartsWith(refBases);
		}

		/// <summary>
		/// Given a ref and alt allele string, return their trimmed version. 
		/// This method should be decommissioned once VariantAlternateAlleles are used in SA.
		/// </summary>
		/// <param name="start"></param>
		/// <param name="refAllele"> The reference allele string </param>
		/// <param name="altAllele">The alternate allele string</param>
		/// <returns>Trimmed position, reference allele and reduced alternate allele as expected by SA</returns>
		public static Tuple<int, string, string> GetReducedAlleles(int start, string refAllele, string altAllele)
		{
            // we have a deletion
            if (refAllele == "-") refAllele = "";
            if (altAllele == "-") altAllele = "";
			if (! NeedsReduction(refAllele, altAllele))
				return Tuple.Create(start, refAllele, altAllele);

			var trimmedTuple = BiDirectionalTrimmer.Trim(start, refAllele, altAllele);

			start = trimmedTuple.Item1;
			refAllele = trimmedTuple.Item2;
			altAllele = trimmedTuple.Item3;

			// we have detected a deletion after trimming
			if (string.IsNullOrEmpty(altAllele))
				return Tuple.Create(start, refAllele, refAllele.Length.ToString(CultureInfo.InvariantCulture));

			// we have an insertion and we indicate that with an i at the beginning
			if (string.IsNullOrEmpty(refAllele))
				return Tuple.Create(start, refAllele, 'i' + altAllele);

			if (refAllele.Length == altAllele.Length) //SNV or CNV
				return Tuple.Create(start, refAllele, altAllele);

			// its a delins 
			altAllele = refAllele.Length.ToString(CultureInfo.InvariantCulture) + altAllele;

			return Tuple.Create(start, refAllele, altAllele);
		}

		private static bool NeedsReduction(string refAllele, string altAllele)
		{
			if (string.IsNullOrEmpty(altAllele)) return true;

			if (!string.IsNullOrEmpty(refAllele) && altAllele.All(x => x == 'N')) return false;

			return !(altAllele[0] == 'i' || altAllele[0] == '<' || char.IsDigit(altAllele[0])) ;
		}

		public static string ConvertToVcfInfoString(string s)
		{
			//characters such as comma, space, etc. are not allowed in vcfinfo strings.
			s = s.Replace(" ", "_");
			return s.Replace(",", "\\x2c");
		}
	}
}