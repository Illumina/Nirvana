using System.Text;

namespace VariantAnnotation.Algorithms
{
    /// <summary>
    /// This is an implementation of the FNV1a hash algorithm.
    /// 
    /// http://programmers.stackexchange.com/questions/49550/which-hashing-algorithm-is-best-for-uniqueness-and-speed
    /// http://www.isthe.com/chongo/tech/comp/fnv/index.html#FNV-1a
    /// </summary>
    public static class FowlerNollVoPrimeHash
    {
        // prime and offset basis are appropriate for 32-bit integers
        private const uint FnvOffsetBasis = 2166136261;
        private const uint FnvPrime       = 16777619;

        private static uint GenerateHash(byte[] bArray, uint hash)
        {
            foreach (var b in bArray)
            {
                hash ^= b;
                hash *= FnvPrime;
            }

            return hash;
        }

        public static int ComputeHash(string s)
        {
            var hash = FnvOffsetBasis;
            if (!string.IsNullOrEmpty(s)) hash = GenerateHash(Encoding.ASCII.GetBytes(s), FnvOffsetBasis);
            return (int)hash;
        }

        public static int ComputeHash(string s, string s2)
        {
            var hash = FnvOffsetBasis;
            if (!string.IsNullOrEmpty(s)) hash = GenerateHash(Encoding.ASCII.GetBytes(s), hash);
            if (!string.IsNullOrEmpty(s2)) hash = GenerateHash(Encoding.ASCII.GetBytes(s2), hash);
            return (int)hash;
        }
    }
}
