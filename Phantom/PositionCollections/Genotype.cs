using System;
using System.Linq;
using OptimizedCore;

namespace Phantom.PositionCollections
{
    public sealed class Genotype : IEquatable<Genotype>
    {
        public readonly int[] AlleleIndexes;
        public readonly bool IsPhased;
        public readonly bool IsHomozygous;

        private Genotype(int[] alleleIndexes, bool isPhased)
        {
            AlleleIndexes = alleleIndexes;
            IsPhased = isPhased;
            IsHomozygous = GetHomozygosity();
        }

        public static Genotype GetGenotype(string genotypeString)
        {
            char separator = GetGenotypeSeparator(genotypeString);
            var gtIndexStrings = genotypeString.OptimizedSplit(separator);
            var gtIndexes = new int[gtIndexStrings.Length];
            for (var i = 0; i < gtIndexStrings.Length; i++)
            {
                (int number, bool foundError) = gtIndexStrings[i].OptimizedParseInt32();
                gtIndexes[i] = foundError ? -1 : number;
            }
                
            return new Genotype(gtIndexes, separator == '|');
        }

        public static bool IsAllHomozygousReference(Genotype[] gtInfo, int startIndex, int numPositions)
        {
            for (int i = startIndex; i < startIndex + numPositions; i++)
            {
                if (!gtInfo[i].IsHomozygousReference()) return false;
            }
            return true;
        }

        private bool IsHomozygousReference() => IsHomozygous && AlleleIndexes[0] == 0;

        private bool GetHomozygosity()
        {
            for (var i = 1; i < AlleleIndexes.Length; i++)
            {
                if (AlleleIndexes[i] != AlleleIndexes[0]) return false;
            }
            return true;
        }

        private static char GetGenotypeSeparator(string genotypeString)
        {
            foreach (char c in genotypeString)
            {
                if (!char.IsDigit(c) && c != '.') return c;
            }
            // treat haplotype as phased
            return '|';
        }

        // used for recomposition purpose
        // for simplicity, 0/1 and 1/0 are considered different, as neither of them would be recomposed
        public bool Equals(Genotype other) => AlleleIndexes.SequenceEqual(other.AlleleIndexes) &&
                                              (IsPhased == other.IsPhased || IsHomozygous && other.IsHomozygous);

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = IsPhased ? 1 : 0;
                foreach (int genotype in AlleleIndexes)
                {
                    hashCode = (hashCode * 1201) ^ genotype.GetHashCode();
                }
                return hashCode;
            }
        }
    }
}