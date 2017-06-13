using System.Linq;
using System.Text.RegularExpressions;

namespace UpdateOmimGeneSymbols
{
    public class GeneMap2Entry
    {
        internal const int GeneSymbolsIndex = 6;

        private readonly string[] _fields;
        public readonly string[] GeneSymbols;

        /// <summary>
        /// constructor
        /// </summary>
        public GeneMap2Entry(string[] fields)
        {
            _fields     = fields;
            GeneSymbols = Regex.Split(fields[GeneSymbolsIndex], ", ");
        }

        public override string ToString()
        {
            var fields = (string[])_fields.Clone();
            fields[GeneSymbolsIndex] = string.Join(", ", GeneSymbols.Distinct());
            return string.Join("\t", fields);
        }
    }
}
