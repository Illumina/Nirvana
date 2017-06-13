using System.Collections.Generic;

namespace UpdateOmimGeneSymbols
{
    public class SymbolDataSource
    {
        private readonly Dictionary<string, UniqueString> _synonymToSymbol;

        /// <summary>
        /// constructor
        /// </summary>
        public SymbolDataSource(Dictionary<string, UniqueString> synonymToSymbol)
        {
            _synonymToSymbol = synonymToSymbol;
        }

        public bool TryUpdateSymbol(string currentSymbol, out string newSymbol)
        {
            newSymbol = currentSymbol;

            UniqueString symbol;
            if (!_synonymToSymbol.TryGetValue(currentSymbol, out symbol)) return false;

            if (symbol.HasConflict) return false;

            newSymbol = symbol.Value;
            return true;
        }
    }
}
