using System;
using System.IO;
using CacheUtils.CombineAndUpdateGenes.DataStructures;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling.Compression;

namespace CacheUtils.CombineAndUpdateGenes.FileHandling
{
    public sealed class HgncReader : IDisposable
    {
        #region members

        private readonly StreamReader _reader;

        private const int HgncIdIndex    = 0;
        private const int SymbolIndex    = 1;
        private const int EntrezIdIndex  = 18;
        private const int EnsemblIdIndex = 19;

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
            _reader.ReadLine();
        }

        /// <summary>
        /// retrieves the next gene. Returns false if there are no more genes available
        /// </summary>
        public GeneInfo Next()
        {
            // get the next line
            string line = _reader.ReadLine();
            if (line == null) return null;

            var cols = line.Split('\t');
            if (cols.Length != 48) throw new GeneralException($"Expected 48 columns but found {cols.Length} when parsing the gene entry.");

            try
            {
                var hgncId    = int.Parse(cols[HgncIdIndex].Substring(5));
                var symbol    = cols[SymbolIndex];
                var entrezId  = string.IsNullOrEmpty(cols[EntrezIdIndex])  ? null : cols[EntrezIdIndex];
                var ensemblId = string.IsNullOrEmpty(cols[EnsemblIdIndex]) ? null : cols[EnsemblIdIndex];

                return new GeneInfo
                {
                    Symbol       = symbol,
                    HgncId       = hgncId,
                    EnsemblId    = ensemblId,
                    EntrezGeneId = entrezId
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
