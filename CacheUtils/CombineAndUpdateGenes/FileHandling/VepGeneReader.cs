using System;
using System.IO;
using CacheUtils.CombineAndUpdateGenes.DataStructures;
using CacheUtils.CreateCache.FileHandling;
using CacheUtils.DataDumperImport.FileHandling;
using VariantAnnotation.DataStructures;
using ErrorHandling.Exceptions;
using VariantAnnotation.FileHandling.Compression;

namespace CacheUtils.CombineAndUpdateGenes.FileHandling
{
    public sealed class VepGeneReader : IVepReader<MutableGene>, IDisposable
    {
        #region members

        private readonly StreamReader _reader;
        public readonly GlobalImportHeader Header;

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
        public VepGeneReader(string filePath)
        {
            // sanity check
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The specified gene file ({filePath}) does not exist.");
            }

            // open the vcf file and parse the header
            _reader = GZipUtilities.GetAppropriateStreamReader(filePath);
            Header  = VepReaderCommon.GetHeader("gene", filePath, GlobalImportCommon.FileType.Gene, _reader);
        }

        /// <summary>
        /// retrieves the next gene. Returns false if there are no more genes available
        /// </summary>
        public MutableGene Next()
        {
            // get the next line
            string line = _reader.ReadLine();
            if (line == null) return null;

            var cols = line.Split('\t');
            if (cols.Length != 10) throw new GeneralException($"Expected 10 columns but found {cols.Length} when parsing the gene entry.");

            try
            {
                var referenceIndex  = ushort.Parse(cols[1]);
                var start           = int.Parse(cols[2]);
                var end             = int.Parse(cols[3]);
                var onReverseStrand = cols[4] == "R";
                var symbol          = cols[5];
                var hgnc            = cols[6];
                var entrezId        = CompactId.Convert(cols[7]);
                var ensemblId       = CompactId.Convert(cols[8]);
                var omimId          = int.Parse(cols[9]);

                var hgncId = hgnc == "" ? -1 : int.Parse(hgnc);

                var gene = new MutableGene
                {
                    ReferenceIndex       = referenceIndex,
                    Start                = start,
                    End                  = end,
                    OnReverseStrand      = onReverseStrand,
                    Symbol               = symbol,
                    HgncId               = hgncId,
                    EntrezGeneId         = entrezId,
                    EnsemblId            = ensemblId,
                    MimNumber            = omimId,
                    TranscriptDataSource = Header.TranscriptSource
                };

                return gene;
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