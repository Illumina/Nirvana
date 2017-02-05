using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
			if (string.IsNullOrEmpty(altAllele))
				return Tuple.Create(start, refAllele, refAllele.Length.ToString(CultureInfo.InvariantCulture));

			// when we have a supplementary annotation for the ref allele (as in clinVar sometimes), we should not apply any trimming or modification to the alleles.
			if (refAllele == altAllele)
				return Tuple.Create(start, refAllele, altAllele);

			// When we have a item that is derived from an entry, the alt alleles may have already been processed. We can detect the inserts and deletions and just return without any further processing. For MNVs, we have no way of detecting
			// we should also avoid any modifications for symbolic allele
			if (altAllele[0] == 'i' || altAllele[0] == '<' || char.IsDigit(altAllele[0]) || altAllele.All(x => x == 'N'))
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

		public static List<string> ConvertMixedFormatStrings(List<string> strings)
		{
			return strings?.Select(ConvertMixedFormatString).ToList();
		}

		public static string ConvertMixedFormatString(string s)
		{
			if (s == null) return null;

			// no hex characters to convert
			if (!s.Contains(@"\x")) return s;

			var sb = new StringBuilder();

			for (var i = 0; i < s.Length - 1; i++)
			{
				if (s[i] == '\\' && s[i + 1] == 'x')
				{
					var hexString = s.Substring(i + 2, 2);
					var value = Convert.ToInt32(hexString, 16);
					sb.Append(char.ConvertFromUtf32(value));
					i += 3;
				}
				else sb.Append(s[i]);
			}

			// the last char has to be added
			sb.Append(s[s.Length - 1]);
			return sb.ToString();
		}


	}
}