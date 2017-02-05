using System;
using System.IO;
using CacheUtils.CombineAndUpdateGenes.DataStructures;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling;

namespace CacheUtils.CombineAndUpdateGenes.FileHandling
{
    public class Gff3Reader : IDisposable
    {
        #region members

        private readonly StreamReader _reader;

        private const int FeatureTypeIndex = 2;
        private const int InfoIndex = 8;

        private const string GeneFeatureType = "gene";

        private const string DbxrefTag = "Dbxref";
        private const string NameTag   = "Name";
        private const string HgncTag   = "HGNC";
        private const string GeneIdTag = "GeneID";

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
        public Gff3Reader(string filePath)
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
            var line = _reader.ReadLine();
            if (line == null) return null;

            if (line.StartsWith("#")) return GeneInfo.Empty;

            var cols = line.Split('\t');
            if (cols.Length != 9) throw new GeneralException($"Expected 9 columns but found {cols.Length} when parsing the gene entry.");

            if (cols[FeatureTypeIndex] != GeneFeatureType || !cols[0].StartsWith("NC_")) return GeneInfo.Empty;

            try
            {
                var infoCols = cols[InfoIndex].Split(';');

                string entrezGeneId = null;
                string symbol       = null;
                int hgncId          = -1;

                foreach (var col in infoCols)
                {
                    var kvp   = col.Split('=');
                    var key   = kvp[0];
                    var value = kvp[1];

                    switch (key)
                    {
                        case DbxrefTag:
                            var ids     = value.Split(',');
                            hgncId       = GetHgncId(ids);
                            entrezGeneId = GetEntrezGeneId(ids);
                            break;
                        case NameTag:
                            symbol = value;
                            break;
                    }
                }

                if (string.IsNullOrEmpty(entrezGeneId) || string.IsNullOrEmpty(symbol))
                {
                    throw new UserErrorException(line);
                }

                return new GeneInfo { Symbol = symbol, HgncId = hgncId, EntrezGeneId = entrezGeneId };
            }
            catch (Exception)
            {
                Console.WriteLine();
                Console.WriteLine("Offending line: {0}", line);
                for (int i = 0; i < cols.Length; i++) Console.WriteLine("- col {0}: [{1}]", i, cols[i]);
                throw;
            }
        }

        private string GetEntrezGeneId(string[] ids)
        {
            foreach (var idPair in ids)
            {
                var cols = idPair.Split(':');
                if (cols[0] == GeneIdTag) return cols[1];
            }

            return null;
        }

        private int GetHgncId(string[] ids)
        {
            foreach (var idPair in ids)
            {
                var cols = idPair.Split(':');
                if (cols[0] == HgncTag) return int.Parse(cols[1]);
            }

            return -1;
        }
    }
}
