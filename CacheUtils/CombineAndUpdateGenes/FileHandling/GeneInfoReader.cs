using System;
using System.IO;
using CacheUtils.CombineAndUpdateGenes.DataStructures;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling.Compression;

namespace CacheUtils.CombineAndUpdateGenes.FileHandling
{
    public sealed class GeneInfoReader : IDisposable
    {
        #region members

        private readonly StreamReader _reader;

        private int _entrezGeneIndex = -1;
        private int _symbolIndex     = -1;
        private int _dbXrefsIndex    = -1;
        private int _hgncSymbolIndex = -1;

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
                    case "GeneID":
                        _entrezGeneIndex = index;
                        break;
                    case "Symbol":
                        _symbolIndex = index;
                        break;
                    case "dbXrefs":
                        _dbXrefsIndex = index;
                        break;
                    case "Symbol_from_nomenclature_authority":
                        _hgncSymbolIndex = index;
                        break;
                }
            }

            if (_entrezGeneIndex == -1 || _symbolIndex == -1 || _dbXrefsIndex == -1 || _hgncSymbolIndex == -1)
            {
                Console.WriteLine("_entrezGeneIndex: {0}", _entrezGeneIndex);
                Console.WriteLine("_symbolIndex:     {0}", _symbolIndex);
                Console.WriteLine("_dbXrefsIndex:    {0}", _dbXrefsIndex);
                Console.WriteLine("_hgncSymbolIndex: {0}", _hgncSymbolIndex);
                throw new UserErrorException("Not all of the indices were set.");
            }
        }

        /// <summary>
        /// retrieves the next gene. Returns false if there are no more genes available
        /// </summary>
        public GeneInfo Next()
        {
            // get the next line
            string line = _reader.ReadLine();
            if (line == null) return null;

            if(!line.StartsWith("9606")) return GeneInfo.Empty;

            var cols = line.Split('\t');
            if (cols.Length != 15) throw new GeneralException($"Expected 15 columns but found {cols.Length} when parsing the gene entry.");

            try
            {
                var entrezGeneId = cols[_entrezGeneIndex];
                var ncbiSymbol   = cols[_symbolIndex];
                var hgncSymbol   = cols[_hgncSymbolIndex];

                var symbol = hgncSymbol == "-" ? ncbiSymbol : hgncSymbol;

                string ensemblId = null;
                int hgncId       = -1;

                var xrefs = cols[_dbXrefsIndex].Split('|');
                foreach (var xref in xrefs)
                {
                    if (xref.StartsWith("Ensembl:")) ensemblId = xref.Substring(8);
                    if (xref.StartsWith("HGNC:HGNC:")) hgncId = int.Parse(xref.Substring(10));
                }

                return new GeneInfo
                {
                    Symbol       = symbol,
                    HgncId       = hgncId,
                    EnsemblId    = ensemblId,
                    EntrezGeneId = entrezGeneId
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
