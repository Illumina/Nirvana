using System;

namespace VariantAnnotation.Algorithms.Consequences
{
    internal sealed class TempVariantEffectCache
    {
        #region members

        private readonly bool[] _isCached;
        private readonly bool[] _cachedResults;

        #endregion

        // constructor
        public TempVariantEffectCache()
        {
            var numConsequences = Enum.GetNames(typeof(TempConsequenceType)).Length;
            _isCached        = new bool[numConsequences];
            _cachedResults   = new bool[numConsequences];
        }

        /// <summary>
        /// returns true if the corresponding value has been cached
        /// </summary>
        public void Add(TempConsequenceType consequence, bool result)
        {
            var index = (int)consequence;

            _isCached[index]      = true;
            _cachedResults[index] = result;
        }

        /// <summary>
        /// returns the cached value for the corresponding result
        /// </summary>
        public bool Get(TempConsequenceType consequence)
        {
            return _cachedResults[(int)consequence];
        }

        /// <summary>
        /// returns true if the corresponding value has been cached
        /// </summary>
        public bool Contains(TempConsequenceType consequence)
        {
            return _isCached[(int)consequence];
        }
    }

    internal enum TempConsequenceType : byte
    {
        AfterCoding,
        BeforeCoding,
        EssentialSpliceSite,
        WithinCdna,
        WithinCds
    }
}
