using System;
using System.IO;
using System.Linq;
using ErrorHandling.Exceptions;
using Compression.Utilities;

namespace SAUtils.CreateOmimTsv
{
    public sealed class GeneInfoReader : IDisposable
    {
        #region members

        private readonly StreamReader _reader;

        private int _symbolIndex   = -1;
        private int _synonymsIndex = -1;

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
        public GeneInfoReader(string filePath)
        {
            // sanity check
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified gene_info file ({filePath}) does not exist.");
            }

            // open the file and parse the header
            _reader = GZipUtilities.GetAppropriateStreamReader(filePath);
            var headerLine = _reader.ReadLine();

            SetColumnIndices(headerLine);
        }

        private void SetColumnIndices(string line)
        {
            if (line.StartsWith("#Format: ")) line = line.Substring(9);
            if (line.StartsWith("#"))         line = line.Substring(1);

            var cols = line.Split('\t');
            if (cols.Length == 1) cols = line.Split(' ');

            for (int index = 0; index < cols.Length; index++)
            {
                var header = cols[index];
                switch (header)
                {
                    case "Symbol":
                        _symbolIndex = index;
                        break;
                    case "Synonyms":
                        _synonymsIndex = index;
                        break;
                }
            }

            if (_symbolIndex == -1 || _synonymsIndex == -1)
            {
                Console.WriteLine("_symbolIndex:   {0}", _symbolIndex);
                Console.WriteLine("_synonymsIndex: {0}", _synonymsIndex);
                throw new UserErrorException("Not all of the indices were set.");
            }
        }

        /// <summary>
        /// retrieves the next gene. Returns false if there are no more genes available
        /// </summary>
        public GeneSymbolSynonyms Next()
        {
            // get the next line
            string line = _reader.ReadLine();
            if (line == null) return null;

            if (!line.StartsWith("9606")) return GeneSymbolSynonyms.Empty;

            var cols = line.Split('\t');
            if (cols.Length != 15) throw new UserErrorException($"Expected 15 columns but found {cols.Length} when parsing the gene entry.");

            try
            {
                var geneSymbol = cols[_symbolIndex];
                var synonyms   = cols[_synonymsIndex].Split('|').ToList();

                return new GeneSymbolSynonyms
                {
                    GeneSymbol = geneSymbol,
                    Synonyms   = synonyms
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
