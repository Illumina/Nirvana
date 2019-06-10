using System.Collections.Generic;
using System.Linq;

namespace Genome
{
	public static class SequenceUtilities
	{
		#region members

		private static readonly char[] ReverseComplementLookupTable;
		private static readonly HashSet<char> CanonicalBases;

		#endregion

		static SequenceUtilities()
		{
			// initialize the reverse complement code
			const string forwardBases = "ABCDGHKMRTVYabcdghkmrtvy";
			const string reverseBases = "TVGHCDMKYABRTVGHCDMKYABR";
			ReverseComplementLookupTable = new char[256];

			for (var i = 0; i < 256; i++) ReverseComplementLookupTable[i] = 'N';
			for (var i = 0; i < forwardBases.Length; i++)
			{
				ReverseComplementLookupTable[forwardBases[i]] = reverseBases[i];
			}

			CanonicalBases = new HashSet<char> { 'A', 'C', 'G', 'T', '-' };
		}

		/// <summary>
		/// returns the reverse complement of the given bases
		/// </summary>
		public static string GetReverseComplement(string bases)
		{
			// sanity check
			if (bases == null) return null;

			int numBases = bases.Length;
			var reverseChars = new char[numBases];

			for (var i = 0; i < numBases; ++i)
			{
				reverseChars[i] = ReverseComplementLookupTable[bases[numBases - i - 1]];
			}

			return new string(reverseChars);
		}

		/// <summary>
		/// returns true if we have a base other than the 4 standard bases: A, C, G, and T
		/// </summary>
		public static bool HasNonCanonicalBase(string bases)
		{
		    return !string.IsNullOrEmpty(bases) && bases.Any(c => !CanonicalBases.Contains(c));
		}
    }
}