using System;
using System.Collections.Generic;
using System.IO;
using ErrorHandling.Exceptions;

namespace UpdateOmimGeneSymbols
{
    public sealed class GeneMap2Reader : IDisposable
    {
        #region members

        private readonly StreamReader _reader;
        public readonly List<string> HeaderLines = new List<string>();

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
        public GeneMap2Reader(Stream stream)
        {
            _reader = new StreamReader(stream);

            // skip the comments
            while (true)
            {
                var nextChar = (char)_reader.Peek();
                if (nextChar != '#') break;
                HeaderLines.Add(_reader.ReadLine());
            }
        }

        /// <summary>
        /// retrieves the next gene. Returns false if there are no more genes available
        /// </summary>
        public GeneMap2Entry Next()
        {
            // get the next line
            string line = _reader.ReadLine();
            if (string.IsNullOrEmpty(line) || line.StartsWith("#")) return null;

            var fields = line.Split('\t');
            if (fields.Length != 14)
                throw new InvalidFileFormatException(
                    $"Expected 14 columns but found {fields.Length} when parsing the genemap2 entry.");

            fields[GeneMap2Entry.GeneSymbolsIndex] = fields[GeneMap2Entry.GeneSymbolsIndex].ToUpper();
            return new GeneMap2Entry(fields);
        }
    }
}
