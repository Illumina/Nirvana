using System;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using Compression.Utilities;

namespace SAUtils.CreateOmimTsv
{
    public sealed class HgncReader : IDisposable
    {
        #region members

        private readonly StreamReader _reader;

        private const int SymbolIndex         = 1;
        private const int PreviousSymbolIndex = 10;

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                _reader.Dispose();
            }
        }

        #endregion

        /// <summary>
        /// constructor
        /// </summary>
        public HgncReader(string filePath)
        {
            // sanity check
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified gene_info file ({filePath}) does not exist.");
            }

            // open the file and parse the header
            _reader = GZipUtilities.GetAppropriateStreamReader(filePath);
            var line = _reader.ReadLine();
            var cols = line.Split('\t');

            const string symbolHeader = "symbol";
            if (cols[SymbolIndex] != symbolHeader) throw new InvalidFileFormatException($"Expected column index {SymbolIndex} to contain {symbolHeader}, but found {cols[SymbolIndex]}");

            const string prevSymbolHeader = "prev_symbol";
            if (cols[PreviousSymbolIndex] != prevSymbolHeader) throw new InvalidFileFormatException($"Expected column index {PreviousSymbolIndex} to contain {prevSymbolHeader}, but found {cols[PreviousSymbolIndex]}");
        }

        /// <summary>
        /// retrieves the next gene. Returns false if there are no more genes available
        /// </summary>
        public GeneSymbolSynonyms Next()
        {
            // get the next line
            string line = _reader.ReadLine();
            if (line == null) return null;

            var cols = line.Split('\t');
            if (cols.Length < 11) throw new UserErrorException($"Expected more than 11 columns but found only {cols.Length} when parsing the gene entry.");

            try
            {
                var geneSymbol = cols[SymbolIndex];
                var synonyms   = cols[PreviousSymbolIndex].Split('|').ToList();

                return new GeneSymbolSynonyms
                {
                    GeneSymbol = geneSymbol,
                    Synonyms   = synonyms,
                };
            }
            catch (Exception)
            {
                Console.WriteLine("Offending line: {0}", line);
                for (int i = 0; i < cols.Length; i++) Console.WriteLine("- col {0}: [{1}]", i, cols[i]);
                throw;
            }
        }
    }
}
