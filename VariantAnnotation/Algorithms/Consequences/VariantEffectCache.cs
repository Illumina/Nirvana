using System;

namespace VariantAnnotation.Algorithms.Consequences
{
    internal sealed class VariantEffectCache
    {
        #region members

        private readonly bool[] _isCached;
        private readonly bool[] _cachedResults;

        #endregion

        // constructor
        public VariantEffectCache()
        {
            var numConsequences = Enum.GetNames(typeof(ConsequenceType)).Length;
            _isCached        = new bool[numConsequences];
            _cachedResults   = new bool[numConsequences];
        }

        /// <summary>
        /// returns true if the corresponding value has been cached
        /// </summary>
        public void Add(ConsequenceType consequence, bool result)
        {
            var index = (int)consequence;

            _isCached[index]      = true;
            _cachedResults[index] = result;
        }

        /// <summary>
        /// returns the cached value for the corresponding result
        /// </summary>
        public bool Get(ConsequenceType consequence)
        {
            return _cachedResults[(int)consequence];
        }

        /// <summary>
        /// returns true if the corresponding value has been cached
        /// </summary>
        public bool Contains(ConsequenceType consequence)
        {
            return _isCached[(int)consequence];
        }
    }
}
